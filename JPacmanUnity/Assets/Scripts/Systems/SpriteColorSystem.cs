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
        foreach (var (sprite, spriteColor) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>, RefRO<SpriteSetColor>>())
        {
            sprite.Value.color = spriteColor.ValueRO.Color;
        }
    }
}
