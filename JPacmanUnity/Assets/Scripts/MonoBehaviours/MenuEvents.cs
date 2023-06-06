using System.Collections;
using UnityEngine;


public class MenuEvents : MonoBehaviour
{
    public AudioEvents m_audioEvents;
    public HudEvents m_hudEvents;
    public float m_fadeTime;

    protected IEnumerator OnClickButtonFadeAsync(AudioEvents.SoundType clickSound)
    {
        m_audioEvents.OnPlaySound(clickSound);
        m_audioEvents.OnFadeMusic(false, m_fadeTime);
        m_hudEvents.OnFadeAnimation(false, 0.5f);
        yield return new WaitForSeconds(m_fadeTime);
    }

}
