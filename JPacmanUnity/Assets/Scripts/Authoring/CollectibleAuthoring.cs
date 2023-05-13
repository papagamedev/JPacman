using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CollectibleAuthoring : MonoBehaviour
{
    public int Score;
    public bool ScoreAnimation;

    public class Baker : Baker<CollectibleAuthoring>
    {
        public override void Bake(CollectibleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Collectible
            {
                Score = authoring.Score,
                ScoreAnimation = authoring.ScoreAnimation
            });
        }
    }
}


