using Unity.Entities;
using Unity.Mathematics;

public struct Main : IComponentData
{
    public Entity DotPrefab;
    public Entity PlayerPrefab;
    public Entity EnemyPrefab;
    public Entity WallPrefab;
    public Entity FruitPrefab;
    public Entity PowerupPrefab;
    public BlobAssetReference<LevelsConfigData> LevelsConfigBlob;
    public BlobAssetReference<MapsConfigData> MapsConfigBlob;
    public BlobAssetReference<IntroConfigData> IntroConfigBlob;
    public uint RandomSeed;
    public int LivesCount;
}

public struct IntroPhaseTag : IComponentData { }

public struct MenuPhaseTag : IComponentData { }

public struct SoundEventBufferElement : IBufferElementData
{
    public AudioEvents.SoundType SoundType;
}

public struct SoundStopEventBufferElement : IBufferElementData
{
    public AudioEvents.SoundType SoundType;
}

public struct MusicEventBufferElement : IBufferElementData
{
    public AudioEvents.MusicType MusicType;
}

public struct PauseAudioEventBufferElement : IBufferElementData
{
    public bool Paused;
}

public struct SetLivesTextBufferElement : IBufferElementData
{
    public int Value;
}

public struct SetScoreTextBufferElement : IBufferElementData
{
    public bool HasAnimation;
    public int Score;
    public int DeltaScore;
    public float3 WorldPos;
}

public struct SetLabelTextBufferElement : IBufferElementData
{
    public HudEvents.LabelMessage Value;
}

public struct SetLabelPosBufferElement : IBufferElementData
{
    public float3 Value;
}

public struct SetLevelIconBufferElement : IBufferElementData
{
    public int IconIdx;
}

public struct KillAllScoreAnimationBufferElement : IBufferElementData
{
    public byte Dummy;
}

public struct FadeAnimationBufferElement : IBufferElementData
{
    public bool IsFadeIn;
    public float Duration;
}

public struct ShowUIBufferElement : IBufferElementData
{
    public HudEvents.ShowUIType UI;
}


public readonly partial struct MainAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<Main> m_main;
    public readonly DynamicBuffer<SoundEventBufferElement> SoundEventBuffer;
    public readonly DynamicBuffer<SoundStopEventBufferElement> SoundStopEventBuffer;
    public readonly DynamicBuffer<MusicEventBufferElement> MusicEventBuffer;
    public readonly DynamicBuffer<PauseAudioEventBufferElement> PauseAudioEventBuffer;
    public readonly DynamicBuffer<SetLivesTextBufferElement> SetLivesTextBuffer;
    public readonly DynamicBuffer<SetScoreTextBufferElement> SetScoreTextBuffer;
    public readonly DynamicBuffer<SetLabelTextBufferElement> SetLabelTextBuffer;
    public readonly DynamicBuffer<SetLabelPosBufferElement> SetLabelPosBuffer;
    public readonly DynamicBuffer<SetLevelIconBufferElement> SetLevelIconBuffer;
    public readonly DynamicBuffer<KillAllScoreAnimationBufferElement> KillAllScoreAnimationsBuffer;
    public readonly DynamicBuffer<FadeAnimationBufferElement> FadeAnimationBuffer;
    public readonly DynamicBuffer<ShowUIBufferElement> ShowUIBuffer;
}
