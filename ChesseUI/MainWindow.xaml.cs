using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation; // For Storyboard, DoubleAnimation, QuadraticEase
using System.Windows.Threading;      // For DispatcherTimer
using Chesselogique;
using ChesseAI; // Use your namespace for the ChessAI

namespace ChesseUI
{
    // Distinguish human vs. computer players
    public enum PlayerType
    {
        Human,
        Computer
    }

    public partial class MainWindow : Window
    {
        // UI arrays for pieces and highlights
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];

        // Dictionary to map "toPositions" to Moves
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();

        // Core game state + selected square
        private GameState gameState;
        private Position selectedPosition = null;

        // Define which side is Human or Computer
        private PlayerType whitePlayerType = PlayerType.Human;
        private PlayerType blackPlayerType = PlayerType.Computer;

        // AI difficulty
        private ChessAI.AIDifficulty currentDifficulty = ChessAI.AIDifficulty.Medium;

        public MainWindow()
        {
            InitializeComponent();
            // Start the game: White to move, fresh initial board
            gameState = new GameState(Player.White, Board.Initial());
            Loaded += Window_Loaded;
        }

        private void AnimatePieceMove(Image pieceImage, Position from, Position to)
        {
            // Calculate the target position in pixels
            double squareSize = BoardGrid.ActualWidth / 8;
            double targetX = to.Column * squareSize;
            double targetY = to.Row * squareSize;

            // Create a TranslateTransform for the piece
            var transform = new TranslateTransform();
            pieceImage.RenderTransform = transform;

            // Create a Storyboard to animate the piece
            var storyboard = new Storyboard();

            // Animate the X property
            var xAnimation = new DoubleAnimation
            {
                To = targetX - (from.Column * squareSize),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(xAnimation, transform);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath(TranslateTransform.XProperty));
            storyboard.Children.Add(xAnimation);

            // Animate the Y property
            var yAnimation = new DoubleAnimation
            {
                To = targetY - (from.Row * squareSize),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(yAnimation, transform);
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath(TranslateTransform.YProperty));
            storyboard.Children.Add(yAnimation);

            // Start the animation
            storyboard.Begin();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeBoard();
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }

        private void InitializeBoard()
        {
            // Create 8x8 Image + 8x8 Rect for each square
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var image = new Image();
                    pieceImages[row, col] = image;
                    PieceGrid.Children.Add(image);

                    var highlight = new Rectangle();
                    highlights[row, col] = highlight;
                    HighlightGrid.Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board[row, col];
                    pieceImages[row, col].Source = Images.GetImage(piece);
                }
            }
        }

        // Mouse click event on the chessboard
        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If a menu is on screen (promotion or game over), ignore
            if (IsMenuOnScreen())
            {
                return;
            }

            // If it's AI's turn, ignore mouse input
            if (IsAIPlayer(gameState.CurrentPlayer))
            {
                return;
            }

            // Calculate which square was clicked
            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if (selectedPosition == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }

        private void OnFromPositionSelected(Position pos)
        {
            // Retrieve moves if there's a piece for the current player
            IEnumerable<Move> moves = gameState.LegalMovesForPiece(pos);
            if (moves.Any())
            {
                selectedPosition = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            selectedPosition = null;
            HideHighlights();

            // If there's a valid move in moveCache
            if (moveCache.TryGetValue(pos, out Move move))
            {
                // If it's a pawn promotion
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
            }
        }

        private void HandlePromotion(Position from, Position to)
        {
            // Show a placeholder pawn image
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;

            // Display the PromotionMenu overlay
            PromotionMenu promMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = promMenu;

            // Once user selects a piece
            promMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);
            };
        }

        private void HandleMove(Move move)
        {
            // Get the piece image at the "from" position
            Image pieceImage = pieceImages[move.FromPos.Row, move.FromPos.Column];

            // Animate the piece move
            AnimatePieceMove(pieceImage, move.FromPos, move.ToPos);

            // Execute the move on gameState after the animation completes
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.3) // Match the animation duration
            };
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                gameState.MakeMove(move);
                DrawBoard(gameState.Board);
                SetCursor(gameState.CurrentPlayer);
                moveCache.Clear();

                // Check if game ended
                if (gameState.IsGameOver())
                {
                    ShowGameOver();
                }
                else
                {
                    // If it's an AI player's turn, let AI move
                    if (IsAIPlayer(gameState.CurrentPlayer))
                    {
                        MakeAIMove();
                    }
                }
            };
            timer.Start();
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {
            Color color = Color.FromArgb(150, 125, 255, 125);
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }
        }

        private void HideHighlights()
        {
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private void SetCursor(Player player)
        {
            // If you have custom cursors for White/Black
            if (player == Player.White)
            {
                Cursor = CheesCursors.WhiteCursor;
            }
            else
            {
                Cursor = CheesCursors.BlackCursor;
            }
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }

        private void ShowGameOver()
        {
            var gameOverMenu = new GameOverMenu(gameState);
            MenuContainer.Content = gameOverMenu;

            gameOverMenu.OptionSelected += option =>
            {
                if (option == Option.Restart)
                {
                    MenuContainer.Content = null;
                    RestartGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };
        }

        private void RestartGame()
        {
            HideHighlights();
            moveCache.Clear();
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }

        //--------------------------------------
        // AI LOGIC
        //--------------------------------------

        // Tells us if the given player is the AI
        private bool IsAIPlayer(Player player)
        {
            if (player == Player.White)
                return (whitePlayerType == PlayerType.Computer);
            else
                return (blackPlayerType == PlayerType.Computer);
        }

        // Let the AI choose and execute its move
        private void MakeAIMove()
        {
            var ai = new ChessAI();
            int depth = (int)currentDifficulty;

            // Define the maximum time limit (e.g., 5 seconds)
            TimeSpan maxTime = TimeSpan.FromSeconds(5);

            // Call ComputeBestMove with the required parameters
            Move bestMove = ai.ComputeBestMove(gameState, depth, maxTime);

            if (bestMove == null)
            {
                // Possibly stalemate
                return;
            }

            gameState.MakeMove(bestMove);
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
            moveCache.Clear();

            // Check if AI move ended the game
            if (gameState.IsGameOver())
            {
                ShowGameOver();
            }
        }

        //--------------------------------------
        // COMBOBOX EVENTS
        //--------------------------------------

        // White player dropdown
        private void WhitePlayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WhitePlayerComboBox.SelectedItem is ComboBoxItem item)
            {
                string choice = item.Content.ToString();
                if (choice == "Human")
                    whitePlayerType = PlayerType.Human;
                else
                    whitePlayerType = PlayerType.Computer;
            }
        }

        // Black player dropdown
        private void BlackPlayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BlackPlayerComboBox.SelectedItem is ComboBoxItem item)
            {
                string choice = item.Content.ToString();
                if (choice == "Human")
                    blackPlayerType = PlayerType.Human;
                else
                    blackPlayerType = PlayerType.Computer;
            }
        }

        // Difficulty dropdown
        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DifficultyComboBox.SelectedItem is ComboBoxItem item)
            {
                switch (item.Content.ToString())
                {
                    case "Easy":
                        currentDifficulty = ChessAI.AIDifficulty.Easy;
                        break;
                    case "Medium":
                        currentDifficulty = ChessAI.AIDifficulty.Medium;
                        break;
                    case "Hard":
                        currentDifficulty = ChessAI.AIDifficulty.Hard;
                        break;
                    case "Expert":
                        currentDifficulty = ChessAI.AIDifficulty.Expert;
                        break;
                }
            }
        }
    }
}
