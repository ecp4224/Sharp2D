using System;
using System.Collections.Generic;

namespace Sharp2D.Common;

public struct Size : IComparable<Size>, IEquatable<Size>
{
    public int Width;
    public int Height;

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public Size()
    {
    }

    public int CompareTo(Size other)
    {
        var widthComparison = Width.CompareTo(other.Width);
        if (widthComparison != 0) return widthComparison;
        return Height.CompareTo(other.Height);
    }

    public bool Equals(Size other)
    {
        return Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object obj)
    {
        return obj is Size other && Equals(other);
    }

    private sealed class WidthHeightEqualityComparer : IEqualityComparer<Size>
    {
        public bool Equals(Size x, Size y)
        {
            return x.Width == y.Width && x.Height == y.Height;
        }

        public int GetHashCode(Size obj)
        {
            return HashCode.Combine(obj.Width, obj.Height);
        }
    }

    public static IEqualityComparer<Size> WidthHeightComparer { get; } = new WidthHeightEqualityComparer();

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    public static bool operator ==(Size obj1, Size obj2)
    {
        return obj1.Equals(obj2);
    }
    
    public static bool operator !=(Size obj1, Size obj2)
    {
        return !obj1.Equals(obj2);
    }
}