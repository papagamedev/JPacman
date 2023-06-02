using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial struct PowerupsMovingSystem: ISystem , ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LevelPlayingPhaseTag>();
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<PowerupsMovingTag>();
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        ref var map = ref gameAspect.GetCurrentMapData();
        var powerupsMoveSpeed = gameAspect.LevelData.PowerupsMoveSpeed;
        foreach (var (_, entity) in SystemAPI.Query<Powerup>().WithEntityAccess())
        {
            var rand = new Random(gameAspect.RandomSeed);
            ecb.AddComponent(entity, new Movable()
            {
                Speed = powerupsMoveSpeed,
                SpeedInTunnel = powerupsMoveSpeed,
                AllowChangeDirInMidCell = false,
                CurrentDir = Direction.None,
                DesiredDir = Direction.None,
                Rand = rand
            });
            ecb.AddComponent(entity,
                new RandomMover()
                {
                    RandomMapPos = map.NewRandomMapPos(ref rand)
                });
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_, entity) in SystemAPI.Query<Powerup>().WithEntityAccess())
        {
            ecb.RemoveComponent<Movable>(entity);
            ecb.RemoveComponent<RandomMover>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
