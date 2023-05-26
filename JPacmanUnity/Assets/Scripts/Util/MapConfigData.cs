using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct MapConfigData
{
    public const char kDotChar = '#';
    public const char kPlayerChar = 'P';
    public const char kTunnelEntranceChar = 'E';
    public const char kFruitChar = 'F';
    public const char kEnemyExitChar = 'S';
    public const char kLabelsChar = 'L';
    public const char kEnemyHorizontalHomeChar = 'H';
    public const char kEnemyVerticalHomeChar = 'V';
    public const char kPowerupVerticalChar = 'G';
    public const char kPowerupHorizontalChar = 'C';
    public const char kWallChar = ' ';
    public const char kTunnelFirstChar = 'a';
    public const char kTunnelLastChar = 'z';

    public int Id;
    public int Width;
    public int Height;
    public half2 PlayerPos;
    public half2 EnemyHousePos;
    public half2 EnemyExitPos;
    public half2 LabelMessagePos;
    public half2 FruitPos;
    public FixedList32Bytes<half2> PowerupPos;
    public FixedList128Bytes<half2> TunnelPos;
    public Direction EnemyExitDir;
    public BlobArray<char> MapData;

    public bool IsDot(int x, int y) => x > 0 && y > 0 && IsChar(x, y, kDotChar) && IsChar(x - 1, y, kDotChar) && IsChar(x, y - 1, kDotChar) && IsChar(x - 1, y - 1, kDotChar);
    public bool IsEnemyHorizontalHome(int x, int y) => IsChar(x, y, kEnemyHorizontalHomeChar);
    public bool IsEnemyVerticalHome(int x, int y) => IsChar(x, y, kEnemyVerticalHomeChar);
    public bool IsWall(int x, int y) => IsChar(x, y, kWallChar);
    public bool IsTunnelEntrance(float2 mapPos) => IsTunnelEntrance((int)mapPos.x, (int)mapPos.y);
    public bool IsTunnelEntrance(int x, int y) => IsChar(x, y, kTunnelEntranceChar);
    public bool IsTunnel(float2 mapPos, out int tunnelIdx) => IsTunnel((int)mapPos.x, (int)mapPos.y, out tunnelIdx);
    public bool IsTunnel(int x, int y, out int tunnelIdx)
    {
        var c = MapData[y * Width + x];
        var isTunnel = c >= kTunnelFirstChar && c <= kTunnelLastChar;
        tunnelIdx = isTunnel ? c - kTunnelFirstChar : -1;
        return isTunnel;
    }
    private bool IsChar(int x, int y, char c) => MapData[y * Width + x] == c;

    public float3 MapToWorldPos(float x, float y) => new float3(x - Width * 0.5f, Height * 0.5f - y, 0);
    public float3 MapToWorldPos(int x, int y) => MapToWorldPos((float)x, (float)y);
    public float3 MapToWorldPos(float2 mapPos) => MapToWorldPos(mapPos.x, mapPos.y);

    public float2 WorldToMapPos(float3 worldPos) => new float2(worldPos.x + Width * 0.5f, Height * 0.5f - worldPos.y);

    public static int OppositeTunnelIdx(int tunnelIdx) => tunnelIdx ^ 1;

    public bool IsDirectionAllowed(int x, int y, Direction direction, bool lastTry = false)
    {
        switch (direction)
        {
            case Direction.Left:
                if (x > 1
                    && !IsWall(x - 2, y)
                    && !IsWall(x - 2, y - 1))
                {
                    return true;
                }
                break;
            case Direction.Right:
                if (x < Width - 1
                    && !IsWall(x + 1, y)
                    && !IsWall(x + 1, y - 1))
                {
                    return true;
                }
                break;
            case Direction.Up:
                if (y > 1
                    && !IsWall(x, y - 2)
                    && !IsWall(x - 1, y - 2))
                {
                    return true;
                }
                break;
            case Direction.Down:
                if (y < Height - 1
                    && !IsWall(x, y + 1)
                    && !IsWall(x - 1, y + 1))
                {
                    return true;
                }
                break;
        }

        if (lastTry || !IsTunnel(x, y, out var tunnelIdx))
        {
            return false;
        }

        tunnelIdx = OppositeTunnelIdx(tunnelIdx);
        x = (int) TunnelPos[tunnelIdx].x;
        y = (int) TunnelPos[tunnelIdx].y;
        return IsDirectionAllowed(x, y, direction, true);
    }
}
