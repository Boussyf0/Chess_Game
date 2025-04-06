using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chesselogique;

namespace ChesseAI
{
    public class ChessAI
    {
        // Transposition table to cache evaluated positions (thread-safe)
        private readonly ConcurrentDictionary<ulong, TranspositionEntry> transpositionTable = new ConcurrentDictionary<ulong, TranspositionEntry>();
        private readonly ConcurrentQueue<ulong> transpositionQueue = new ConcurrentQueue<ulong>();
        private const int TranspositionTableSize = 1000000; // Example size limit

        // Debug mode flag
        public bool DebugMode { get; set; } = true;

        // Difficulty levels
        public enum AIDifficulty
        {
            Easy = 1,      // Depth 1
            Medium = 3,    // Depth 3
            Hard = 5,      // Depth 5
            Expert = 7     // Depth 7
        }

        // Piece-square tables for evaluation
        private readonly int[,] pawnTable = {
            { 0,  0,  0,  0,  0,  0,  0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            { 5,  5, 10, 25, 25, 10,  5,  5 },
            { 0,  0,  0, 20, 20,  0,  0,  0 },
            { 5, -5,-10,  0,  0,-10, -5,  5 },
            { 5, 10, 10,-20,-20, 10, 10,  5 },
            { 0,  0,  0,  0,  0,  0,  0,  0 }
        };

        private readonly int[,] knightTable = {
            { -50,-40,-30,-30,-30,-30,-40,-50 },
            { -40,-20,  0,  0,  0,  0,-20,-40 },
            { -30,  0, 10, 15, 15, 10,  0,-30 },
            { -30,  5, 15, 20, 20, 15,  5,-30 },
            { -30,  0, 15, 20, 20, 15,  0,-30 },
            { -30,  5, 10, 15, 15, 10,  5,-30 },
            { -40,-20,  0,  5,  5,  0,-20,-40 },
            { -50,-40,-30,-30,-30,-30,-40,-50 }
        };

        private readonly int[,] bishopTable = {
            { -20,-10,-10,-10,-10,-10,-10,-20 },
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0, 5, 10, 10, 5,  0,-10 },
            { -10,  5, 5, 10, 10, 5,  5,-10 },
            { -10,  0, 10, 10, 10, 10,  0,-10 },
            { -10, 10, 10, 10, 10, 10, 10,-10 },
            { -10,  5,  0,  0,  0,  0,  5,-10 },
            { -20,-10,-10,-10,-10,-10,-10,-20 }
        };

        private readonly int[,] rookTable = {
            { 0,  0,  0,  0,  0,  0,  0,  0 },
            { 5, 10, 10, 10, 10, 10, 10,  5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { 0,  0,  0,  5,  5,  0,  0,  0 }
        };

        private readonly int[,] queenTable = {
            { -20,-10,-10, -5, -5,-10,-10,-20 },
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0, 5, 5, 5, 5,  0,-10 },
            { -5,  0, 5, 5, 5, 5,  0, -5 },
            { 0,  0, 5, 5, 5, 5,  0, -5 },
            { -10, 5, 5, 5, 5, 5,  0,-10 },
            { -10,  0, 5,  0,  0,  0,  0,-10 },
            { -20,-10,-10, -5, -5,-10,-10,-20 }
        };

        private readonly int[,] kingTable = {
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -20,-30,-30,-40,-40,-30,-30,-20 },
            { -10,-20,-20,-20,-20,-20,-20,-10 },
            { 20, 20,  0,  0,  0,  0, 20, 20 },
            { 20, 30, 10,  0,  0, 10, 30, 20 }
        };

        // Configuration for piece values and penalties
        public static class Config
        {
            public const int PawnValue = 100;
            public const int KnightValue = 300;
            public const int BishopValue = 300;
            public const int RookValue = 500;
            public const int QueenValue = 900;
            public const int KingValue = 20000;

            public const int CheckBonus = 50;
            public const int ExposedKingPenalty = -50;
            public const int PassedPawnBonus = 20;
            public const int IsolatedPawnPenalty = -10;
            public const int MobilityWeight = 5; // Weight for mobility evaluation
            public const int CenterControlWeight = 10; // Weight for center control evaluation
        }

