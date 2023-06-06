using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyColorSystem))]
[UpdateAfter(typeof(SpriteColorSystem))]
public partial class SpriteOpacitySystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<SpriteSetOpacity>();
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, SpriteRenderer sprite, in SpriteSetOpacity spriteOpacity) =>
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, spriteOpacity.Opacity);
        }).WithoutBurst().Run();
    }
}
