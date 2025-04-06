using Chesselogique;

public class Pawn : Piece
{
    public override PieceType Type => PieceType.Pawn;
    public override Player color { get; }

    private readonly Direction forward;

    // Constructor to initialize pawn based on color (White or Black)
    public Pawn(Player color)
    {
        this.color = color;
        forward = color == Player.White ? Direction.North : Direction.South; // Set the forward direction for the pawn based on color
    }

    // Correctly implementing the Copy() method from Piece
    public override Piece copy() // Capitalized method name to match C# conventions
    {
        var copy = new Pawn(color) { HasMoved = this.HasMoved }; // Copy pawn with the same color and HasMoved state
        return copy;
    }

    // Helper method to check if a position is valid and empty
    private static bool CanMoveTo(Position pos, Board board)
    {
        return Board.IsInside(pos) && board.IsEmpty(pos); // Valid position and empty square
    }

    // Helper method to check if the pawn can capture at a position
    private bool CanCaptureAt(Position pos, Board board)
    {
        return Board.IsInside(pos) && !board.IsEmpty(pos) && board[pos].color != color; // Can capture if it's an opponent's piece
    }

    private static IEnumerable<Move> PromotionMoves(Position from, Position to)
    {
        yield return new PawnPromotion(from, to, PieceType.Knight);
        yield return new PawnPromotion(from, to, PieceType.Bishop);
        yield return new PawnPromotion(from, to, PieceType.Rook);
        yield return new PawnPromotion(from, to, PieceType.Queen);
    }

    // Generate possible forward moves (1 or 2 squares) for the pawn
    private IEnumerable<Move> ForwardMoves(Position from, Board board)
    {
        Position oneMovePos = from + forward; // 1 square forward
        if (CanMoveTo(oneMovePos, board))
        {
            if (oneMovePos.Row == 0 || oneMovePos.Row == 7) // Corrected misplaced parentheses
            {
                foreach (Move PromMove in PromotionMoves(from, oneMovePos))
                {
                    yield return PromMove;
                }
            }
            else
            {
                yield return new NormalMove(from, oneMovePos);
            }

            // Pawn's first move allows it to move 2 squares forward
            Position twoMovesPos = oneMovePos + forward;
            if (!HasMoved && CanMoveTo(twoMovesPos, board))
            {
                yield return new NormalMove(from, twoMovesPos);
            }
        }
    }

    // Generate possible diagonal capture moves (captures are diagonal)
    private IEnumerable<Move> DiagonalMoves(Position from, Board board)
    {
        foreach (Direction dir in new[] { Direction.East, Direction.West })
        {
            Position to = from + forward + dir; // Diagonal move (forward + east/west)
            if (CanCaptureAt(to, board))
            {
                if (to.Row == 0 || to.Row == 7) // Corrected misplaced parentheses
                {
                    foreach (Move PromMove in PromotionMoves(from, to))
                    {
                        yield return PromMove;
                    }
                }
                else
                {
                    yield return new NormalMove(from, to);
                }
            }
        }
    }

    // Get all legal moves for the pawn, combining forward and diagonal moves
    public override IEnumerable<Move> GetMoves(Position from, Board board)
    {
        return ForwardMoves(from, board).Concat(DiagonalMoves(from, board)); // Combine both forward and capture moves
    }

    public override bool CanCaptureOpponentKing(Position from, Board board)
    {
        return DiagonalMoves(from, board).Any(move =>
        {
            Piece piece = board[move.ToPos];
            return piece != null && piece.Type == PieceType.King;
        });
    }
}
