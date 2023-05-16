using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpriteAnimatorAuthoring : MonoBehaviour
{
    public Sprite[] AnimationFrames;
    public float AnimationLength = 1.0f;
    public WrapMode WrapMode;
    public bool StartPlaying = true;

    public class Baker : Baker<SpriteAnimatorAuthoring>
    {
        public override void Bake(SpriteAnimatorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new SpriteAnimatorDef()
            {
                AnimationFrames = authoring.AnimationFrames
            });
            AddComponent(entity, new SpriteAnimator()
            {
                Frame = 0,
                FramesCount = authoring.AnimationFrames.Length,
                AnimationLength = authoring.AnimationLength,
                WrapMode = authoring.WrapMode,
                Backwards = false,
                Running = authoring.StartPlaying
            });
        }
    }
}


