using System;

namespace Orbital
{
    public class Vector3 : Vector2
    {
        public static new readonly Vector3 Zero = new(0);

        public double Z { get; set; }

        /// <summary>
        /// Magnitude of a vector.
        /// </summary>
        public new double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3(double x, double y, double z) : base(x, y)
        {
            Z = z;
        }

        public Vector3(double d) : this(d, d, d)
        {

        }

        public bool Equals(Vector3 vector3)
        {
            return X == vector3.X && Y == vector3.Y && Z == vector3.Z;
        }

        public override string ToString()
        {
            return $"{X,11:e3}, {Y,11:e3}, {Z,11:e3}";
        }

        public static Vector3 operator +(Vector3 a, Vector3 b) =>
            new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3 operator -(Vector3 a, Vector3 b) =>
            new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3 operator *(Vector3 a, Vector3 b) =>
            new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);

        public static Vector3 operator /(Vector3 a, Vector3 b) =>
            new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }
}
