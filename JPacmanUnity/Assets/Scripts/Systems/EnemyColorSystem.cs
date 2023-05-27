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
        Entities.ForEach((Entity entity, SpriteRenderer sprite, in EnemyDef enemyDef, in Enemy enemy, in EnemyHomeTag enemyHome) =>
        {
            sprite.color = enemyDef.EnemyColors[enemy.Id];
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, in EnemyDef enemyDef, in Enemy enemy, in EnemyFollowPlayerTag enemyFollowPlayerTag) =>
        {
            sprite.color = enemyDef.EnemyColors[enemy.Id];
        }).WithoutBurst().Run();

        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var powerupModeAspect = SystemAPI.GetAspect<PowerupModeAspect>(mainEntity);
        var enemyScaredBlinking = powerupModeAspect.IsEnemyScaredBlinking;

        Entities.ForEach((Entity entity, SpriteRenderer sprite, in EnemyDef enemyDef, in EnemyScaredTag enemy) =>
        {
            sprite.color = enemyScaredBlinking ? enemyDef.EnemyScaredBlinkColor : enemyDef.EnemyScaredColor;
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, in EnemyDef enemyDef, in EnemyHomeScaredTag enemy) =>
        {
            sprite.color = enemyScaredBlinking ? enemyDef.EnemyScaredBlinkColor : enemyDef.EnemyScaredColor;
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, in EnemyDef enemyDef, in EnemyReturnHomeTag enemy) =>
        {
            sprite.color = enemyDef.EnemyReturnHomeColor;
        }).WithoutBurst().Run();
    }
}
