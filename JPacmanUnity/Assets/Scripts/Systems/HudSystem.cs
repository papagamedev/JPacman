using System;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial class HudSystem : SystemBase
{
    public Action<IngameEvents.LabelMessage, int> OnSetLabelText;
    public Action<float3> OnSetLabelPos; 
    public Action<int> OnSetLivesText;
    public Action<int> OnSetLevelIcon;
    public Action<bool, int, int, float3> OnSetScoreText;
    public Action OnKillAllScoreAnimations;
    public Action<bool, float> OnFadeAnimation;
    public Action<UIEvents.ShowUIType> OnShowUI;

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<Main>();
    }

    protected override void OnUpdate()
    {
        var main = SystemAPI.GetSingletonEntity<Main>();
        var mainAspect = SystemAPI.GetAspect<MainAspect>(main);

        if (OnSetLivesText != null)
        {
            foreach (var element in mainAspect.SetLivesTextBuffer)
            {
                OnSetLivesText(element.Value);
            }
        }
        mainAspect.SetLivesTextBuffer.Clear();

        if (OnSetScoreText != null)
        {
            foreach (var element in mainAspect.SetScoreTextBuffer)
            {
                OnSetScoreText(element.HasAnimation, element.Score, element.DeltaScore, element.WorldPos);
            }
        }
        mainAspect.SetScoreTextBuffer.Clear();

        if (OnSetLabelText != null)
        {
            foreach (var element in mainAspect.SetLabelTextBuffer)
            {
                var gameAspect = SystemAPI.GetAspect<GameAspect>(main);
                int value = 0;
                switch (element.Value)
                {
                    case IngameEvents.LabelMessage.Round:
                        value = gameAspect.LevelData.RoundNumber;
                        break;

                    case IngameEvents.LabelMessage.Level:
                        value = gameAspect.LevelData.LevelNumber;
                        break;
                }
                OnSetLabelText(element.Value, value);
            }
        }
        mainAspect.SetLabelTextBuffer.Clear();

        if (OnSetLabelPos != null)
        {
            foreach (var element in mainAspect.SetLabelPosBuffer)
            {
                OnSetLabelPos(element.Value);
            }
        }
        mainAspect.SetLabelPosBuffer.Clear();

        if (OnSetLevelIcon != null)
        {
            foreach (var element in mainAspect.SetLevelIconBuffer)
            {
                OnSetLevelIcon(element.IconIdx);
            }
        }
        mainAspect.SetLevelIconBuffer.Clear();

        if (OnKillAllScoreAnimations != null)
        {
            foreach (var element in mainAspect.KillAllScoreAnimationsBuffer)
            {
                OnKillAllScoreAnimations();
            }
        }
        mainAspect.KillAllScoreAnimationsBuffer.Clear();

        if (OnFadeAnimation != null)
        {
            foreach (var element in mainAspect.FadeAnimationBuffer)
            {
                OnFadeAnimation(element.IsFadeIn, element.Duration);
            }
        }
        mainAspect.FadeAnimationBuffer.Clear();

        if (OnShowUI != null)
        {
            foreach (var element in mainAspect.ShowUIBuffer)
            {
                OnShowUI(element.UI);
            }
        }
        mainAspect.ShowUIBuffer.Clear();
    }

    public void OnMenuPlay(int levelIndex)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var lives = mainComponent.ValueRO.LivesCount;
        ecb.RemoveComponent<MenuPhaseTag>(mainEntity);
        ecb.AddComponent(mainEntity, new Game()
        {
            Lives = lives,
            Score = 0,
            LevelId = levelIndex
        });
        ecb.Playback(EntityManager);
        ecb.Dispose();

    }

    public void OnPausedContinue()
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        gameAspect.SetPaused(false, mainEntity, ecb);
        ecb.Playback(EntityManager);
        ecb.Dispose();

    }

    public void OnPausedExit()
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        ecb.RemoveComponent<LevelPlayingPhaseTag>(mainEntity);
        ecb.AddComponent(mainEntity, new LevelClearPhaseTag());
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
