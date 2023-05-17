using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
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

        Entities.ForEach((Entity entity, SpriteRenderer sprite, SpriteAnimatorDef spriteDef, ref SpriteAnimator spriteAnimator) =>
        {
            sprite.sprite = spriteDef.AnimationFrames[spriteAnimator.Frame];
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, SpriteRenderer sprite, EnemyColorDef enemyDef, Enemy enemy) =>
        {
            sprite.color = enemy.Scared ? enemyDef.EnemyScaredColor : enemyDef.EnemyColors[enemy.Id];
        }).WithoutBurst().Run();
    }
}
