using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct SpriteAnimatedMovableAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<Movable> m_movable;
    private readonly RefRW<SpriteAnimator> m_animator;

    public void UpdateAnimation()
    {
        var currentDir = m_movable.ValueRO.CurrentDir;
        if (currentDir == Movable.Direction.None)
        {
            return;
        }
        int framesPerDirAnim = m_animator.ValueRO.FramesCount / 4;
        m_animator.ValueRW.StartFrame = framesPerDirAnim * (int)currentDir;
        m_animator.ValueRW.LastFrame = framesPerDirAnim * (int)currentDir + framesPerDirAnim - 1;
    }
}
