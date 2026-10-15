using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// Sprite drawn by Entities Graphics: the MeshRenderer must use the SpriteQuad mesh and a JPacman/Sprite
// material with the frames' texture. Draw order comes from the material's render queue.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SpriteAuthoring : MonoBehaviour
{
    // All frames must come from the same texture. Frame 0 is shown initially.
    public Sprite[] Frames;
    public Color Color = Color.white;

#if UNITY_EDITOR
    // Shows frame 0 in the editor; baking ignores property blocks
    private void OnValidate()
    {
        if (Frames == null || Frames.Length == 0 || Frames[0] == null)
        {
            return;
        }
        var frame = GetSpriteFrame(Frames[0]);
        var block = new MaterialPropertyBlock();
        block.SetVector("_SpriteUV", frame.UVRect);
        block.SetVector("_SpriteQuad", frame.QuadRect);
        block.SetColor("_Color", Color);
        GetComponent<MeshRenderer>().SetPropertyBlock(block);
    }
#endif

    private static SpriteFrame GetSpriteFrame(Sprite sprite)
    {
        var texture = sprite.texture;
        var rect = sprite.rect;
        var texSize = new float2(texture.width, texture.height);
        var ppu = sprite.pixelsPerUnit;
        return new SpriteFrame()
        {
            UVRect = new float4(rect.x / texSize.x, rect.y / texSize.y, rect.width / texSize.x, rect.height / texSize.y),
            QuadRect = new float4(-sprite.pivot.x / ppu, -sprite.pivot.y / ppu, rect.width / ppu, rect.height / ppu)
        };
    }

    public class Baker : Baker<SpriteAuthoring>
    {
        public override void Bake(SpriteAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<SpriteFramesBlob>();
            var frames = builder.Allocate(ref root.Frames, authoring.Frames.Length);
            for (int i = 0; i < authoring.Frames.Length; i++)
            {
                var sprite = authoring.Frames[i];
                DependsOn(sprite);
                frames[i] = GetSpriteFrame(sprite);
            }
            var blob = builder.CreateBlobAssetReference<SpriteFramesBlob>(Allocator.Persistent);
            builder.Dispose();
            AddBlobAsset(ref blob, out _);

            var firstFrame = authoring.Frames.Length > 0 ? blob.Value.Frames[0] : default;
            AddComponent(entity, new SpriteFrames() { Blob = blob });
            AddComponent(entity, new SpriteUVProperty() { Value = firstFrame.UVRect });
            AddComponent(entity, new SpriteQuadProperty() { Value = firstFrame.QuadRect });
            AddComponent(entity, SpriteColorProperty.FromColor(authoring.Color));
        }
    }
}
