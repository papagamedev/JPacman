using Unity.Entities;
using Unity.Mathematics;

public struct MenuDotShape : IComponentData
{
    public int ShapeIdx;
    public float2 ShapePos;
    public float DotSpeed;
}
