using System;
using System.Collections.Generic;
using System.Linq;

namespace Chesselogique
{
    public class Board
    {
        private readonly Piece[,] pieces = new Piece[8, 8];

        // Indexer to access pieces by row and column
        public Piece this[int row, int col]
        {
            get { return pieces[row, col]; }
            set { pieces[row, col] = value; }
        }

        // Indexer to access pieces by Position object
        public Piece this[Position pos]
        {
            get { return this[pos.Row, pos.Column]; }
            set { this[pos.Row, pos.Column] = value; }
        }

        // Initialize the board with starting pieces
        public static Board Initial()
        {
            Board board = new Board();
            board.AddStartPieces();
            return board;
        }

        // Add starting pieces to the board
        private void AddStartPieces()
        {
            // Black pieces
            this[0, 0] = new Rook(Player.Black);
            this[0, 1] = new Knight(Player.Black);
            this[0, 2] = new Bishop(Player.Black);
            this[0, 3] = new Queen(Player.Black);
            this[0, 4] = new King(Player.Black);
            this[0, 5] = new Bishop(Player.Black);
            this[0, 6] = new Knight(Player.Black);
            this[0, 7] = new Rook(Player.Black);

            // White pieces
            this[7, 0] = new Rook(Player.White);
            this[7, 1] = new Knight(Player.White);
            this[7, 2] = new Bishop(Player.White);
            this[7, 3] = new Queen(Player.White);
            this[7, 4] = new King(Player.White);
            this[7, 5] = new Bishop(Player.White);
            this[7, 6] = new Knight(Player.White);
            this[7, 7] = new Rook(Player.White);

            // Pawns
            for (int c = 0; c < 8; c++)
            {
                this[1, c] = new Pawn(Player.Black);
                this[6, c] = new Pawn(Player.White);
            }
        }

        // Check if a position is inside the board
        public static bool IsInside(Position pos)
        {
            return pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8;
        }

        // Check if a position is empty
        public bool IsEmpty(Position pos)
        {
            return this[pos] == null;
        }

        // Get all positions occupied by pieces
        public IEnumerable<Position> PiecePositions()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Position pos = new Position(r, c);

