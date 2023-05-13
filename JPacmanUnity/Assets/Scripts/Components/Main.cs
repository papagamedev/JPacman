using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
}

public struct Main : IComponentData
{
    public Entity DotPrefab;
    public Entity PlayerPrefab;
    public Entity EnemyPrefab;
    public BlobAssetReference<MapConfigData> MapConfigBlob;
}
