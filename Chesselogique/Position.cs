using System;
using System.Collections.Generic;

namespace Chesselogique
{
    public class Position
    {
        public int Row { get; }
        public int Column { get; }

        // Constructor to initialize the row and column of the position.
        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        // Determine the color of the square (light or dark) based on its row and column.
        public Player SquareColor() => (Row + Column) % 2 == 0 ? Player.White : Player.Black;

        // Override Equals method to compare two positions.
        public override bool Equals(object obj)
        {
            return obj is Position position &&
                   Row == position.Row &&
                   Column == position.Column;
        }

        // Override GetHashCode for correct hashing.
        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        // Operator overload to check if two positions are equal.
        public static bool operator ==(Position left, Position right)
        {
            return EqualityComparer<Position>.Default.Equals(left, right);
        }

        // Operator overload to check if two positions are not equal.
        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }

        // Override ToString to provide a string representation of the Position.
        public override string ToString()
        {
            return $"Position(Row: {Row}, Column: {Column})";
        }
    }
}
