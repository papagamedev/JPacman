using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(MovableSystem))]
public partial class AudioSystem : SystemBase
{
    public Action<AudioEvents.SoundType> OnPlaySound;
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
        foreach (var music in mainAspect.MusicEventBuffer)
        {
            OnPlayMusic(music.MusicType);
        }
        foreach (var sound in mainAspect.SoundEventBuffer)
        {
            OnPlaySound(sound.SoundType);
        }
        mainAspect.MusicEventBuffer.Clear();
        mainAspect.SoundEventBuffer.Clear();
    }
}
