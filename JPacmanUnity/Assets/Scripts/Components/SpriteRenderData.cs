using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

// Where a sprite frame is in its texture, and the quad it's drawn on, baked from a UnityEngine.Sprite
public struct SpriteFrame
{
    // xy: UV offset, zw: UV size
    public float4 UVRect;
    // xy: quad offset from the pivot, zw: quad size, in world units
    public float4 QuadRect;
}

public struct SpriteFramesBlob
{
    public BlobArray<SpriteFrame> Frames;
}

public struct SpriteFrames : IComponentData
{
    public BlobAssetReference<SpriteFramesBlob> Blob;
}

// Per-entity properties of the JPacman/Sprite shader

[MaterialProperty("_SpriteUV")]
public struct SpriteUVProperty : IComponentData
{
    public float4 Value;
}

[MaterialProperty("_SpriteQuad")]
public struct SpriteQuadProperty : IComponentData
{
    public float4 Value;
}

[MaterialProperty("_Color")]
public struct SpriteColorProperty : IComponentData
{
    public float4 Value;

    // Like SpriteRenderer.color, takes a gamma-space color. The project uses linear color space,
    // and DOTS instanced properties aren't converted for us (Color.linear isn't Burst-compatible)
    public static SpriteColorProperty FromColor(Color color)
    {
        var c = new float3(color.r, color.g, color.b);
        var linear = math.select(math.pow((c + 0.055f) / 1.055f, 2.4f), c / 12.92f, c <= 0.04045f);
        return new SpriteColorProperty() { Value = new float4(linear, color.a) };
    }
}
