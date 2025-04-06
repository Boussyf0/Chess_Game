using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chesselogique
{
    public class Castle : Move
    {
        public override MoveType Type { get; }
        public override Position FromPos => From; // Use the base class property
        public override Position ToPos => To;    // Use the base class property

        private readonly Direction KingMoveDir;
        private readonly Position rookFromPos;
        private readonly Position rookToPos;

        public Castle(MoveType type, Position kingPos)
            : base(kingPos, type == MoveType.CastleKS ? new Position(kingPos.Row, 6) : new Position(kingPos.Row, 2))
        {
            Type = type;

            if (type == MoveType.CastleKS)
            {
                KingMoveDir = Direction.East;
                rookFromPos = new Position(kingPos.Row, 7);
                rookToPos = new Position(kingPos.Row, 5);
            }
            else if (type == MoveType.CastleQS)
            {
                KingMoveDir = Direction.West;
                rookFromPos = new Position(kingPos.Row, 0);
                rookToPos = new Position(kingPos.Row, 3);
            }
        }

        public override void Execute(Board board)
        {
            new NormalMove(FromPos, ToPos).Execute(board);
            new NormalMove(rookFromPos, rookToPos).Execute(board);
        }

        public override bool IsLegal(Board board)
        {
            Player player = board[FromPos].color;

            if (board.IsInCheck(player))
            {
                return false;
            }

            Board copy = board.Copy();
            Position kingPosInCopy = FromPos;

            for (int i = 0; i < 2; i++)
            {
                new NormalMove(kingPosInCopy, kingPosInCopy + KingMoveDir).Execute(copy);
                kingPosInCopy += KingMoveDir;

                if (copy.IsInCheck(player))
                {
                    return false;
                }
            }

            return true;
        }
    }
}