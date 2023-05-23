using System;
using Unity.Entities;
using UnityEngine;

public class AudioEvents : MonoBehaviour
{
    public enum SoundType
    {
        PlayerEatDot,
        PlayerEatPowerup,
        PlayerEatFruit,
        PlayerEatEnemy,
        EnemyScared,
        EnemyReturnHome,
    }

    public enum MusicType
    {
        Intro,
        Menu,
        LevelStart,
        Level,
        LevelEnd,
        LevelBonus,
        Dead
    }

    [Serializable]
    public struct AudioConfig
    {
        public AudioClip m_audioClip;
        public bool m_loop;
    }

    private AudioSource[] m_soundSource;
    private AudioSource m_musicSource;

    public AudioConfig SoundPlayerEatDot;
    public AudioConfig SoundPlayerEatPowerup;
    public AudioConfig SoundPlayerEatFruit;
    public AudioConfig SoundPlayerEatEnemy;
    public AudioConfig SoundEnemyScared;
    public AudioConfig SoundEnemyReturnHome;
    public AudioConfig MusicDead;
    public AudioConfig MusicIntro;
    public AudioConfig MusicMenu;
    public AudioConfig MusicLevelStart;
    public AudioConfig MusicLevel;
    public AudioConfig MusicLevelEnd;
    public AudioConfig MusicLevelBonus;

    private void OnEnable()
    {
        var audioSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<AudioSystem>();
        audioSystem.OnPlaySound += OnPlaySound;
        audioSystem.OnStopSound += OnStopSound;
        audioSystem.OnPlayMusic += OnPlayMusic;

        var soundSourcesCount = Enum.GetValues(typeof(SoundType)).Length;
        m_soundSource = new AudioSource[soundSourcesCount];
        for (int i = 0; i < soundSourcesCount; i++)
        {
            m_soundSource[i] = gameObject.AddComponent<AudioSource>();
        }
        m_musicSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnDisable()
    {
        if (World.DefaultGameObjectInjectionWorld != null)
        {
            var audioSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<AudioSystem>();
            audioSystem.OnPlaySound -= OnPlaySound;
            audioSystem.OnPlayMusic -= OnPlayMusic;
        }
    }

    private void OnPlaySound(SoundType sound)
    {
        var source = m_soundSource[(int)sound];
        if (source.isPlaying)
        {
            return;
        }
        AudioConfig config;
        switch (sound)
        {
            case SoundType.PlayerEatDot:
                config = SoundPlayerEatDot;
                break;

            case SoundType.PlayerEatEnemy:
                config = SoundPlayerEatEnemy;
                break;

            case SoundType.PlayerEatPowerup:
                config = SoundPlayerEatPowerup;
                break;

            case SoundType.PlayerEatFruit:
                config = SoundPlayerEatFruit;
                break;

            case SoundType.EnemyScared:
                config = SoundEnemyScared;
                break;

            case SoundType.EnemyReturnHome:
                config = SoundEnemyReturnHome;
                break;

            default:
                return;
        }
        PlaySource(source, config);
    }

    private void PlaySource(AudioSource source, AudioConfig config)
    {
        source.loop = config.m_loop;
        source.clip = config.m_audioClip;
        source.Play();
    }

    private void OnStopSound(SoundType sound)
    {
        var source = m_soundSource[(int)sound];
        if (source == null || !source.isPlaying)
        {
            return;
        }
        source.Stop();
    }

    private void OnPlayMusic(MusicType music)
    {
        m_musicSource.Stop();
        AudioConfig config;
        switch (music)
        {
            case MusicType.LevelStart:
                config = MusicLevelStart;
                break;
            case MusicType.Level:
                config = MusicLevel;
                break;
            case MusicType.LevelEnd:
                config = MusicLevelEnd;
                break;
            case MusicType.Dead:
                config = MusicDead;
                break;
            case MusicType.Intro:
                config = MusicIntro;
                break;
            case MusicType.Menu:
                config = MusicMenu;
                break;
            case MusicType.LevelBonus:
                config = MusicLevelBonus;
                break;
            default:
                return;
        }
        PlaySource(m_musicSource, config);
    }
}
