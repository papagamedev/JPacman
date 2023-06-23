using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;


public class PausedMenuEvents : MenuEvents
{
    public Button m_continueButton;
    public Button m_exitButton;
    public TMP_Text m_messageText;
    public float m_blinkDuration;

    private float m_blinkTime;

    void OnEnable()
    {
        m_continueButton.onClick.AddListener(OnContinue);
        m_exitButton.onClick.AddListener(OnExit);
    }

    private void Update()
    {
        UpdateMessageBlink();
    }

    private void UpdateMessageBlink()
    {
        m_blinkTime += Time.deltaTime;
        if (m_blinkTime > m_blinkDuration)
        {
            m_blinkTime -= m_blinkDuration;
            m_messageText.gameObject.SetActive(!m_messageText.gameObject.activeSelf);
        }
    }

    void OnDisable()
    {
        m_continueButton.onClick.RemoveListener(OnContinue);
        m_exitButton.onClick.RemoveListener(OnExit);
    }

    private void OnExit()
    {
        StartCoroutine(OnExitAsync());
    }

    private IEnumerator OnExitAsync()
    {
        yield return OnClickButtonFadeAsync(AudioEvents.SoundType.PlayerEatFruit);
        HudSystem.Instance?.OnPausedExit();
    }

    private void OnContinue()
    {
        HudSystem.Instance?.OnPausedContinue();
    }
}
