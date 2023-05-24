using Unity.Entities;
using Unity.Mathematics;

public struct PowerupMode : IComponentData
{
    public float ActiveTime;
    public bool EnemiesBlinking;
    public int EnemyScore;
    public int DefaultEnemyScore;
    public float EnemyScaredTime;
    public int EnemyScaredCount;
    public int EnemyReturnHomeCount;
    public bool BonusLevel;
}

public struct PowerupModeActiveTag : IComponentData { }

public struct PowerupCollectedBufferElement : IBufferElementData
{
    public float2 CollectedPos;
}

public struct EnemyEatenBufferElement : IBufferElementData
{
    public float3 WorldPos;
}

public struct EnemyReturnedHomeBufferElement : IBufferElementData
{
    public byte Dummy;
}

public readonly partial struct PowerupModeAspect : IAspect
{
    const float kBlinkTime = 3.0f;
    const float kBlinkFreq = 0.5f;

    public readonly Entity Entity;
    private readonly RefRO<Main> m_main;
    private readonly RefRW<PowerupMode> m_powerupMode;
    private readonly DynamicBuffer<PowerupCollectedBufferElement> m_powerupCollectedBuffer;
    private readonly DynamicBuffer<EnemyEatenBufferElement> m_enemyEatenBuffer;
    private readonly DynamicBuffer<EnemyReturnedHomeBufferElement> m_enemyReturnedHomeBuffer;

    public void InitLive(LevelConfigData levelData)
    {
        m_powerupMode.ValueRW.EnemyScaredTime = levelData.EnemyScaredTime;
        m_powerupMode.ValueRW.DefaultEnemyScore = levelData.EnemyScore;
        m_powerupMode.ValueRW.EnemyScore = levelData.EnemyScore;
        m_powerupMode.ValueRW.BonusLevel = levelData.BonusLevel;
        m_powerupMode.ValueRW.EnemyScaredCount = 0;
        m_powerupMode.ValueRW.EnemyReturnHomeCount = 0;
    }

    public void AddEnemyScaredCount(int count) => m_powerupMode.ValueRW.EnemyScaredCount += count;

    public bool CheckPowerupCollected()
    {
        return !m_powerupCollectedBuffer.IsEmpty;
    }

    public bool ProcessPowerupCollected(Entity mainEntity, EntityCommandBuffer ecb)
    {
        if (m_powerupCollectedBuffer.IsEmpty)
        {
            return false;
        }
        m_powerupCollectedBuffer.Clear();

        m_powerupMode.ValueRW.ActiveTime = 0;
        m_powerupMode.ValueRW.EnemiesBlinking = false;
        if (m_powerupMode.ValueRO.BonusLevel && m_powerupMode.ValueRO.EnemyScore > m_powerupMode.ValueRO.DefaultEnemyScore * 2)
        {
            m_powerupMode.ValueRW.EnemyScore /= 4;
        }
        else
        {
            m_powerupMode.ValueRW.EnemyScore = m_powerupMode.ValueRO.DefaultEnemyScore;
        }

        return true;
    }

    public void UpdateActive(float deltaTime, Entity mainEntity, EntityCommandBuffer ecb)
    {
        CheckEnemiesEaten(mainEntity, ecb);

        m_powerupMode.ValueRW.ActiveTime += deltaTime;        
        var scaredTime = m_powerupMode.ValueRO.ActiveTime;
        var enemyScaredTime = m_powerupMode.ValueRO.EnemyScaredTime;
        var scaredEnemiesCount = m_powerupMode.ValueRO.EnemyScaredCount;
        if (scaredTime > enemyScaredTime || scaredEnemiesCount == 0)
        {
            ecb.RemoveComponent<PowerupModeActiveTag>(mainEntity);
        }
        else if (scaredTime > enemyScaredTime - kBlinkTime)
        {
            var scaredBlink = math.fmod(scaredTime, kBlinkFreq) < kBlinkFreq * 0.5f;
            SetEnemyScaredBlinking(scaredBlink);
        }

    }

    public void CheckEnemiesEaten(Entity mainEntity, EntityCommandBuffer ecb)
    {
        foreach (var item in m_enemyEatenBuffer)
        {
            ecb.AppendToBuffer(mainEntity, new AddScoreBufferElement()
            {
                WorldPos = item.WorldPos,
                Score = m_powerupMode.ValueRO.EnemyScore,
                ScoreAnimation = true
            });

            m_powerupMode.ValueRW.EnemyScore *= 2;
            m_powerupMode.ValueRW.EnemyScaredCount--;

            m_powerupMode.ValueRW.EnemyReturnHomeCount++;
            if (m_powerupMode.ValueRW.EnemyReturnHomeCount == 1)
            {
                ecb.AppendToBuffer(mainEntity, new SoundEventBufferElement()
                {
                    SoundType = AudioEvents.SoundType.EnemyReturnHome
                });
            }
        }
        m_enemyEatenBuffer.Clear();
    }

    public void CheckEnemiesReturnedHome(Entity mainEntity, EntityCommandBuffer ecb)
    {
        foreach (var item in m_enemyReturnedHomeBuffer)
        {
            m_powerupMode.ValueRW.EnemyReturnHomeCount--;
            if (m_powerupMode.ValueRW.EnemyReturnHomeCount == 0)
            {
                ecb.AppendToBuffer(mainEntity, new SoundStopEventBufferElement()
                {
                    SoundType = AudioEvents.SoundType.EnemyReturnHome
                });
            }
        }
        m_enemyReturnedHomeBuffer.Clear();
    }

    public void SetEnemyScaredBlinking(bool blinking) => m_powerupMode.ValueRW.EnemiesBlinking = blinking;
    public bool IsEnemyScaredBlinking => m_powerupMode.ValueRO.EnemiesBlinking;

}
