using System;
using System.Collections.Generic;
using System.Linq;

namespace Chesselogique
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;

        private readonly List<Move> moveHistory;  // Historique des mouvements effectués

        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;
            moveHistory = new List<Move>();

        }

        // Récupère les mouvements légaux pour une pièce spécifique à la position donnée
        public IEnumerable<Move> LegalMovesForPiece(Position pos)
        {
            if (Board.IsEmpty(pos) || Board[pos].color != CurrentPlayer)
            {
                return Enumerable.Empty<Move>();
            }

            Piece piece = Board[pos];
            IEnumerable<Move> movecandidates = piece.GetMoves(pos, Board);
            return movecandidates.Where(move => move.IsLegal(Board));
        }

        // Exécute un mouvement et passe le tour au joueur adverse
        public void MakeMove(Move move)
        {
            move.Execute(Board);
            moveHistory.Add(move);  // Ajoute le mouvement à l'historique
            CurrentPlayer = CurrentPlayer.Opponent();
            CheckForGameOver();
        }
    

        // Méthode pour annuler le dernier mouvement (optionnelle)
        public void UndoLastMove()
        {
            if (moveHistory.Count == 0)
            {
                throw new InvalidOperationException("Aucun mouvement à annuler.");
            }

           
        }
        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            IEnumerable<Move> moveCandidates = Board.PiecePositionsFor(player).SelectMany(pos =>
            {
                Piece piece = Board[pos];
                return piece.GetMoves(pos, Board);
            });

            return moveCandidates.Where(move => move.IsLegal(Board)); 
        }

        private void CheckForGameOver()
        {
            if (!AllLegalMovesFor(CurrentPlayer).Any())
            {
                if(Board.IsInCheck(CurrentPlayer))
                {
                     Result = Result.Win(CurrentPlayer.Opponent());
                }
                else
                {
                     Result = Result.Draw(EndReason.Stalemate);
                }
            }
        }

        // Create a deep copy of the game state
        public GameState Copy()
        {
            Board boardCopy = Board.Copy();
            return new GameState(CurrentPlayer, boardCopy);
        }

        // Check if the current player is in check
        public bool IsInCheck(Player player)
        {
            return Board.PiecePositionsFor(player.Opponent()).Any(pos =>
            {
                Piece piece = Board[pos];
                return piece.CanCaptureOpponentKing(pos, Board);
            });
        }

        public bool IsGameOver()
        {
            return Result != null;
        }

        
    }
}
