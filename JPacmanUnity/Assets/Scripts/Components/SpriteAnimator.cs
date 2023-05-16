using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpriteAnimatorDef : IComponentData
{
    public Sprite[] AnimationFrames;
}

public struct SpriteAnimator : IComponentData, IEnableableComponent
{
    public float StartTime;
    public int Frame;
    public int FramesCount;
    public float AnimationLength;
    public WrapMode WrapMode;
    public bool Running;
    public bool Backwards;
}

public readonly partial struct SpriteAnimatorAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRW<SpriteAnimator> m_animator;

    public void UpdateAnimation(float time)
    {
        if (!m_animator.ValueRO.Running)
        {
            return;
        }
        float currentTime = UpdateAnimationTime(time);
        m_animator.ValueRW.Frame = UpdateAnimationFrame(currentTime);
    }

    private float UpdateAnimationTime(float time)
    {
        float currentTime = time - m_animator.ValueRO.StartTime;
        if (currentTime > m_animator.ValueRO.AnimationLength)
        {
            switch (m_animator.ValueRO.WrapMode)
            {
                case WrapMode.Default:
                case WrapMode.Once:
                    currentTime = 0.0f;
                    m_animator.ValueRW.Running = false;
                    break;

                case WrapMode.ClampForever:
                    m_animator.ValueRW.Running = false;
                    break;

                case WrapMode.Loop:
                    currentTime -= m_animator.ValueRO.AnimationLength;
                    m_animator.ValueRW.StartTime += m_animator.ValueRO.AnimationLength;
                    break;

                case WrapMode.PingPong:
                    currentTime -= m_animator.ValueRO.AnimationLength;
                    m_animator.ValueRW.StartTime += m_animator.ValueRO.AnimationLength;
                    m_animator.ValueRW.Backwards = !m_animator.ValueRO.Backwards;
                    break;
            }
        }

        return currentTime;
    }

    int UpdateAnimationFrame(float currentTime)
    {
        int frameCount = m_animator.ValueRO.FramesCount;
        if (m_animator.ValueRO.WrapMode == WrapMode.PingPong)
        {
            frameCount--;
        }
        int frame = (int)(currentTime * frameCount / m_animator.ValueRO.AnimationLength);
        if (frame >= frameCount)
        {
            frame = frameCount - 1;
        }
        if (m_animator.ValueRO.Backwards)
        {
            frame = frameCount - frame;
        }
        return frame;
    }
}
