using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(IntroSystem))]
public partial struct MenuSystem : ISystem, ISystemStartStop
{
    Random m_random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Main>();
        state.RequireForUpdate<MenuPhase>();
    }


    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var menuPhase = SystemAPI.GetComponentRO<MenuPhase>(mainEntity);
        ecb.AppendToBuffer(mainEntity, new ShowUIBufferElement()
        {
            UI = menuPhase.ValueRO.UIType
        });
        ecb.AppendToBuffer(mainEntity, new MusicEventBufferElement()
        {
            MusicType = AudioEvents.MusicType.Menu
        });
        ecb.AddComponent(mainEntity, new MenuDotShape()
        {
            ShapeIdx = menuPhase.ValueRO.UIType == UIEvents.ShowUIType.Menu ? 0 : 1
        });
        var mainComponent = SystemAPI.GetComponentRW<Main>(mainEntity);
        m_random = new Random(mainComponent.ValueRW.RandomSeed++);
        ref var menuData = ref mainComponent.ValueRO.MenuConfigBlob.Value;
        CreateEnemies(ref state, mainComponent, ref menuData, mainEntity, ecb);
        CreatePlayer(ref state, mainComponent, ref menuData, mainEntity, ecb);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        ref var menuData = ref mainComponent.ValueRO.MenuConfigBlob.Value;
        UpdatePlayerEnemies(ref state, mainComponent, ref menuData, mainEntity, ecb);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<Main>(out var mainEntity))
        {
            return;
        }
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_, entity) in SystemAPI.Query<MenuSpriteTag>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }
        ecb.RemoveComponent<MenuDotShape>(mainEntity);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    private void CreatePlayer(ref SystemState state, RefRW<Main> mainComponent, ref MenuConfigData menuData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        GetNewPosAndDir(out var pos, out var dir, menuData.PlayerBoundsSize, ref m_random, false);
        var player = ecb.Instantiate(mainComponent.ValueRO.PlayerPrefab);
        ecb.SetComponent(player,
            new LocalTransform()
            {
                Position = pos,
                Scale = 1.0f,
                Rotation = quaternion.identity
            });
        ecb.AddComponent(player,
                new MoveAnimator()
                {
                    Speed = menuData.PlayerSpeed,
                    Direction = dir
                });
        ecb.AddComponent(player, new MenuSpriteTag());
    }

    private void CreateEnemies(ref SystemState state, RefRW<Main> mainComponent, ref MenuConfigData menuData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        for (int i = 0; i < 4; i++)
        {
            GetNewPosAndDir(out var pos, out var dir, menuData.EnemiesBoundsSize, ref m_random, true);
            var enemy = ecb.Instantiate(mainComponent.ValueRO.EnemyPrefab);
            ecb.SetComponent(enemy,
                new LocalTransform()
                {
                    Position = pos,
                    Scale = 1.0f,
                    Rotation = quaternion.identity,
                });
            ecb.AddComponent(enemy,
                new MoveAnimator()
                {
                    Speed = menuData.EnemySpeed + i * 1.0f,
                    Direction = dir
                });
            ecb.AddComponent(enemy,
                new Enemy()
                {
                    Id = i
                });
            ecb.AddComponent(enemy,
                new SpriteSetColor()
                {
                    Color = UnityEngine.Color.white
                });
            ecb.AddComponent(enemy, new MenuSpriteTag());
        }
    }

    private void UpdatePlayerEnemies(ref SystemState state, RefRO<Main> mainComponent, ref MenuConfigData menuData, Entity mainEntity, EntityCommandBuffer ecb)
    {
        new UpdateMenuPlayerJob
        {
            BoundsSize = menuData.PlayerBoundsSize,
            RandomSeed = m_random.NextUInt()
        }.ScheduleParallel();
        m_random.NextInt();
        new UpdateMenuEnemiesJob
        {
            BoundsSize = menuData.EnemiesBoundsSize,
            RandomSeed = m_random.NextUInt()
        }.ScheduleParallel();
    }

    static public void GetNewPosAndDir(out float3 pos, out Direction dir, float2 boundsSize, ref Random rand, bool canUseUpDown)
    {
        dir = rand.NextInt(canUseUpDown ? 4 : 2);
        switch (dir)
        {
            case Direction.Up:
                pos = new float3(rand.NextFloat(-boundsSize.x, boundsSize.x), boundsSize.y, 0);
                break;
            case Direction.Down:
                pos = new float3(rand.NextFloat(-boundsSize.x, boundsSize.x), -boundsSize.y, 0);
                break;
            case Direction.Left:
                pos = new float3(boundsSize.x, rand.NextFloat(-boundsSize.y, boundsSize.y), 0);
                break;
            case Direction.Right:
            default:
                pos = new float3(-boundsSize.x, rand.NextFloat(-boundsSize.y, boundsSize.y), 0);
                break;
        }
    }

    static public bool IsOutOfBounds(float3 pos, float2 boundsSize)
    {
        return (math.abs(pos.x) > boundsSize.x) || (math.abs(pos.y) > boundsSize.y);
    }

    public static void UpdatePosDirAnim(RefRW<LocalTransform> transform, RefRW<MoveAnimator> animator, RefRW<SpriteAnimator> spriteAnim, float2 boundsSize, ref Random random, bool canUseUpDown)
    {
        if (IsOutOfBounds(transform.ValueRO.Position, boundsSize))
        {
            GetNewPosAndDir(out var pos, out var dir, boundsSize, ref random, canUseUpDown);
            animator.ValueRW.Direction = dir;
            transform.ValueRW.Position = pos;
        }

        var direction = animator.ValueRO.Direction;
        int framesPerDirAnim = spriteAnim.ValueRO.FramesCount / 4;
        spriteAnim.ValueRW.StartFrame = framesPerDirAnim * (int)direction;
        spriteAnim.ValueRW.LastFrame = framesPerDirAnim * (int)direction + framesPerDirAnim - 1;
    }
}

public partial struct UpdateMenuPlayerJob : IJobEntity
{
    public float2 BoundsSize;
    public uint RandomSeed;

    private void Execute(RefRO<Player> player, RefRW<LocalTransform> transform, RefRW<MoveAnimator> animator, RefRW<SpriteAnimator> spriteAnim)
    {
        var rand = new Random(RandomSeed);
        MenuSystem.UpdatePosDirAnim(transform, animator, spriteAnim, BoundsSize, ref rand, false);
    }

}

public partial struct UpdateMenuEnemiesJob : IJobEntity
{
    public float2 BoundsSize;
    public uint RandomSeed;

    private void Execute(RefRO<Enemy> enemy, RefRW<LocalTransform> transform, RefRW<MoveAnimator> animator, RefRW<SpriteAnimator> spriteAnim, RefRW<SpriteSetColor> spriteColor, RefRO<EnemyDef> enemyDef)
    {
        var enemyId = enemy.ValueRO.Id;
        var rand = new Random((uint)(RandomSeed * (enemyId + 1)));
        MenuSystem.UpdatePosDirAnim(transform, animator, spriteAnim, BoundsSize, ref rand, true);

        spriteColor.ValueRW.Color = enemyDef.ValueRO.EnemyColors[enemy.ValueRO.Id];
    }
}
