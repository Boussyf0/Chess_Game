using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chesselogique
{
    public class Bishop : Piece
    {
        public override PieceType Type => PieceType.Bishop;
        public override Player color { get; }

        // Directions in which the bishop can move (diagonals)
        private static readonly Direction[] dirs = new Direction[]
        {
            Direction.NorthWest,
            Direction.NorthEast,
            Direction.SouthWest,
            Direction.SouthEast
        };

        public Bishop(Player color)
        {
            this.color = color;
        }

        // Clone the bishop, including the HasMoved state
        public override Piece copy()
        {
            Bishop copy = new Bishop(color);
            copy.HasMoved = HasMoved;  // Preserve the HasMoved state
            return copy;
        }

        // Get all possible legal moves for the bishop from a given position
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            // Generate moves for each diagonal direction
            return MovePositionsInDirs(from, board, dirs)
                .Select(to => new NormalMove(from, to)); // Wrap positions into NormalMove objects
        }
    }
}
