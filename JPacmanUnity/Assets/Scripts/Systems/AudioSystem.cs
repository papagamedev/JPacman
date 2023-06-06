using System;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial class AudioSystem : SystemBase
{
    public Action<AudioEvents.SoundType> OnPlaySound;
    public Action<AudioEvents.SoundType> OnStopSound;
    public Action<AudioEvents.MusicType> OnPlayMusic;
    public Action<bool> OnPauseAudio;
    public Action<bool, float> OnFadeMusic;

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<Main>();
    }

    protected override void OnUpdate()
    {
        var main = SystemAPI.GetSingletonEntity<Main>();
        var mainAspect = SystemAPI.GetAspect<MainAspect>(main);
        if (OnPlayMusic != null)
        {
            foreach (var music in mainAspect.MusicEventBuffer)
            {
                OnPlayMusic(music.MusicType);
            }
        }
        mainAspect.MusicEventBuffer.Clear();
        if (OnPlaySound != null)
        {
            foreach (var sound in mainAspect.SoundEventBuffer)
            {
                OnPlaySound(sound.SoundType);
            }
        }
        mainAspect.SoundEventBuffer.Clear();
        if (OnStopSound != null)
        {
            foreach (var sound in mainAspect.SoundStopEventBuffer)
            {
                OnStopSound(sound.SoundType);
            }
        }
        mainAspect.SoundStopEventBuffer.Clear();
        if (OnPauseAudio != null)
        {
            foreach (var sound in mainAspect.PauseAudioEventBuffer)
            {
                OnPauseAudio(sound.Paused);
            }
        }
        mainAspect.PauseAudioEventBuffer.Clear();
        if (OnFadeMusic!= null)
        {
            foreach (var fade in mainAspect.FadeMusicEventBuffer)
            {
                OnFadeMusic(fade.IsFadeIn, fade.Duration);
            }
        }
        mainAspect.FadeMusicEventBuffer.Clear();
    }
}
