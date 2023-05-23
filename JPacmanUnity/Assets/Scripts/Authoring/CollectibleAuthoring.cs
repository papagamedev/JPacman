using Unity.Entities;
using UnityEngine;

public class CollectibleAuthoring : MonoBehaviour
{
    public int Score;
    public bool ScoreAnimation;
    public AudioEvents.SoundType SoundType;
    public float CollisionRadius;
    public bool IsPowerup;

    public class Baker : Baker<CollectibleAuthoring>
    {
        public override void Bake(CollectibleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Collectible
            {
                Score = authoring.Score,
                ScoreAnimation = authoring.ScoreAnimation,
                SoundType = authoring.SoundType,
                IsPowerup = authoring.IsPowerup
            });
            AddComponent(entity, new CollisionCircle()
            {
                Radius = authoring.CollisionRadius
            });
        }
    }
}


