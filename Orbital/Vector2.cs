using System;

namespace Orbital
{
    public class Vector2
    {
        public static readonly Vector2 Zero = new(0);

        /// <summary>
        /// Magnitude of a vector.
        /// </summary>
        public double Magnitude => Math.Sqrt(X * X + Y * Y);

        public double X { get; set; }
        public double Y { get; set; }

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2(double d) : this(d, d)
        {

        }

        public bool Equals(Vector2 vector2)
        {
            return X == vector2.X && Y == vector2.Y;
        }

        public override string ToString()
        {
            return $"{X,11:e3}, {Y,11:e3}";
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) =>
            new(a.X + b.X, a.Y + b.Y);

        public static Vector2 operator -(Vector2 a, Vector2 b) =>
            new(a.X - b.X, a.Y - b.Y);

        public static Vector2 operator *(Vector2 a, Vector2 b) =>
            new(a.X * b.X, a.Y * b.Y);

        public static Vector2 operator /(Vector2 a, Vector2 b) =>
            new(a.X / b.X, a.Y / b.Y);
    }
}