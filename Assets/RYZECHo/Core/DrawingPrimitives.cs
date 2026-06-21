namespace RYZECHo;

internal readonly struct Point : IEquatable<Point>
{
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }
    public int Y { get; }

    public bool Equals(Point other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is Point other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public static bool operator ==(Point left, Point right) => left.Equals(right);
    public static bool operator !=(Point left, Point right) => !left.Equals(right);
}

internal struct Vector2 : IEquatable<Vector2>
{
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public static Vector2 Empty => default;

    public bool Equals(Vector2 other) => X.Equals(other.X) && Y.Equals(other.Y);
    public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
    public static bool operator !=(Vector2 left, Vector2 right) => !left.Equals(right);
}

internal readonly struct Size : IEquatable<Size>
{
    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }

    public bool Equals(Size other) => Width == other.Width && Height == other.Height;
    public override bool Equals(object? obj) => obj is Size other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Width, Height);
}

internal readonly struct Rectangle : IEquatable<Rectangle>
{
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public int Left => X;
    public int Top => Y;
    public int Right => X + Width;
    public int Bottom => Y + Height;
    public Size Size => new(Width, Height);

    public bool Contains(Point point) => Contains(point.X, point.Y);
    public bool Contains(int x, int y) => x >= Left && x < Right && y >= Top && y < Bottom;
    public bool Equals(Rectangle other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    public override bool Equals(object? obj) => obj is Rectangle other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    public static implicit operator RectangleF(Rectangle value) => new(value.X, value.Y, value.Width, value.Height);
}

internal readonly struct RectangleF : IEquatable<RectangleF>
{
    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float X { get; }
    public float Y { get; }
    public float Width { get; }
    public float Height { get; }
    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;

    public bool Contains(Vector2 point) => Contains(point.X, point.Y);
    public bool Contains(float x, float y) => x >= Left && x < Right && y >= Top && y < Bottom;

    public static RectangleF Inflate(RectangleF rectangle, float x, float y) =>
        new(rectangle.X - x, rectangle.Y - y, rectangle.Width + (x * 2f), rectangle.Height + (y * 2f));

    public bool Equals(RectangleF other) =>
        X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);

    public override bool Equals(object? obj) => obj is RectangleF other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}

internal readonly struct Color : IEquatable<Color>
{
    public Color(byte alpha, byte red, byte green, byte blue)
    {
        A = alpha;
        R = red;
        G = green;
        B = blue;
    }

    public Color(float red, float green, float blue, float alpha)
        : this(ToByte(alpha), ToByte(red), ToByte(green), ToByte(blue))
    {
    }

    public byte A { get; }
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    public static Color FromArgb(int red, int green, int blue) => FromArgb(255, red, green, blue);

    public static Color FromArgb(int alpha, int red, int green, int blue) =>
        new(ToByte(alpha), ToByte(red), ToByte(green), ToByte(blue));

    public static Color FromArgb(int alpha, Color baseColor) =>
        new(ToByte(alpha), baseColor.R, baseColor.G, baseColor.B);

    public static implicit operator UnityEngine.Color(Color value) =>
        new(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);

    public bool Equals(Color other) => A == other.A && R == other.R && G == other.G && B == other.B;
    public override bool Equals(object? obj) => obj is Color other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(A, R, G, B);

    private static byte ToByte(float value) => ToByte((int)Math.Round(value * 255f));
    private static byte ToByte(int value) => (byte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, value));
}

internal enum MatrixOrder
{
    Prepend,
    Append,
}

internal sealed class Matrix : IDisposable
{
    private System.Numerics.Matrix3x2 _value;

    public Matrix(float m11, float m12, float m21, float m22, float offsetX, float offsetY)
    {
        _value = new System.Numerics.Matrix3x2(m11, m12, m21, m22, offsetX, offsetY);
    }

    public void Invert()
    {
        if (!System.Numerics.Matrix3x2.Invert(_value, out var inverse))
        {
            throw new InvalidOperationException("座標変換行列を反転できません。");
        }

        _value = inverse;
    }

    public void TransformPoints(Vector2[] points)
    {
        for (var index = 0; index < points.Length; index++)
        {
            var transformed = System.Numerics.Vector2.Transform(
                new System.Numerics.Vector2(points[index].X, points[index].Y),
                _value);
            points[index] = new Vector2(transformed.X, transformed.Y);
        }
    }

    public void Translate(float offsetX, float offsetY, MatrixOrder order)
    {
        Apply(System.Numerics.Matrix3x2.CreateTranslation(offsetX, offsetY), order);
    }

    public void Scale(float scaleX, float scaleY, MatrixOrder order)
    {
        Apply(System.Numerics.Matrix3x2.CreateScale(scaleX, scaleY), order);
    }

    public void Dispose()
    {
    }

    private void Apply(System.Numerics.Matrix3x2 transform, MatrixOrder order)
    {
        _value = order == MatrixOrder.Append ? _value * transform : transform * _value;
    }
}
