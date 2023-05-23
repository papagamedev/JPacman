using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MovableSystem))]
public partial class SpriteAnimationSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<SpriteAnimator>();
        RequireForUpdate<SpriteAnimatorDef>();
    }

    protected override void OnUpdate()
    {
        var time = SystemAPI.Time.ElapsedTime;
        Entities.ForEach((SpriteAnimatorAspect spriteAnimator) =>
        {
            spriteAnimator.UpdateAnimation((float)time);
        }).ScheduleParallel();

        Entities.ForEach((SpriteAnimCopyParentFrameAspect sprite) =>
        {
            sprite.UpdateAnimFrame(EntityManager);
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, SpriteAnimatorDef spriteDef, in SpriteSetFrame set) =>
        {
            sprite.sprite = spriteDef.AnimationFrames[set.Frame];
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, SpriteAnimatorDef spriteDef, in SpriteAnimator spriteAnimator) =>
        {
            sprite.sprite = spriteDef.AnimationFrames[spriteAnimator.Frame];
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, EnemyDef enemyDef, in Enemy enemy, in EnemyHomeTag enemyHome) =>
        {
            sprite.color = enemyDef.EnemyColors[enemy.Id];
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, EnemyDef enemyDef, in Enemy enemy, in EnemyFollowPlayerTag enemyFollowPlayerTag) =>
        {
            sprite.color = enemyDef.EnemyColors[enemy.Id];
        }).WithoutBurst().Run();

        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        var enemyScaredBlinking = gameAspect.IsEnemyScaredBlinking;

        Entities.ForEach((Entity entity, SpriteRenderer sprite, EnemyDef enemyDef, in EnemyScaredTag enemy) =>
        {
            sprite.color = enemyScaredBlinking ? enemyDef.EnemyScaredBlinkColor : enemyDef.EnemyScaredColor;
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, EnemyDef enemyDef, in EnemyHomeScaredTag enemy) =>
        {
            sprite.color = enemyScaredBlinking ? enemyDef.EnemyScaredBlinkColor : enemyDef.EnemyScaredColor;
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, EnemyDef enemyDef, in EnemyReturnHomeTag enemy) =>
        {
            sprite.color = enemyDef.EnemyReturnHomeColor;
        }).WithoutBurst().Run();
    }
}
