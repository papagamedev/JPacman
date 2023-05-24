using System;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollectibleSystem))]
public partial class HudSystem : SystemBase
{
    public Action<HudEvents.LabelMessage, int> OnSetLabelText;
    public Action<float3> OnSetLabelPos; 
    public Action<int> OnSetLivesText;
    public Action<int> OnSetScoreText;
    public Action<int, float3> OnStartScoreAnimation;
    public Action OnKillAllScoreAnimations;
    public Action<bool, float> OnFadeAnimation;
    public Action<HudEvents.ShowUIType> OnShowUI;

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
                OnSetScoreText(element.Value);
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
                    case HudEvents.LabelMessage.Round:
                        value = gameAspect.LevelData.RoundNumber;
                        break;

                    case HudEvents.LabelMessage.Level:
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

        if (OnStartScoreAnimation != null)
        {
            foreach (var element in mainAspect.StartScoreAnimationBuffer)
            {
                OnStartScoreAnimation(element.Score, element.WorldPos);
            }
        }
        mainAspect.StartScoreAnimationBuffer.Clear();

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
}
