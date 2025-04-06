using System;

namespace Chesselogique
{
    public class NormalMove : Move
    {
        public override MoveType Type => MoveType.Normal;
        public override Position FromPos => From; // Use the base class property
        public override Position ToPos => To;    // Use the base class property

        private Piece capturedPiece;  // Stocke la pièce capturée, s'il y en a une

        public NormalMove(Position from, Position to)
            : base(from, to)
        {
        }

        public override void Execute(Board board)
        {
            Piece piece = board[FromPos];
            capturedPiece = board[ToPos];  // Stocke la pièce capturée, si elle existe
            board[ToPos] = piece;  // Déplace la pièce à la nouvelle position
            board[FromPos] = null;  // Supprime la pièce de la position initiale
            piece.HasMoved = true;  // Marque la pièce comme ayant été déplacée (utile pour le roque)
        }
    }
}
