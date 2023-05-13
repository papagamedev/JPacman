using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MovableAuthoring : MonoBehaviour
{
    public float Speed;

    public class Baker : Baker<MovableAuthoring>
    {
        public override void Bake(MovableAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Movable
            {
                Speed = authoring.Speed
            });
        }
    }
}


