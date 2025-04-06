using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chesselogique
{
    public class Queen : Piece
    {
        public override PieceType Type => PieceType.Queen;
        public override Player color { get; }

        // Directions in which the queen can move (all straight and diagonal directions)
        private static readonly Direction[] dirs = new Direction[]
        {
            Direction.East,        // Right
            Direction.South,       // Down
            Direction.West,        // Left
            Direction.North,       // Up
            Direction.NorthEast,   // Diagonal up-right
            Direction.SouthEast,   // Diagonal down-right
            Direction.SouthWest,   // Diagonal down-left
            Direction.NorthWest    // Diagonal up-left
        };

        // Constructor to initialize the Queen piece with the given color
        public Queen(Player color)
        {
            this.color = color; // Assign the color (either White or Black)
        }

        // Method to create a copy of the Queen piece, preserving the HasMoved state
        public override Piece copy()
        {
            // Clone the Queen and copy the HasMoved state from the original piece
            Queen copy = new Queen(color);
            copy.HasMoved = HasMoved;  // Copy HasMoved state
            return copy;
        }

        // Method to generate all possible legal moves for the Queen from a given position
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            // Generate all possible moves in the directions defined above (using MovePositionsInDirs)
            return MovePositionsInDirs(from, board, dirs)
                .Select(to => new NormalMove(from, to)); // Convert each valid position into a NormalMove
        }
    }
}
