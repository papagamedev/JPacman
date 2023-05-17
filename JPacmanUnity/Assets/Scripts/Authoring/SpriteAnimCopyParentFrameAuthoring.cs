using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpriteAnimCopyParentFrameAuthoring : MonoBehaviour
{

    public class Baker : Baker<SpriteAnimCopyParentFrameAuthoring>
    {
        public override void Bake(SpriteAnimCopyParentFrameAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpriteAnimCopyParentFrame()
            {
            });
        }
    }
}


