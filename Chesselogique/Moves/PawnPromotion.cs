using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chesselogique
{
    public class PawnPromotion : Move
    {
        public override MoveType Type => MoveType.PawnPromotion;
        public override Position FromPos => From; // Use the base class property
        public override Position ToPos => To;    // Use the base class property

        private readonly PieceType newType;

        public PawnPromotion(Position from, Position to, PieceType newType)
            : base(from, to, isPromotion: true, promotionPiece: null) // Pass null for promotionPiece initially
        {
            this.newType = newType;
        }

        private Piece CreatePromotionPiece(Player color)
        {
            return newType switch
            {
                PieceType.Knight => new Knight(color),
                PieceType.Bishop => new Bishop(color),
                PieceType.Rook => new Rook(color),
                _ => new Queen(color), // Default to Queen if no valid promotion type is specified
            };
        }

        public override void Execute(Board board)
        {
            Piece pawn = board[FromPos];
            board[FromPos] = null;

            // Create the promotion piece dynamically
            Piece promotionPiece = CreatePromotionPiece(pawn.color);
            promotionPiece.HasMoved = true;
            board[ToPos] = promotionPiece;
        }
    }
}
