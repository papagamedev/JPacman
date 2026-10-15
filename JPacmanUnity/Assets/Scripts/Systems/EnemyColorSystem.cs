using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MovableSystem))]
public partial struct EnemyColorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Enemy>();
        state.RequireForUpdate<PowerupMode>();
        state.RequireForUpdate<SpriteFrames>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (color, enemyDef, enemy) in SystemAPI.Query<RefRW<SpriteColorProperty>, RefRO<EnemyDef>, RefRO<Enemy>>().WithAll<EnemyHomeTag>())
        {
            color.ValueRW = SpriteColorProperty.FromColor(enemyDef.ValueRO.EnemyColors[enemy.ValueRO.Id]);
        }

        foreach (var (color, enemyDef, enemy) in SystemAPI.Query<RefRW<SpriteColorProperty>, RefRO<EnemyDef>, RefRO<Enemy>>().WithAll<EnemyFollowPlayerTag>())
        {
            color.ValueRW = SpriteColorProperty.FromColor(enemyDef.ValueRO.EnemyColors[enemy.ValueRO.Id]);
        }

        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var powerupModeAspect = new PowerupModeAspect(mainEntity, SystemAPI.GetComponentRW<PowerupMode>(mainEntity),
            SystemAPI.GetBuffer<PowerupCollectedBufferElement>(mainEntity), SystemAPI.GetBuffer<EnemyEatenBufferElement>(mainEntity),
            SystemAPI.GetBuffer<EnemyReturnedHomeBufferElement>(mainEntity));
        var enemyScaredBlinking = powerupModeAspect.IsEnemyScaredBlinking;

        foreach (var (color, enemyDef) in SystemAPI.Query<RefRW<SpriteColorProperty>, RefRO<EnemyDef>>().WithAll<EnemyScaredTag>())
        {
            color.ValueRW = SpriteColorProperty.FromColor(enemyScaredBlinking ? enemyDef.ValueRO.EnemyScaredBlinkColor : enemyDef.ValueRO.EnemyScaredColor);
        }

        foreach (var (color, enemyDef) in SystemAPI.Query<RefRW<SpriteColorProperty>, RefRO<EnemyDef>>().WithAll<EnemyHomeScaredTag>())
        {
            color.ValueRW = SpriteColorProperty.FromColor(enemyScaredBlinking ? enemyDef.ValueRO.EnemyScaredBlinkColor : enemyDef.ValueRO.EnemyScaredColor);
        }

        foreach (var (color, enemyDef) in SystemAPI.Query<RefRW<SpriteColorProperty>, RefRO<EnemyDef>>().WithAll<EnemyReturnHomeTag>())
        {
            color.ValueRW = SpriteColorProperty.FromColor(enemyDef.ValueRO.EnemyReturnHomeColor);
        }
    }
}