        // Transposition table entry
        private struct TranspositionEntry
        {
            public int Score { get; set; }
            public int Depth { get; set; }
            public ScoreType Type { get; set; }
        }

        private enum ScoreType
        {
            Exact,
            LowerBound,
            UpperBound
        }

        // Time management
        private DateTime startTime;
        private TimeSpan maxTime;

        // Learning mechanism: Use ConcurrentDictionary for thread-safe operations
        private readonly ConcurrentDictionary<ulong, double> learnedEvaluations = new ConcurrentDictionary<ulong, double>();

        /// <summary>
        /// Main entry point for AI to compute the best move.
        /// </summary>
        public Move ComputeBestMove(GameState gameState, int maxDepth, TimeSpan maxTime)
        {
            this.startTime = DateTime.Now;
            this.maxTime = maxTime;

            Move bestMove = null;

            // Iterative Deepening: Start with depth 1 and increase until maxDepth or time runs out
            for (int depth = 1; depth <= maxDepth; depth++)
            {
                if (DateTime.Now - startTime >= maxTime)
                {
                    Log($"Time limit reached. Stopping at depth {depth - 1}.");
                    break;
                }

                Log($"Searching at depth {depth}...");
                Move currentBestMove = ComputeBestMoveAtDepth(gameState, depth);

                if (currentBestMove != null)
                {
                    bestMove = currentBestMove;
                    Log($"New best move at depth {depth}: {bestMove.From} to {bestMove.To}");
                }
            }

            Log($"Final best move: {bestMove?.From} to {bestMove?.To}");
            return bestMove;
        }

