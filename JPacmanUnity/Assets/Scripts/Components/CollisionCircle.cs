using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct CollisionCircle : IComponentData
{
    public float Radius;

    public static bool CheckCollision(float2 pos1, float2 pos2, float collisionRadius)
    {
        var dx = pos1.x - pos2.x;
        var dy = pos1.y - pos2.y;
        var dist2 = dx * dx + dy * dy;
        return (dist2 <= collisionRadius);
    }
}
