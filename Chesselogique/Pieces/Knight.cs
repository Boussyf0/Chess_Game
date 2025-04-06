using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chesselogique
{
    public class Knight : Piece
    {
        public override PieceType Type => PieceType.Knight;
        public override Player color { get; }

        // Calculate all potential knight moves from a given position (in an "L" shape).
        private static IEnumerable<Position> CalculatePotentialKnightMoves(Position from)
        {
            foreach (Direction vDir in new Direction[] { Direction.North, Direction.South })
            {
                foreach (Direction hDir in new Direction[] { Direction.East, Direction.West })
                {
                    // The two possible "L" shapes: 2 squares in one direction and 1 square in a perpendicular direction.
                    yield return from + 2 * vDir + hDir;
                    yield return from + 2 * hDir + vDir;
                }
            }
        }

        // Get all valid positions the knight can move to from the given position.
        private IEnumerable<Position> MovePositions(Position from, Board board)
        {
            // Filter out invalid positions (out of bounds or occupied by a friendly piece).
            return CalculatePotentialKnightMoves(from)
                .Where(pos => Board.IsInside(pos) && (board.IsEmpty(pos) || board[pos].color != color));
        }

        // Get the actual legal moves for the knight, wrapping each move in a NormalMove.
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            return MovePositions(from, board).Select(to => new NormalMove(from, to));
        }

        public Knight(Player color)
        {
            this.color = color; // Set the knight's color (white or black).
        }

        // Create a copy of the knight, preserving the HasMoved state.
        public override Piece copy()
        {
            Knight copy = new Knight(color); // Create a new knight with the same color.
            copy.HasMoved = HasMoved; // Copy the HasMoved state.
            return copy;
        }
    }
}
