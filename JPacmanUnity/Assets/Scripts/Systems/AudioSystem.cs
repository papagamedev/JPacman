using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial class AudioSystem : SystemBase
{
    public Action<AudioEvents.SoundType> OnPlaySound;
    public Action<AudioEvents.SoundType> OnStopSound;
    public Action<AudioEvents.MusicType> OnPlayMusic;

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
        if (OnPlaySound != null)
        {
            foreach (var sound in mainAspect.SoundEventBuffer)
            {
                OnPlaySound(sound.SoundType);
            }
        }
        if (OnStopSound != null)
        {
            foreach (var sound in mainAspect.SoundStopEventBuffer)
            {
                OnStopSound(sound.SoundType);
            }
        }
        mainAspect.MusicEventBuffer.Clear();
        mainAspect.SoundEventBuffer.Clear();
        mainAspect.SoundStopEventBuffer.Clear();
    }
}
