using System.Collections;
using UnityEngine;


public class MenuEvents : MonoBehaviour
{
    public AudioEvents m_audioEvents;
    public UIEvents m_uiEvents;
    public float m_fadeTime;

    protected IEnumerator OnClickButtonFadeAsync(AudioEvents.SoundType clickSound)
    {
        m_audioEvents.OnPlaySound(clickSound);
        m_audioEvents.OnFadeMusic(false, m_fadeTime);
        m_uiEvents.OnFadeAnimation(false, 0.5f);
        yield return new WaitForSeconds(m_fadeTime);
    }

    protected void SwitchToMenuUI(UIEvents.ShowUIType uiType)
    {
        m_audioEvents.OnPlaySound(AudioEvents.SoundType.PlayerEatDot);
        m_uiEvents.OnShowUI(uiType);
        UISystem.Instance?.OnSwitchToMenuUI(uiType);
    }
}
