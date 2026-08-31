using System;
using System.Drawing;

namespace ApeFree.Cake2D.Math
{
    /// <summary>
    /// 二维向量结构体，支持点与方向运算。
    /// </summary>
    public struct Vector2D : IEquatable<Vector2D>
    {
        public float X;
        public float Y;

        public static readonly Vector2D Zero = new Vector2D(0, 0);
        public static readonly Vector2D One = new Vector2D(1, 1);
        public static readonly Vector2D UnitX = new Vector2D(1, 0);
        public static readonly Vector2D UnitY = new Vector2D(0, 1);

        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2D(PointF point)
        {
            X = point.X;
            Y = point.Y;
        }

        public float Length => (float)System.Math.Sqrt(X * X + Y * Y);
        public float LengthSquared => X * X + Y * Y;

        public Vector2D Normalized()
        {
            float len = Length;
            return len > 1e-6f ? new Vector2D(X / len, Y / len) : Zero;
        }

        public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
        public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
        public static Vector2D operator -(Vector2D v) => new Vector2D(-v.X, -v.Y);
        public static Vector2D operator *(Vector2D v, float scalar) => new Vector2D(v.X * scalar, v.Y * scalar);
        public static Vector2D operator *(float scalar, Vector2D v) => new Vector2D(v.X * scalar, v.Y * scalar);
        public static Vector2D operator /(Vector2D v, float scalar) => new Vector2D(v.X / scalar, v.Y / scalar);

        public static bool operator ==(Vector2D a, Vector2D b) => System.Math.Abs(a.X - b.X) < 1e-6f && System.Math.Abs(a.Y - b.Y) < 1e-6f;
        public static bool operator !=(Vector2D a, Vector2D b) => !(a == b);

        public static float Dot(Vector2D a, Vector2D b) => a.X * b.X + a.Y * b.Y;
        public static float Cross(Vector2D a, Vector2D b) => a.X * b.Y - a.Y * b.X;
        public static float Distance(Vector2D a, Vector2D b) => (a - b).Length;
        public static Vector2D Lerp(Vector2D a, Vector2D b, float t) => a + (b - a) * t;

        public PointF ToPointF() => new PointF(X, Y);
        public static implicit operator PointF(Vector2D v) => new PointF(v.X, v.Y);
        public static implicit operator Vector2D(PointF p) => new Vector2D(p.X, p.Y);

        public bool Equals(Vector2D other) => this == other;
        public override bool Equals(object obj) => obj is Vector2D other && Equals(other);
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
        public override string ToString() => $"({X:F3}, {Y:F3})";
    }
}