        /// <summary>
        /// Computes the best move at a specific depth using Minimax with Alpha-Beta pruning.
        /// </summary>
        private Move ComputeBestMoveAtDepth(GameState gameState, int depth)
        {
            Player aiPlayer = gameState.CurrentPlayer;

            // Generate and order moves
            IEnumerable<Move> moves = OrderMoves(gameState.AllLegalMovesFor(aiPlayer), gameState, aiPlayer);

            if (!moves.Any())
            {
                Log("No legal moves available.");
                return null; // No legal moves
            }

            Move bestMove = null;
            int bestScore = int.MinValue;
            object lockObj = new object();

            // Parallelize the search for moves at the root level
            Parallel.ForEach(moves, move =>
            {
                try
                {
                    GameState copyState = new GameState(gameState.CurrentPlayer, gameState.Board.Copy());
                    copyState.MakeMove(move);

                    int score = Minimax(copyState, depth - 1, int.MinValue, int.MaxValue, false, aiPlayer);

                    lock (lockObj)
                    {
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = move;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    Log($"Timeout occurred while processing move: {move.From} to {move.To}");
                }
            });

            Log($"Best Move: {bestMove?.From} to {bestMove?.To}, Best Score: {bestScore}");
            return bestMove;
        }

        /// <summary>
        /// The Minimax algorithm with Alpha-Beta pruning.
        /// </summary>
        private int Minimax(GameState gameState, int depth, int alpha, int beta, bool isMaximizingPlayer, Player aiPlayer)
        {
            if (DateTime.Now - startTime >= maxTime)
            {
                throw new TimeoutException("Search timed out.");
            }

            // Generate a unique hash for the current position
            ulong hash = gameState.Board.GetZobristHash();

            // Check if the position is already in the transposition table
            if (transpositionTable.TryGetValue(hash, out TranspositionEntry entry) && entry.Depth >= depth)
            {
                Log($"Transposition table hit. Cached score: {entry.Score}");
                return entry.Score;
            }

            // Base case: Evaluate the position if depth is 0 or the game is over
            if (depth == 0 || gameState.IsGameOver())
            {
                int score = QuiescenceSearch(gameState, alpha, beta, aiPlayer);
                Log($"Base case reached. Score: {score}");
                return score;
            }

            Player current = gameState.CurrentPlayer;
            IEnumerable<Move> moves = OrderMoves(gameState.AllLegalMovesFor(current), gameState, aiPlayer);

            // If no legal moves, evaluate the position
            if (!moves.Any())
            {
                int score = EvaluateWithLearning(gameState, aiPlayer);
                Log($"No legal moves. Score: {score}");
                return score;
            }

            int value;

            if (isMaximizingPlayer)
            {
                // Maximizing player
                value = int.MinValue;
                foreach (Move move in moves)
                {
                    GameState copyState = new GameState(gameState.CurrentPlayer, gameState.Board.Copy());
                    copyState.MakeMove(move);

                    int score = Minimax(copyState, depth - 1, alpha, beta, false, aiPlayer);
                    value = Math.Max(value, score);
                    alpha = Math.Max(alpha, value);

                    Log($"Maximizing: Move {move.From} to {move.To}, Score: {score}, Alpha: {alpha}, Beta: {beta}");

                    // Alpha-Beta cutoff
                    if (beta <= alpha)
                    {
                        Log($"Alpha-Beta cutoff. Pruning remaining moves.");
                        break;
                    }
                }
            }
            else
            {
                // Minimizing player
                value = int.MaxValue;
                foreach (Move move in moves)
                {
                    GameState copyState = new GameState(gameState.CurrentPlayer, gameState.Board.Copy());
                    copyState.MakeMove(move);

                    int score = Minimax(copyState, depth - 1, alpha, beta, true, aiPlayer);
                    value = Math.Min(value, score);
                    beta = Math.Min(beta, value);

                    Log($"Minimizing: Move {move.From} to {move.To}, Score: {score}, Alpha: {alpha}, Beta: {beta}");

                    // Alpha-Beta cutoff
                    if (beta <= alpha)
                    {
                        Log($"Alpha-Beta cutoff. Pruning remaining moves.");
                        break;
                    }
                }
            }

            // Store the result in the transposition table (thread-safe)
            StoreTransposition(hash, value, depth, ScoreType.Exact);
            Log($"Stored position in transposition table. Hash: {hash}, Score: {value}");

            return value;
        }

        /// <summary>
        /// Quiescence search to evaluate "noisy" positions (e.g., captures, checks).
        /// </summary>
        private int QuiescenceSearch(GameState gameState, int alpha, int beta, Player aiPlayer)
        {
            // Evaluate the position statically
            int standPat = EvaluateWithLearning(gameState, aiPlayer);
            Log($"Quiescence Search: Stand-pat score = {standPat}");

            if (standPat >= beta)
            {
                Log($"Quiescence Search: Beta cutoff, returning {beta}");
                return beta;
            }

            if (standPat > alpha)
            {
                alpha = standPat;
            }

            // Generate capture moves
            IEnumerable<Move> captureMoves = gameState.AllLegalMovesFor(gameState.CurrentPlayer)
                .Where(move => gameState.Board[move.To] != null);

            foreach (Move move in captureMoves)
            {
                GameState copyState = new GameState(gameState.CurrentPlayer, gameState.Board.Copy());
                copyState.MakeMove(move);

                int score = -QuiescenceSearch(copyState, -beta, -alpha, aiPlayer);
                Log($"Quiescence Search: Move {move.From} to {move.To}, Score = {score}");

                if (score >= beta)
                {
                    Log($"Quiescence Search: Beta cutoff, returning {beta}");
                    return beta;
                }

                if (score > alpha)
                {
                    alpha = score;
                }
            }

            Log($"Quiescence Search: Final alpha = {alpha}");
            return alpha;
        }

        /// <summary>
        /// Evaluates the game state with learning mechanism.
        /// </summary>
        private int EvaluateWithLearning(GameState gameState, Player aiPlayer)
        {
            ulong hash = gameState.Board.GetZobristHash();

            // Use TryGetValue for thread-safe read
            if (learnedEvaluations.TryGetValue(hash, out double learnedScore))
            {
                return (int)learnedScore;
            }

            int score = Evaluate(gameState, aiPlayer);

            // Use TryAdd for thread-safe write
            learnedEvaluations.TryAdd(hash, score);
            return score;
        }

        private int Evaluate(GameState gameState, Player aiPlayer)
        {
            if (gameState.IsGameOver())
            {
                if (gameState.Result.Winner == aiPlayer)
                {
                    Log("AI wins! Score: 999999");
                    return 999999;
                }
                else if (gameState.Result.Winner == aiPlayer.Opponent())
                {
                    Log("Opponent wins! Score: -999999");
                    return -999999;
                }
                else
                {
                    Log("Draw! Score: 0");
                    return 0;
                }
            }

            int aiScore = 0;
            int playerScore = 0;

            // Material evaluation
            foreach (Position pos in gameState.Board.PiecePositions())
            {
                Piece piece = gameState.Board[pos];
                int pieceValue = GetPieceValue(piece);

                // Add piece-square table value
                pieceValue += GetPieceSquareValue(piece, pos);

                if (piece.color == aiPlayer)
                {
                    aiScore += pieceValue;
                }
                else
                {
                    playerScore += pieceValue;
                }
            }

            // King safety evaluation
            aiScore += EvaluateKingSafety(gameState, aiPlayer);
            playerScore += EvaluateKingSafety(gameState, aiPlayer.Opponent());

            // Pawn structure evaluation
            aiScore += EvaluatePawnStructure(gameState, aiPlayer);
            playerScore += EvaluatePawnStructure(gameState, aiPlayer.Opponent());

            // Mobility evaluation
            aiScore += EvaluateMobility(gameState, aiPlayer);
            playerScore += EvaluateMobility(gameState, aiPlayer.Opponent());

            // Control of the center evaluation
            aiScore += EvaluateCenterControl(gameState, aiPlayer);
            playerScore += EvaluateCenterControl(gameState, aiPlayer.Opponent());

            // Log the scores for AI and player
            Log($"AI Score: {aiScore}, Player Score: {playerScore}");

            // Return the relative score (AI score - player score)
            int finalScore = aiScore - playerScore;
            Log($"Final Evaluation Score: {finalScore}");
            return finalScore;
        }

        /// <summary>
        /// Evaluates mobility (number of legal moves available).
        /// </summary>
        private int EvaluateMobility(GameState gameState, Player player)
        {
            int mobility = gameState.AllLegalMovesFor(player).Count();
            return mobility * Config.MobilityWeight;
        }

        /// <summary>
        /// Evaluates control of the center squares (d4, d5, e4, e5).
        /// </summary>
        private int EvaluateCenterControl(GameState gameState, Player player)
        {
            int centerControl = 0;
            Position[] centerSquares = { new Position(3, 3), new Position(3, 4), new Position(4, 3), new Position(4, 4) };

            foreach (var pos in centerSquares)
            {
                if (gameState.Board.GetAttackedSquares(pos).Any(sq => gameState.Board[sq]?.color == player))
                {
                    centerControl += Config.CenterControlWeight;
                }
            }

            return centerControl;
        }

        /// <summary>
        /// Assigns a value to each piece type.
        /// </summary>
        private int GetPieceValue(Piece piece)
        {
            return piece switch
            {
                Pawn _ => Config.PawnValue,
                Knight _ => Config.KnightValue,
                Bishop _ => Config.BishopValue,
                Rook _ => Config.RookValue,
                Queen _ => Config.QueenValue,
                King _ => Config.KingValue,
                _ => 0
            };
        }

        /// <summary>
        /// Returns the piece-square table value for a piece at a given position.
        /// </summary>
        private int GetPieceSquareValue(Piece piece, Position pos)
        {
            int[,] table = piece switch
            {
                Pawn _ => pawnTable,
                Knight _ => knightTable,
                Bishop _ => bishopTable,
                Rook _ => rookTable,
                Queen _ => queenTable,
                King _ => kingTable,
                _ => new int[8, 8]
            };

            return table[pos.Row, pos.Column];
        }

        /// <summary>
        /// Evaluates king safety for the given player.
        /// </summary>
        private int EvaluateKingSafety(GameState gameState, Player aiPlayer)
        {
            int score = 0;

            // Penalize if the king is exposed to attacks
            Position kingPos = gameState.Board.PiecePositionsFor(aiPlayer)
                .First(pos => gameState.Board[pos] is King);

            if (gameState.Board.GetAttackedSquares(kingPos).Any())
            {
                score += Config.ExposedKingPenalty;
                Log($"King safety penalty: {Config.ExposedKingPenalty} (exposed king at {kingPos})");
            }

            return score;
        }

        /// <summary>
        /// Evaluates pawn structure for the given player.
        /// </summary>
        private int EvaluatePawnStructure(GameState gameState, Player aiPlayer)
        {
            int score = 0;

            // Reward passed pawns and penalize isolated pawns
            foreach (Position pos in gameState.Board.PiecePositionsFor(aiPlayer))
            {
                if (gameState.Board[pos] is Pawn)
                {
                    if (IsPassedPawn(gameState, pos, aiPlayer))
                    {
                        score += Config.PassedPawnBonus;
                        Log($"Passed pawn bonus: +{Config.PassedPawnBonus} (pawn at {pos})");
                    }

                    if (IsIsolatedPawn(gameState, pos, aiPlayer))
                    {
                        score += Config.IsolatedPawnPenalty;
                        Log($"Isolated pawn penalty: {Config.IsolatedPawnPenalty} (pawn at {pos})");
                    }
                }
            }

            return score;
        }

        /// <summary>
        /// Checks if a pawn is a passed pawn.
        /// </summary>
        private bool IsPassedPawn(GameState gameState, Position pawnPos, Player aiPlayer)
        {
            int direction = aiPlayer == Player.White ? -1 : 1;
            int targetRow = pawnPos.Row + direction;

            while (targetRow >= 0 && targetRow < 8)
            {
                if (gameState.Board[targetRow, pawnPos.Column] != null)
                {
                    return false; // Blocked by another piece
                }

                targetRow += direction;
            }

            return true; // No blocking pieces
        }

        /// <summary>
        /// Checks if a pawn is an isolated pawn.
        /// </summary>
        private bool IsIsolatedPawn(GameState gameState, Position pawnPos, Player aiPlayer)
        {
            int col = pawnPos.Column;

            // Check adjacent files for friendly pawns
            bool hasLeftPawn = col > 0 && gameState.Board[pawnPos.Row, col - 1] is Pawn;
            bool hasRightPawn = col < 7 && gameState.Board[pawnPos.Row, col + 1] is Pawn;

            return !hasLeftPawn && !hasRightPawn; // Isolated if no adjacent pawns
        }

        /// <summary>
        /// Orders moves to prioritize captures, promotions, and checks.
        /// </summary>
        private IEnumerable<Move> OrderMoves(IEnumerable<Move> moves, GameState gameState, Player aiPlayer)
        {
            return moves.OrderByDescending(move =>
            {
                int score = 0;

                // MVV-LVA: Prioritize capturing high-value pieces with low-value attackers
                if (gameState.Board[move.To] != null)
                {
                    score += GetPieceValue(gameState.Board[move.To]) * 10 - GetPieceValue(gameState.Board[move.From]);
                }

                // Bonus for promotions
                if (move.IsPromotion)
                {
                    score += GetPieceValue(move.PromotionPiece);
                }

                // Bonus for checks
                GameState copyState = new GameState(gameState.CurrentPlayer, gameState.Board.Copy());
                copyState.MakeMove(move);
                if (copyState.IsInCheck(aiPlayer.Opponent()))
                {
                    score += Config.CheckBonus;
                }

                return score;
            });
        }

        /// <summary>
        /// Stores a position in the transposition table.
        /// </summary>
        private void StoreTransposition(ulong hash, int score, int depth, ScoreType type)
        {
            if (transpositionTable.Count >= TranspositionTableSize)
            {
                if (transpositionQueue.TryDequeue(out ulong oldestHash))
                {
                    transpositionTable.TryRemove(oldestHash, out _);
                }
            }

            transpositionTable[hash] = new TranspositionEntry { Score = score, Depth = depth, Type = type };
            transpositionQueue.Enqueue(hash);
        }

        /// <summary>
        /// Logging method for debug output.
        /// </summary>
        private void Log(string message)
        {
            if (DebugMode)
            {
                Console.WriteLine($"[DEBUG] {message}");
            }
        }
    }
}