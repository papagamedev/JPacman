using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct LevelsConfigData
{
    public BlobArray<LevelConfigData> LevelsData;
}

public struct LevelConfigData
{
    public int LevelNumber;
    public int RoundNumber;
    public float PlayerSpeed;
    public int MapId;
}

public struct MapsConfigData
{
    public BlobArray<MapConfigData> MapsData;
}

public struct MapConfigData
{
    public const char kDotChar = '#';
    public const char kPlayerChar = 'P';
    public const char kTunnelEntranceChar = 'E';
    public const char kFruitChar = 'F';
    public const char kGhostsExitChar = 'S';
    public const char kLabelsChar = 'L';
    public const char kGhostsHorizontalHomeChar = 'H';
    public const char kGhostsVerticalHomeChar = 'V';
    public const char kCookiesVerticalChar = 'G';
    public const char kCookiesHorizontalChar = 'C';
    public const char kWallChar = ' ';
    public const char kTunnelFirstChar = 'a';

    public int Id;
    public int Width;
    public int Height;
    public float2 PlayerPos;
    public BlobArray<char> MapData;

    public bool IsDot(int x, int y) => x > 0 && y > 0 && IsChar(x, y, kDotChar) && IsChar(x - 1, y, kDotChar) && IsChar(x, y - 1, kDotChar) && IsChar(x - 1, y - 1, kDotChar);
    public bool IsGhostsHorizontalHome(int x, int y) => IsChar(x, y, kGhostsHorizontalHomeChar);
    private bool IsChar(int x, int y, char c) => MapData[y * Width + x] == c;

    public float3 MapToWorldPos(float x, float y) => new float3(x - Width * 0.5f, Height * 0.5f - y, 0);
    public float3 MapToWorldPos(int x, int y) => MapToWorldPos((float)x, (float)y);
    public float3 MapToWorldPos(float2 mapPos) => MapToWorldPos(mapPos.x, mapPos.y);

    public float2 WorldToMapPos(float3 worldPos) => new float2(worldPos.x + Width * 0.5f, Height * 0.5f - worldPos.y);

    public bool IsDirectionAllowed(int x, int y, Movable.Direction direction)
    {
        switch (direction)
        {
            case Movable.Direction.Left:
                if (x > 1
                    && !IsChar(x - 2, y, kWallChar)
                    && !IsChar(x - 2, y - 1, kWallChar))
                {
                    return true;
                }
                break;
            case Movable.Direction.Right:
                if (x < Width - 1
                    && !IsChar(x + 1, y, kWallChar)
                    && !IsChar(x + 1, y - 1, kWallChar))
                {
                    return true;
                }
                break;
            case Movable.Direction.Up:
                if (y > 1
                    && !IsChar(x, y - 2, kWallChar)
                    && !IsChar(x - 1, y - 2, kWallChar))
                {
                    return true;
                }
                break;
            case Movable.Direction.Down:
                if (y < Height - 1
                    && !IsChar(x, y + 1, kWallChar)
                    && !IsChar(x - 1, y + 1, kWallChar))
                {
                    return true;
                }
                break;
        }
        return false;
    }
}

public struct SoundEventBufferElement : IBufferElementData
{
    public AudioEvents.SoundType SoundType;
}

public struct SoundStopEventBufferElement : IBufferElementData
{
    public AudioEvents.SoundType SoundType;
}

public struct MusicEventBufferElement : IBufferElementData
{
    public AudioEvents.MusicType MusicType;
}

public struct Main : IComponentData
{
    public Entity DotPrefab;
    public Entity PlayerPrefab;
    public Entity EnemyPrefab;
    public BlobAssetReference<LevelsConfigData> LevelsConfigBlob;
    public BlobAssetReference<MapsConfigData> MapsConfigBlob;
}

public readonly partial struct MainAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<Main> m_main;
    public readonly DynamicBuffer<SoundEventBufferElement> SoundEventBuffer;
    public readonly DynamicBuffer<SoundStopEventBufferElement> SoundStopEventBuffer;
    public readonly DynamicBuffer<MusicEventBufferElement> MusicEventBuffer;
}
