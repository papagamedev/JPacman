using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PausedMenuEvents : MenuEvents
{
    public Button m_continueButton;
    public Button m_exitButton;
    public TMP_Text m_messageText;
    public float m_blinkDuration;

    private float m_blinkTime;
    private InputAction m_pauseAction;
    private bool m_continueRequested;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_continueRequested = false;
        m_continueButton.onClick.AddListener(OnContinue);
        m_exitButton.onClick.AddListener(OnExit);

        // the same button that paused the game (Start / Esc / P) also resumes it
        m_pauseAction = InputSystem.actions.FindAction("Player/Pause", throwIfNotFound: true);
        m_pauseAction.performed += OnPausePerformed;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        OnContinue();
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

    protected override void OnDisable()
    {
        base.OnDisable();

        m_continueButton.onClick.RemoveListener(OnContinue);
        m_exitButton.onClick.RemoveListener(OnExit);
        m_pauseAction.performed -= OnPausePerformed;
    }

    private void OnExit()
    {
        StartCoroutine(OnExitAsync());
    }

    private IEnumerator OnExitAsync()
    {
        yield return OnClickButtonFadeAsync(AudioEvents.SoundType.PlayerEatFruit);
        UISystem.Instance?.OnPausedExit();
    }

    private void OnContinue()
    {
        // Esc is bound to both Player/Pause and UI/Cancel (which clicks Continue); resume only once
        if (m_continueRequested)
        {
            return;
        }
        m_continueRequested = true;
        UISystem.Instance?.OnPausedContinue();
    }
}
