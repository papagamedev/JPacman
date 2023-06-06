using Unity.Mathematics;

public struct Direction
{
    public const int Right = 0;
    public const int Left = 1;
    public const int Down = 2;
    public const int Up = 3;
    public const int None = -1;

    public int m_value;

    public static bool operator ==(Direction left, Direction right) => left.m_value == right.m_value;

    public static bool operator !=(Direction left, Direction right) => left.m_value != right.m_value;

    public static implicit operator int(Direction dir) => dir.m_value;

    public static implicit operator Direction(int dir)
    {
        if (dir >= Right && dir <= Up)
        {
            return new Direction() { m_value = dir };
        }
        return new Direction() { m_value = None };
    }

    public override int GetHashCode() => m_value;

    public override bool Equals(object obj)
    {
        if (obj is Direction dir)
        {
            return dir.m_value == m_value;
        }
        if (obj is int dirInt)
        {
            return dirInt == m_value;
        }
        return false;
    }

    public override string ToString()
    {
        return m_value switch
        {
            Right => "R",
            Left => "L",
            Down => "D",
            Up => "U",
            _ => "-"
        };
    }

    public Direction Opposite()
    {
        return new Direction() { m_value = this.m_value ^ 1 };
    }

    public float2 Vector()
    {
        return m_value switch
        {
            Right => new float2(1, 0),
            Left => new float2(-1, 0),
            Down => new float2(0, 1),
            Up => new float2(0, -1),
            _ => new float2(0, 0)
        };
    }
}
