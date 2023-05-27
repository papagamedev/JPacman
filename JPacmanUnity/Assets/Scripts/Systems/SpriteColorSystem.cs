using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(SpriteAnimationSystem))]
public partial class SpriteColorSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<SpriteSetColor>();
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, SpriteRenderer sprite, in SpriteSetColor spriteColor) =>
        {
            sprite.color = spriteColor.Color;
        }).WithoutBurst().Run();
    }
}
