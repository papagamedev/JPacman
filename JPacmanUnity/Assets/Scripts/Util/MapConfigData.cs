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
    public bool IsEnemyExit(int x, int y) => IsChar(x, y, kEnemyExitChar) 
        || (x < Width - 1 && IsChar(x, y, kWallChar) && IsChar(x + 1, y, kEnemyExitChar))
        || (y < Height - 1 && IsChar(x, y, kWallChar) && IsChar(x, y + 1, kEnemyExitChar));
    public bool IsWall(int x, int y) => IsChar(x, y, kWallChar) || IsEnemyExit(x, y);
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

    public float2 NewRandomMapPos(ref Random rand) => new float2(rand.NextInt(Width), rand.NextInt(Height));
}

public struct MapsConfigData
{
    public BlobArray<MapConfigData> MapsData;

    public static BlobAssetReference<MapsConfigData> CreateMapsConfigBlob(MapConfig[] maps)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var mapsConfigData = ref builder.ConstructRoot<MapsConfigData>();
        var mapsCount = maps.Length;
        var mapsArrayBuilder = builder.Allocate(ref mapsConfigData.MapsData, mapsCount);
        for (int i = 0; i < mapsCount; i++)
        {
            var map = maps[i].Map;
            var mapDataSize = map.m_height * map.m_width;
            mapsArrayBuilder[i].Id = i;
            mapsArrayBuilder[i].Width = map.m_width;
            mapsArrayBuilder[i].Height = map.m_height;
            var arrayBuilder = builder.Allocate(ref mapsArrayBuilder[i].MapData, mapDataSize);
            var enemyHousePos = half2.zero;
            var enemyExitPos = half2.zero;
            for (int y = 0; y < map.m_height; y++)
            {
                for (int x = 0; x < map.m_width; x++)
                {
                    var c = map.m_data[x, y];
                    arrayBuilder[y * map.m_width + x] = c;

                    if (c == MapConfigData.kPlayerChar)
                    {
                        mapsArrayBuilder[i].PlayerPos = new half2((half)x, (half)y);
                    }
                    else if (c == MapConfigData.kLabelsChar)
                    {
                        mapsArrayBuilder[i].LabelMessagePos = new half2((half)x, (half)y);
                    }
                    else if (c == MapConfigData.kFruitChar)
                    {
                        mapsArrayBuilder[i].FruitPos = new half2((half)x, (half)y);
                    }
                    else if (c == MapConfigData.kEnemyExitChar)
                    {
                        enemyExitPos = new half2((half)x, (half)y);
                    }
                    else if (c >= MapConfigData.kTunnelFirstChar && c <= MapConfigData.kTunnelLastChar)
                    {
                        int tunnelIdx = c - MapConfigData.kTunnelFirstChar;
                        var length = mapsArrayBuilder[i].TunnelPos.Length;
                        if (tunnelIdx >= length)
                        {
                            var addCount = tunnelIdx - length + 1;
                            mapsArrayBuilder[i].TunnelPos.AddReplicate(half2.zero, addCount);
                        }
                        mapsArrayBuilder[i].TunnelPos[tunnelIdx] = new half2((half)x, (half)y);
                    }
                    else if (c == MapConfigData.kEnemyHorizontalHomeChar ||
                        c == MapConfigData.kEnemyVerticalHomeChar)
                    {
                        enemyHousePos = new half2((half)x, (half)y);
                    }
                    else if (c == MapConfigData.kPowerupVerticalChar)
                    {
                        mapsArrayBuilder[i].PowerupPos.Add(new half2((half)x, (half)(y + 0.5f)));
                    }
                    else if (c == MapConfigData.kPowerupHorizontalChar)
                    {
                        mapsArrayBuilder[i].PowerupPos.Add(new half2((half)(x + 0.5f), (half)y));
                    }
                }
            }

            if (enemyHousePos.x == enemyExitPos.x)
            {
                if (enemyHousePos.y > enemyExitPos.y)
                {
                    mapsArrayBuilder[i].EnemyExitDir = Direction.Up;
                    mapsArrayBuilder[i].EnemyExitPos = new half2(enemyExitPos.x, (half)(enemyExitPos.y - 1));
                }
                else
                {
                    mapsArrayBuilder[i].EnemyExitDir = Direction.Down;
                    mapsArrayBuilder[i].EnemyExitPos = new half2(enemyExitPos.x, (half)(enemyExitPos.y + 2));
                }
            }
            else
            {
                if (enemyHousePos.x > enemyExitPos.x)
                {
                    mapsArrayBuilder[i].EnemyExitDir = Direction.Left;
                    mapsArrayBuilder[i].EnemyExitPos = new half2((half)(enemyExitPos.x - 1), enemyExitPos.y);
                }
                else
                {
                    mapsArrayBuilder[i].EnemyExitDir = Direction.Right;
                    mapsArrayBuilder[i].EnemyExitPos = new half2((half)(enemyExitPos.x + 2), enemyExitPos.y);
                }
            }
            mapsArrayBuilder[i].EnemyHousePos = enemyHousePos;
        }
        var result = builder.CreateBlobAssetReference<MapsConfigData>(Allocator.Persistent);
        builder.Dispose();
        return result;
    }
}

