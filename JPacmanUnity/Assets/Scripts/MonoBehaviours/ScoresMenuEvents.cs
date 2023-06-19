using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class ScoresMenuEvents : MenuEvents
{
    public Button m_backButton;
    public GameObject m_scoreEntry;

    void OnEnable()
    {
        m_backButton.onClick.AddListener(OnBack);
        m_hudEvents.OnFadeAnimation(true, 0.5f);
    }

    void OnDisable()
    {
        m_backButton.onClick.RemoveListener(OnBack);
    }

    public void OnBack()
    {
        StartCoroutine(OnBackAsync());
    }

    private IEnumerator OnBackAsync()
    {
        yield return OnClickButtonFadeAsync(AudioEvents.SoundType.PlayerEatDot);
        m_hudEvents.OnShowUI(UIEvents.ShowUIType.Menu);
    }
}
