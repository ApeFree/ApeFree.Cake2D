using System;
using System.Drawing;

namespace ApeFree.Cake2D.Math
{
    /// <summary>
    /// 3×2 二维仿射变换矩阵（行向量约定）。
    /// [ x'  y'  1 ] = [ x  y  1 ] * [ M11  M12  0 ]
    ///                              [ M21  M22  0 ]
    ///                              [ M31  M32  1 ]
    /// </summary>
    public struct Matrix3x2 : IEquatable<Matrix3x2>
    {
        public float M11;
        public float M12;
        public float M21;
        public float M22;
        public float M31; // X平移
        public float M32; // Y平移

        public Matrix3x2(float m11, float m12, float m21, float m22, float m31, float m32)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            M31 = m31;
            M32 = m32;
        }

        /// <summary>单位矩阵</summary>
        public static Matrix3x2 Identity => new Matrix3x2(1f, 0f, 0f, 1f, 0f, 0f);

        /// <summary>是否为单位矩阵</summary>
        public bool IsIdentity => M11 == 1f && M12 == 0f && M21 == 0f && M22 == 1f && M31 == 0f && M32 == 0f;

        /// <summary>创建平移矩阵</summary>
        public static Matrix3x2 CreateTranslation(float tx, float ty)
        {
            return new Matrix3x2(1f, 0f, 0f, 1f, tx, ty);
        }

        public static Matrix3x2 CreateTranslation(Vector2D translation)
        {
            return CreateTranslation(translation.X, translation.Y);
        }

        /// <summary>创建等比缩放矩阵</summary>
        public static Matrix3x2 CreateScale(float scale)
        {
            return new Matrix3x2(scale, 0f, 0f, scale, 0f, 0f);
        }

        /// <summary>创建非等比缩放矩阵</summary>
        public static Matrix3x2 CreateScale(float sx, float sy)
        {
            return new Matrix3x2(sx, 0f, 0f, sy, 0f, 0f);
        }

        public static Matrix3x2 CreateScale(Vector2D scale)
        {
            return CreateScale(scale.X, scale.Y);
        }

        /// <summary>创建绕原点旋转矩阵（弧度）</summary>
        public static Matrix3x2 CreateRotation(float radians)
        {
            float c = (float)System.Math.Cos(radians);
            float s = (float)System.Math.Sin(radians);
            return new Matrix3x2(c, s, -s, c, 0f, 0f);
        }

        /// <summary>创建绕指定中心点旋转矩阵（弧度）</summary>
        public static Matrix3x2 CreateRotation(float radians, Vector2D center)
        {
            float c = (float)System.Math.Cos(radians);
            float s = (float)System.Math.Sin(radians);
            float tx = center.X * (1f - c) + center.Y * s;
            float ty = center.Y * (1f - c) - center.X * s;
            return new Matrix3x2(c, s, -s, c, tx, ty);
        }

        /// <summary>变换点</summary>
        public PointF TransformPoint(PointF point)
        {
            return new PointF(
                point.X * M11 + point.Y * M21 + M31,
                point.X * M12 + point.Y * M22 + M32
            );
        }

        /// <summary>变换向量（不含平移）</summary>
        public Vector2D TransformVector(Vector2D vector)
        {
            return new Vector2D(
                vector.X * M11 + vector.Y * M21,
                vector.X * M12 + vector.Y * M22
            );
        }

        /// <summary>矩阵乘法：先应用 left 变换，再应用 right 变换</summary>
        public static Matrix3x2 Multiply(Matrix3x2 left, Matrix3x2 right)
        {
            return new Matrix3x2(
                left.M11 * right.M11 + left.M12 * right.M21,
                left.M11 * right.M12 + left.M12 * right.M22,
                left.M21 * right.M11 + left.M22 * right.M21,
                left.M21 * right.M12 + left.M22 * right.M22,
                left.M31 * right.M11 + left.M32 * right.M21 + right.M31,
                left.M31 * right.M12 + left.M32 * right.M22 + right.M32
            );
        }

        public static Matrix3x2 operator *(Matrix3x2 left, Matrix3x2 right) => Multiply(left, right);

        /// <summary>计算逆矩阵</summary>
        public static bool Invert(Matrix3x2 matrix, out Matrix3x2 result)
        {
            float det = matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;
            if (System.Math.Abs(det) < 1e-6f)
            {
                result = Identity;
                return false;
            }

            float invDet = 1f / det;
            result = new Matrix3x2(
                matrix.M22 * invDet,
                -matrix.M12 * invDet,
                -matrix.M21 * invDet,
                matrix.M11 * invDet,
                (matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31) * invDet,
                (matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32) * invDet
            );
            return true;
        }

        public bool Equals(Matrix3x2 other)
        {
            return M11 == other.M11 && M12 == other.M12 &&
                   M21 == other.M21 && M22 == other.M22 &&
                   M31 == other.M31 && M32 == other.M32;
        }

        public override bool Equals(object obj) => obj is Matrix3x2 other && Equals(other);
        public override int GetHashCode() => M11.GetHashCode() ^ M22.GetHashCode() ^ M31.GetHashCode() ^ M32.GetHashCode();
        public static bool operator ==(Matrix3x2 a, Matrix3x2 b) => a.Equals(b);
        public static bool operator !=(Matrix3x2 a, Matrix3x2 b) => !a.Equals(b);
    }
}