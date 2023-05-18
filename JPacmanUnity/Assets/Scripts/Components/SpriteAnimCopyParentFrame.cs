using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct SpriteAnimCopyParentFrame : IComponentData { }

public readonly partial struct SpriteAnimCopyParentFrameAspect : IAspect
{
    public readonly Entity Entity;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly RefRO<SpriteAnimCopyParentFrame> m_copyFromParent;
#pragma warning restore IDE0052 // Remove unread private members
    private readonly RefRW<SpriteAnimator> m_animator;
    private readonly RefRO<Parent> m_parent;

    public void UpdateAnimFrame(EntityManager manager)
    {
        var parentAnimator = manager.GetComponentData<SpriteAnimator>(m_parent.ValueRO.Value);
        m_animator.ValueRW.Frame = parentAnimator.Frame;
    }
}
