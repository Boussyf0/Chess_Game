using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chesselogique
{
    public class King : Piece
    {
        public override PieceType Type => PieceType.King; // The piece type is King.
        public override Player color { get; } // The color of the king.

        // The 8 directions the king can move: vertically, horizontally, and diagonally.
        private static readonly Direction[] dirs = new Direction[]
        {
            Direction.East,        // Right
            Direction.South,       // Down
            Direction.West,        // Left
            Direction.North,       // Up
            Direction.NorthEast,   // Diagonal: Up-Right
            Direction.SouthEast,   // Diagonal: Down-Right
            Direction.SouthWest,   // Diagonal: Down-Left
            Direction.NorthWest    // Diagonal: Up-Left
        };

        public King(Player color)
        {
            this.color = color; // Set the color of the king.
        }

        private static bool IsUnmovedRook(Position pos, Board board)
        {
            if(board.IsEmpty(pos))
            {
                return false;
            }
            Piece piece = board[pos];
            return piece.Type == PieceType.Rook && !piece.HasMoved;
        }

        private static bool AllEmpty(IEnumerable<Position> positions, Board board)
        {
            return positions.All(pos => board.IsEmpty(pos));    
        }

        private bool CanCastleKingSide(Position from, Board board)
        {
            if (HasMoved)
            {
                return false;
            }

            Position rookPos = new Position(from.Row, 7);
            Position[] betweenPositions = new Position[] { new(from.Row, 5), new(from.Row, 6) };

            return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
        }

        private bool CanCastleQweenSide(Position from, Board board)
        {
            if (HasMoved)
            {
                return false;
            }

            Position rookPos = new Position(from.Row, 0);
            Position[] betweenPositions = new Position[] { new(from.Row, 1), new(from.Row, 2), new(from.Row, 3) };

            return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
        }

        // Create a copy of the king, including the HasMoved state.
        public override Piece copy()
        {
            King copy = new King(color); // Create a new king with the same color.
            copy.HasMoved = HasMoved;   // Preserve the HasMoved state.
            return copy;
        }

        // Generate all possible valid move positions for the king from the given position.
        private IEnumerable<Position> MovePositions(Position from, Board board)
        {
            foreach (Direction dir in dirs)
            {
                Position to = from + dir; // Calculate the target position based on the direction.

                // If the target position is outside the board, skip it.
                if (!Board.IsInside(to))
                {
                    continue;
                }

                // If the target position is empty or occupied by an enemy piece, yield the position.
                if (board.IsEmpty(to) || board[to].color != color)
                {
                    yield return to;
                }
            }
        }

        // Get all possible valid moves for the king from the given position on the board.
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            foreach (Position to in MovePositions(from, board))
            {
                yield return new NormalMove(from, to);  // Return the valid move wrapped in a NormalMove object.
            }

            if(CanCastleKingSide(from, board))
            {
                yield return new Castle(MoveType.CastleKS, from);
            }

            if (CanCastleQweenSide(from, board))
            {
                yield return new Castle(MoveType.CastleQS, from);
            }

        }

        public override bool CanCaptureOpponentKing(Position from, Board board)
        {
            return MovePositions(from, board).Any(to =>
            {
                Piece piece = board[to];
                return piece != null && piece.Type == PieceType.King;
            });
        }
    }
}