                    if (!IsEmpty(pos))
                    {
                        yield return pos;
                    }
                }
            }
        }

        // Get all positions occupied by pieces of a specific player
        public IEnumerable<Position> PiecePositionsFor(Player player)
        {
            return PiecePositions().Where(pos => this[pos].color == player);
        }

        // Check if a player is in check
        public bool IsInCheck(Player player)
        {
            return PiecePositionsFor(player.Opponent()).Any(pos =>
            {
                Piece piece = this[pos];
                return piece.CanCaptureOpponentKing(pos, this);
            });
        }

        // Create a copy of the board
        public Board Copy()
        {
            Board copy = new Board();
            foreach (Position pos in PiecePositions())
            {
                copy[pos] = this[pos].copy();
            }
            return copy;
        }

        // Get all squares attacked by the piece at the given position
        public IEnumerable<Position> GetAttackedSquares(Position position)
        {
            Piece piece = this[position];
            if (piece == null)
            {
                return Enumerable.Empty<Position>(); // No piece at the position
            }

            List<Position> attackedSquares = new List<Position>();

            // Handle each piece type
            switch (piece)
            {
                case Pawn pawn:
                    attackedSquares.AddRange(GetPawnAttacks(position, pawn.color));
                    break;
                case Knight knight:
                    attackedSquares.AddRange(GetKnightAttacks(position));
                    break;
                case Bishop bishop:
                    attackedSquares.AddRange(GetBishopAttacks(position));
                    break;
                case Rook rook:
                    attackedSquares.AddRange(GetRookAttacks(position));
                    break;
                case Queen queen:
                    attackedSquares.AddRange(GetQueenAttacks(position));
                    break;
                case King king:
                    attackedSquares.AddRange(GetKingAttacks(position));
                    break;
            }

            return attackedSquares;
        }

        // Get squares attacked by a pawn
        private IEnumerable<Position> GetPawnAttacks(Position position, Player color)
        {
            List<Position> attacks = new List<Position>();
            int direction = (color == Player.White) ? -1 : 1; // Pawns move forward based on color

            // Diagonal attacks
            Position leftAttack = new Position(position.Row + direction, position.Column - 1);
            Position rightAttack = new Position(position.Row + direction, position.Column + 1);

            if (IsInside(leftAttack))
            {
                attacks.Add(leftAttack);
            }
            if (IsInside(rightAttack))
            {
                attacks.Add(rightAttack);
            }

            return attacks;
        }

        // Get squares attacked by a knight
        private IEnumerable<Position> GetKnightAttacks(Position position)
        {
            int[] rowOffsets = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] colOffsets = { 1, 2, 2, 1, -1, -2, -2, -1 };

            List<Position> attacks = new List<Position>();

            for (int i = 0; i < 8; i++)
            {
                Position attack = new Position(position.Row + rowOffsets[i], position.Column + colOffsets[i]);
                if (IsInside(attack))
                {
                    attacks.Add(attack);
                }
            }

            return attacks;
        }

        // Get squares attacked by a bishop
        private IEnumerable<Position> GetBishopAttacks(Position position)
        {
            return GetDiagonalAttacks(position);
        }

        // Get squares attacked by a rook
        private IEnumerable<Position> GetRookAttacks(Position position)
        {
            return GetStraightAttacks(position);
        }

        // Get squares attacked by a queen
        private IEnumerable<Position> GetQueenAttacks(Position position)
        {
            return GetDiagonalAttacks(position).Concat(GetStraightAttacks(position));
        }

        // Get squares attacked by a king
        private IEnumerable<Position> GetKingAttacks(Position position)
        {
            int[] rowOffsets = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] colOffsets = { -1, 0, 1, -1, 1, -1, 0, 1 };

            List<Position> attacks = new List<Position>();

            for (int i = 0; i < 8; i++)
            {
                Position attack = new Position(position.Row + rowOffsets[i], position.Column + colOffsets[i]);
                if (IsInside(attack))
                {
                    attacks.Add(attack);
                }
            }

            return attacks;
        }

        // Get all squares attacked diagonally from the given position
        private IEnumerable<Position> GetDiagonalAttacks(Position position)
        {
            List<Position> attacks = new List<Position>();

            // Four diagonal directions
            int[] rowDirections = { -1, -1, 1, 1 };
            int[] colDirections = { -1, 1, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int row = position.Row + rowDirections[i];
                int col = position.Column + colDirections[i];

                while (IsInside(new Position(row, col)))
                {
                    attacks.Add(new Position(row, col));
                    if (this[row, col] != null)
                    {
                        break; // Stop if we hit a piece
                    }
                    row += rowDirections[i];
                    col += colDirections[i];
                }
            }

            return attacks;
        }

        // Get all squares attacked in straight lines from the given position
        private IEnumerable<Position> GetStraightAttacks(Position position)
        {
            List<Position> attacks = new List<Position>();

            // Four straight directions
            int[] rowDirections = { -1, 0, 1, 0 };
            int[] colDirections = { 0, 1, 0, -1 };

            for (int i = 0; i < 4; i++)
            {
                int row = position.Row + rowDirections[i];
                int col = position.Column + colDirections[i];

                while (IsInside(new Position(row, col)))
                {
                    attacks.Add(new Position(row, col));
                    if (this[row, col] != null)
                    {
                        break; // Stop if we hit a piece
                    }
                    row += rowDirections[i];
                    col += colDirections[i];
                }
            }

            return attacks;
        }

        // Generate a Zobrist hash for the board state
        public ulong GetZobristHash()
        {
            ulong hash = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = this[row, col];
                    if (piece != null)
                    {
                        // Combine piece type, color, and position into the hash
                        hash ^= ZobristKey(piece, new Position(row, col));
                    }
                }
            }
            return hash;
        }

        // Generate a unique key for a piece at a given position
        private ulong ZobristKey(Piece piece, Position position)
        {
            // Use a unique value for each piece type and color
            ulong pieceKey = (ulong)piece.GetType().GetHashCode() ^ (ulong)piece.color.GetHashCode();
            // Combine with the position
            return pieceKey ^ (ulong)position.GetHashCode();
        }

        public void PrintBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = this[row, col];
                    char pieceChar = piece == null ? '.' : GetPieceChar(piece);
                    Console.Write($"{pieceChar} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Returns a character representation of a piece.
        /// </summary>
        private char GetPieceChar(Piece piece)
        {
            return piece switch
            {
                Pawn p => p.color == Player.White ? 'P' : 'p',
                Knight _ => piece.color == Player.White ? 'N' : 'n',
                Bishop _ => piece.color == Player.White ? 'B' : 'b',
                Rook _ => piece.color == Player.White ? 'R' : 'r',
                Queen _ => piece.color == Player.White ? 'Q' : 'q',
                King _ => piece.color == Player.White ? 'K' : 'k',
                _ => '.'
            };
        }
    }
}