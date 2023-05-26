using Unity.Collections;
using Unity.Mathematics;

public struct AvailableDirections
{
    public bool Right;
    public bool Left;
    public bool Down;
    public bool Up;

    public int Count => (Right ? 1 : 0) + (Left ? 1 : 0) + (Down ? 1 : 0) + (Up ? 1 : 0);
    public Direction First => Right ? Direction.Right : Left ? Direction.Left : Down ? Direction.Down : Up ? Direction.Up : Direction.None;
    public bool Check(Direction direction)
    {
        return (int)direction switch
        {
            Direction.Right => Right,
            Direction.Left => Left,
            Direction.Down => Down,
            Direction.Up => Up,
            _ => false,
        };
    }
    public Direction RandomNotOpposite(Direction currentDir, ref Random random)
    {
        var oppositeDir = currentDir.Opposite();
        if (Count <= 1)
        {
            var first = First;
            return oppositeDir == first ? Direction.None : first;
        }

        var dir = random.NextInt(4);
        while (!Check((Direction)dir) || dir == (int)oppositeDir)
        {
            dir = (dir + 1) % 4;
        }
        return (Direction)dir;
    }

    public FixedString32Bytes ToFixedString()
    {
        var str = new FixedString32Bytes();
        if (Right) str.Append("R");
        if (Left) str.Append("L");
        if (Down) str.Append("D");
        if (Up) str.Append("U");
        if (Count == 0) str.Append("-");
        return str;
    }
}
