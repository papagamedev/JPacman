using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class HudEvents : MonoBehaviour
{
    public GameObject m_ingameRoot;
    public GameObject m_menuRoot;
    public TMP_Text m_scoreLabel;
    public TMP_Text m_livesLabel;
    public TMP_Text m_messageLabel;
    public Image m_fade;

    private class FadeState
    {
        public bool IsFadeIn;
        public float FadeDuration;
        public float FadeStartTime;
        public bool IsFinished;

        public void Update(Image fade)
        {
            var currentTime = Time.time - FadeStartTime;
            var currentOpacity = currentTime / FadeDuration;
            if (currentOpacity > 1.0f)
            {
                currentOpacity = 1.0f;
                IsFinished = true;
            }
            if (IsFadeIn)
            {
                currentOpacity = 1.0f - currentOpacity;
            }
            fade.color = new Color(1, 1, 1, currentOpacity);
        }


    };

    private FadeState m_fadeAnimation;

    public enum LabelMessage
    {
        Round,
        Level,
        Bonus,
        None,
        GameOver
    }

    public enum ShowUIType
    {
        Menu,
        Ingame,
        None
    }

    private void OnEnable()
    {
        OnShowUI(ShowUIType.None);

        var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
        hudSystem.OnSetLabelText += OnSetLabelText;
        hudSystem.OnSetLabelPos += OnSetLabelPos;
        hudSystem.OnSetLivesText += OnSetLivesText;
        hudSystem.OnSetScoreText += OnSetScoreText;
        hudSystem.OnStartScoreAnimation += OnStartScoreAnimation;
        hudSystem.OnFadeAnimation += OnFadeAnimation;
        hudSystem.OnShowUI += OnShowUI;

        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        UpdateFade();
    }

    private void OnDisable()
    {
        KillAllScoreAnimations();

        if (World.DefaultGameObjectInjectionWorld != null)
        {
            var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
            hudSystem.OnSetLabelText -= OnSetLabelText;
            hudSystem.OnSetLabelPos -= OnSetLabelPos;
            hudSystem.OnSetLivesText -= OnSetLivesText;
            hudSystem.OnSetScoreText -= OnSetScoreText;
            hudSystem.OnStartScoreAnimation -= OnStartScoreAnimation;
            hudSystem.OnFadeAnimation -= OnFadeAnimation;
            hudSystem.OnShowUI += OnShowUI;
        }
    }

    private void OnStartScoreAnimation(int arg1, float3 arg2)
    {
        
    }

    private void KillAllScoreAnimations()
    {

    }

    private void OnSetScoreText(int obj)
    {
        m_scoreLabel.text = obj.ToString();
    }

    private void OnSetLivesText(int obj)
    {
        m_livesLabel.text = obj.ToString();
    }

    private void OnSetLabelText(LabelMessage message, int value)
    {
        switch (message)
        {
            case LabelMessage.None:
                m_messageLabel.text = "";
                return;
            case LabelMessage.Round:
                m_messageLabel.text = "Ronda " + value;
                return;
            case LabelMessage.Level:
                m_messageLabel.text = "Nivel " + value;
                return;
            case LabelMessage.Bonus:
                m_messageLabel.text = "Nivel X";
                return;
            case LabelMessage.GameOver:
                m_messageLabel.text = "Fin del Juego";
                break;
        }
    }

    private void OnSetLabelPos(float3 worldPos)
    {
        m_messageLabel.rectTransform.localPosition = new Vector2(worldPos.x * 16, (worldPos.y - 1) * 16);
    }

    private void OnFadeAnimation(bool isFadeIn, float duration)
    {
        m_fadeAnimation = new FadeState()
        {
            IsFadeIn = isFadeIn,
            FadeDuration = duration,
            FadeStartTime = Time.time
        };
    }

    private void UpdateFade()
    {
        if (m_fadeAnimation != null)
        {
            m_fadeAnimation.Update(m_fade);
            if (m_fadeAnimation.IsFinished)
            {
                m_fadeAnimation = null;
            }
        }

    }

    private void OnShowUI(ShowUIType uiType)
    {
        m_menuRoot.SetActive(uiType == ShowUIType.Menu);
        m_ingameRoot.SetActive(uiType == ShowUIType.Ingame);
    }
}
