﻿using System;

namespace Chesselogique
{
    public class Direction
    {
        public readonly static Direction North = new Direction(-1, 0);
        public readonly static Direction South = new Direction(1, 0);
        public readonly static Direction East = new Direction(0, 1);
        public readonly static Direction West = new Direction(0, -1);
        public readonly static Direction NorthEast = North + East;
        public readonly static Direction NorthWest = North + West;
        public readonly static Direction SouthEast = South + East;
        public readonly static Direction SouthWest = South + West;

        public int RowDelta { get; }
        public int ColumnDelta { get; }

        public Direction(int rowDelta, int columnDelta)
        {
            RowDelta = rowDelta;
            ColumnDelta = columnDelta;
        }

        // Overloading the '+' operator for combining two directions
        public static Direction operator +(Direction dir1, Direction dir2)
        {
            return new Direction(dir1.RowDelta + dir2.RowDelta, dir1.ColumnDelta + dir2.ColumnDelta);
        }

        // Overloading '*' operator to scale a direction by a scalar
        public static Direction operator *(int scalar, Direction dir)
        {
            return new Direction(dir.RowDelta * scalar, dir.ColumnDelta * scalar);
        }

        // Overloading the '+' operator to add a direction to a position
        public static Position operator +(Position pos, Direction dir)
        {
            // Corrected the ColumnDelta logic here
            return new Position(pos.Row + dir.RowDelta, pos.Column + dir.ColumnDelta);
        }

        // Optional: Add a ToString method for easy debugging
        public override string ToString()
        {
            return $"RowDelta: {RowDelta}, ColumnDelta: {ColumnDelta}";
        }
    }
}
