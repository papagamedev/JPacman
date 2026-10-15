using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(SpriteAnimationSystem))]
public partial struct SpriteColorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpriteSetColor>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new SpriteSetColorJob().ScheduleParallel();
    }
}

[BurstCompile]
[WithChangeFilter(typeof(SpriteSetColor))]
public partial struct SpriteSetColorJob : IJobEntity
{
    private void Execute(in SpriteSetColor spriteColor, ref SpriteColorProperty color)
    {
        color = SpriteColorProperty.FromColor(spriteColor.Color);
    }
}
