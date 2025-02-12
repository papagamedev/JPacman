using System;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial class UISystem : SystemBase
{
    public Action<IngameEvents.LabelMessage, int> OnSetLabelText;
    public Action<float3> OnSetLabelPos;
    public Action<int> OnSetLivesText;
    public Action<int> OnSetLevelIcon;
    public Action<bool, int, int, float3> OnSetScoreText;
    public Action OnKillAllScoreAnimations;
    public Action<bool, float> OnFadeAnimation;
    public Action<UIEvents.ShowUIType> OnShowUI;

    public static UISystem Instance => World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<UISystem>();

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
            mainAspect.SetLivesTextBuffer.Clear();
        }

        if (OnSetScoreText != null)
        {
            foreach (var element in mainAspect.SetScoreTextBuffer)
            {
                OnSetScoreText(element.HasAnimation, element.Score, element.DeltaScore, element.WorldPos);
            }
            mainAspect.SetScoreTextBuffer.Clear();
        }

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
            mainAspect.SetLabelTextBuffer.Clear();
        }

        if (OnSetLabelPos != null)
        {
            foreach (var element in mainAspect.SetLabelPosBuffer)
            {
                OnSetLabelPos(element.Value);
            }
            mainAspect.SetLabelPosBuffer.Clear();
        }

        if (OnSetLevelIcon != null)
        {
            foreach (var element in mainAspect.SetLevelIconBuffer)
            {
                OnSetLevelIcon(element.IconIdx);
            }
            mainAspect.SetLevelIconBuffer.Clear();
        }

        if (OnKillAllScoreAnimations != null)
        {
            foreach (var element in mainAspect.KillAllScoreAnimationsBuffer)
            {
                OnKillAllScoreAnimations();
            }
            mainAspect.KillAllScoreAnimationsBuffer.Clear();
        }

        if (OnFadeAnimation != null)
        {
            foreach (var element in mainAspect.FadeAnimationBuffer)
            {
                OnFadeAnimation(element.IsFadeIn, element.Duration);
            }
            mainAspect.FadeAnimationBuffer.Clear();
        }

        if (OnShowUI != null)
        {
            foreach (var element in mainAspect.ShowUIBuffer)
            {
                OnShowUI(element.UI);
            }
            mainAspect.ShowUIBuffer.Clear();
        }
    }


    public void OnIntroSkip()
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var intro = SystemAPI.GetComponentRW<IntroPhase>(mainEntity);
        intro.ValueRW.Skip = true;
    }

    public void OnMenuPlay(int levelIndex)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var lives = mainComponent.ValueRO.LivesCount;
        ecb.RemoveComponent<MenuPhase>(mainEntity);
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

    private void SwitchToMainMenu<PreviousPhaseType>(UIEvents.ShowUIType menuType)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        ecb.RemoveComponent<PreviousPhaseType>(mainEntity);
        ecb.AddComponent(mainEntity, new LevelClearPhase()
        {
            MenuUIType = menuType
        });
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }

    public void OnPausedExit()
    {
        SwitchToMainMenu<LevelPlayingPhaseTag>(UIEvents.ShowUIType.Menu);
    }

    public void OnGameOverExit()
    {
        SwitchToMainMenu<LevelGameOverPhaseTag>(UIEvents.ShowUIType.Menu);
    }

    public void OnGameOverPostScore()
    {
        SwitchToMainMenu<LevelGameOverPhaseTag>(UIEvents.ShowUIType.Scores);
    }

    public void OnSwitchToMenuUI(UIEvents.ShowUIType uiType)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var mainComponent = SystemAPI.GetComponentRW<Main>(mainEntity);
        ref var menuData = ref mainComponent.ValueRO.MenuConfigBlob.Value;
        MenuSystem.SetDotShape(uiType, ref menuData, mainEntity, ecb);
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }

    public void GetGameOverParams(out int score, out string mapId, out int round)
    {
        var mainEntity = SystemAPI.GetSingletonEntity<Main>();
        var mainComponent = SystemAPI.GetComponentRO<Main>(mainEntity);
        var gameAspect = SystemAPI.GetAspect<GameAspect>(mainEntity);
        var mapIdx = gameAspect.LevelData.MapId;
        mapId = mainComponent.ValueRO.MapsConfigBlob.Value.MapsData[mapIdx].Id.ToString();
        round = gameAspect.LevelData.RoundNumber;
        score = gameAspect.Score;
    }
}
