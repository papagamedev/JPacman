using Unity.Entities;
using UnityEngine;

// Animates the frames of the SpriteAuthoring on the same GameObject
[RequireComponent(typeof(SpriteAuthoring))]
public class SpriteAnimatorAuthoring : MonoBehaviour
{
    public float AnimationLength = 1.0f;
    public WrapMode WrapMode;
    public bool StartPlaying = true;

    public class Baker : Baker<SpriteAnimatorAuthoring>
    {
        public override void Bake(SpriteAnimatorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var framesCount = GetComponent<SpriteAuthoring>().Frames.Length;
            if (authoring.AnimationLength > 0)
            {
                AddComponent(entity, new SpriteAnimator()
                {
                    StartTime = 0,
                    Frame = 0,
                    FramesCount = framesCount,
                    StartFrame = 0,
                    LastFrame = framesCount - 1,
                    AnimationLength = authoring.AnimationLength,
                    WrapMode = authoring.WrapMode,
                    Backwards = false,
                    Running = authoring.StartPlaying
                });
            }
        }
    }
}
