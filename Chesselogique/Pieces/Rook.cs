using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chesselogique
{
    public class Rook : Piece
    {
        // Piece type (Rook) is assigned here.
        public override PieceType Type => PieceType.Rook;

        // Color of the Rook (White or Black).
        public override Player color { get; }

        // Directions the Rook can move: North, South, East, West (straight lines).
        private static readonly Direction[] dirs = new Direction[]
        {
            Direction.North, // Move up the board
            Direction.South, // Move down the board
            Direction.East,  // Move right
            Direction.West   // Move left
        };

        // Constructor to initialize the Rook with a specific color.
        public Rook(Player color)
        {
            this.color = color; // Set the color of the Rook (either White or Black).
        }

        // Method to create a copy of the Rook piece, preserving its HasMoved state.
        public override Piece copy()
        {
            // Create a new Rook and copy the HasMoved property.
            Rook copy = new Rook(color);
            copy.HasMoved = HasMoved;  // Preserve the HasMoved state from the original Rook.
            return copy;
        }

        // Method to get all possible legal moves for the Rook from a given position.
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            // Generate all valid move positions in the four cardinal directions (North, South, East, West)
            return MovePositionsInDirs(from, board, dirs)
                .Select(to => new NormalMove(from, to)); // Return each valid position as a NormalMove.
        }
    }
}
