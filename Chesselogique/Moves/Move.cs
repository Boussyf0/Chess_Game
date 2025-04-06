using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chesselogique
{
    public abstract class Move
    {
        public abstract MoveType Type { get; }
        public abstract Position FromPos { get; }
        public abstract Position ToPos { get; }
        public abstract void Execute(Board board);

        public Piece PromotionPiece { get; } // The piece to promote to (if any)
        public Position From { get; } // Starting position
        public Position To { get; }   // Destination position
        public bool IsPromotion { get; } // Whether the move is a promotion

        public virtual bool IsLegal(Board board)
        {
            Player player = board[FromPos].color;
            Board boardCopy = board.Copy();
            Execute(boardCopy);
            return !boardCopy.IsInCheck(player);
        }

        public Move(Position from, Position to, bool isPromotion = false, Piece promotionPiece = null)
        {
            From = from;
            To = to;
            IsPromotion = isPromotion;
            PromotionPiece = promotionPiece;
        }
    }

}
