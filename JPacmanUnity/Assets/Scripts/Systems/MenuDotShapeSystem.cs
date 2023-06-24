using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct MenuDotShapeSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<MenuDotShape>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var menuDotShape = SystemAPI.GetComponentRO<MenuDotShape>(mainEntity);

        ref var shapesData = ref mainComponent.ValueRO.MenuDotShapeConfigBlob.Value;
        CreateDots(ref state, mainComponent, ref shapesData, menuDotShape.ValueRO.ShapeIdx, menuDotShape.ValueRO.ShapePos, mainEntity, ecb);

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);        
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var menuDotShape = SystemAPI.GetComponentRO<MenuDotShape>(mainEntity);
        ref var introData = ref mainComponent.ValueRO.IntroConfigBlob.Value;
        var deltaTime = SystemAPI.Time.DeltaTime;
        new MenuDotShapeJob
        {
            DeltaTime = deltaTime,
            ShapeIdx = menuDotShape.ValueRO.ShapeIdx,
            ShapePos = menuDotShape.ValueRO.ShapePos,
            DotSpeed = menuDotShape.ValueRO.DotSpeed,
            ShapesBlob = mainComponent.ValueRO.MenuDotShapeConfigBlob
        }.ScheduleParallel();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_, entity) in SystemAPI.Query<MenuAnimatedDot>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }


    private void CreateDots(ref SystemState state, RefRO<Main> mainComponent, ref MenuDotShapeConfigData shapesData, int shapeIdx, float2 shapePos, Entity mainEntity, EntityCommandBuffer ecb)
    {
        ref var dotsPos = ref shapesData.ShapesData[shapeIdx].DotPos;
        var dotsCount = dotsPos.Length;
        for (int i = 0; i < dotsCount; i++)
        {
            var dot = ecb.Instantiate(mainComponent.ValueRO.DotPrefab);
            ecb.SetComponent(dot,
                new LocalTransform()
                {
                    Position = new float3(dotsPos[i] + shapePos, 0),
                    Scale = 1.0f,
                    Rotation = quaternion.identity
                });
            ecb.RemoveComponent<Collectible>(dot);
            ecb.AddComponent(dot,
                new RotateAnimator()
                {
                    Speed = 30.0f
                });
            ecb.AddComponent(dot,
                new MenuAnimatedDot()
                {
                    Idx = i,
                });
        }
    }
}

public partial struct MenuDotShapeJob : IJobEntity
{
    public float DeltaTime;
    public int ShapeIdx;
    public float2 ShapePos;
    public float DotSpeed;
    public BlobAssetReference<MenuDotShapeConfigData> ShapesBlob;

    private void Execute(MenuAnimatedDotAspect dot)
    {
        dot.UpdateAnimation(DeltaTime, ShapeIdx, ShapePos, DotSpeed, ShapesBlob);
    }
}

