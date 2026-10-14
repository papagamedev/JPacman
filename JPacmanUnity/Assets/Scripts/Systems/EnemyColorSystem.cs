using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MovableSystem))]
public partial class EnemyColorSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<Enemy>();
        RequireForUpdate<PowerupMode>();
        RequireForUpdate<SpriteAnimator>();
        RequireForUpdate<SpriteAnimatorDef>();
    }

    protected override void OnUpdate()
    {
        foreach (var (sprite, enemyDef, enemy) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>, RefRO<EnemyDef>, RefRO<Enemy>>().WithAll<EnemyHomeTag>())
        {
            sprite.Value.color = enemyDef.ValueRO.EnemyColors[enemy.ValueRO.Id];
        }

        foreach (var (sprite, enemyDef, enemy) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>, RefRO<EnemyDef>, RefRO<Enemy>>().WithAll<EnemyFollowPlayerTag>())
        {
            sprite.Value.color = enemyDef.ValueRO.EnemyColors[enemy.ValueRO.Id];
        }

        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var powerupModeAspect = new PowerupModeAspect(ref CheckedStateRef, mainEntity);
        var enemyScaredBlinking = powerupModeAspect.IsEnemyScaredBlinking;

        foreach (var (sprite, enemyDef) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>, RefRO<EnemyDef>>().WithAll<EnemyScaredTag>())
        {
            sprite.Value.color = enemyScaredBlinking ? enemyDef.ValueRO.EnemyScaredBlinkColor : enemyDef.ValueRO.EnemyScaredColor;
        }

        foreach (var (sprite, enemyDef) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>, RefRO<EnemyDef>>().WithAll<EnemyHomeScaredTag>())
        {
            sprite.Value.color = enemyScaredBlinking ? enemyDef.ValueRO.EnemyScaredBlinkColor : enemyDef.ValueRO.EnemyScaredColor;
        }

        foreach (var (sprite, enemyDef) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>, RefRO<EnemyDef>>().WithAll<EnemyReturnHomeTag>())
        {
            sprite.Value.color = enemyDef.ValueRO.EnemyReturnHomeColor;
        }
    }
}
