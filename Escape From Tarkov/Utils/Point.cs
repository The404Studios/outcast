namespace ConsoleEscapeFromTarkov.Utils
{
    /// <summary>
    /// Simple structure representing a 2D point
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// X coordinate
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Constructor for Point
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Calculates Manhattan distance between two points
        /// </summary>
        /// <param name="other">Other point</param>
        /// <returns>Manhattan distance</returns>
        public int ManhattanDistance(Point other)
        {
            return System.Math.Abs(X - other.X) + System.Math.Abs(Y - other.Y);
        }

        /// <summary>
        /// Calculates Manhattan distance from coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Manhattan distance</returns>
        public int ManhattanDistance(int x, int y)
        {
            return System.Math.Abs(X - x) + System.Math.Abs(Y - y);
        }

        /// <summary>
        /// ToString implementation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;

            Point other = (Point)obj;
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// GetHashCode implementation
        /// </summary>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }
    }
}