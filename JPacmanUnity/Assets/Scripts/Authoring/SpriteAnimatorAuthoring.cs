using Unity.Entities;
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
            if (authoring.AnimationLength > 0)
            {
                AddComponent(entity, new SpriteAnimator()
                {
                    StartTime = 0,
                    Frame = 0,
                    FramesCount = authoring.AnimationFrames.Length,
                    StartFrame = 0,
                    LastFrame = authoring.AnimationFrames.Length - 1,
                    AnimationLength = authoring.AnimationLength,
                    WrapMode = authoring.WrapMode,
                    Backwards = false,
                    Running = authoring.StartPlaying
                });
            }
        }
    }
}


