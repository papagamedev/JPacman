using System.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class MainMenuEvents : MenuEvents
{
    public Button m_playButton;
    public Button m_optionsButton;
    public Button m_scoresButton;
    public Button m_exitButton;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_playButton.onClick.AddListener(OnPlay);
        m_optionsButton.onClick.AddListener(OnOptions);
        m_scoresButton.onClick.AddListener(OnScores);
        m_exitButton.onClick.AddListener(OnExit);
        m_uiEvents.OnFadeAnimation(true, 0.5f);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        m_playButton.onClick.RemoveListener(OnPlay);
        m_optionsButton.onClick.RemoveListener(OnOptions);
        m_scoresButton.onClick.RemoveListener(OnScores);
        m_exitButton.onClick.RemoveListener(OnExit);
    }

    public void OnExit()
    {
        StartCoroutine(OnExitAsync());
    }

    private IEnumerator OnExitAsync()
    {
        yield return OnClickButtonFadeAsync(AudioEvents.SoundType.PlayerEatFruit);
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            EditorApplication.ExitPlaymode();
            yield break;
        }
#endif
        Application.Quit();
    }

    public void OnPlay()
    {
        StartCoroutine(OnPlayAsync());
    }

    private IEnumerator OnPlayAsync()
    {
        yield return OnClickButtonFadeAsync(AudioEvents.SoundType.PlayerEatFruit);
        UISystem.Instance?.OnMenuPlay(GameSettings.StartRound, GameSettings.MovingCollectibles);
    }

    private void OnOptions()
    {
        SwitchToMenuUI(UIEvents.ShowUIType.Options);
    }

    private void OnScores()
    {
        SwitchToMenuUI(UIEvents.ShowUIType.Scores);
    }

}
