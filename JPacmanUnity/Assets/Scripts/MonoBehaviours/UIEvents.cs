using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIEvents : MonoBehaviour
{
    public GameObject m_levelBackgroundRoot;
    public GameObject m_ingameRoot;
    public GameObject m_menuRoot;
    public GameObject m_scoresRoot;
    public GameObject m_pausedRoot;
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

    public enum ShowUIType
    {
        Menu,
        Scores,
        Ingame,
        Paused,
        None
    }

    private void OnEnable()
    {
        OnShowUI(ShowUIType.None);

        var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
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
        if (World.DefaultGameObjectInjectionWorld != null)
        {
            var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
            hudSystem.OnFadeAnimation -= OnFadeAnimation;
            hudSystem.OnShowUI += OnShowUI;
        }
    }

    public void OnFadeAnimation(bool isFadeIn, float duration)
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

    public void OnShowUI(ShowUIType uiType)
    {
        m_menuRoot.SetActive(uiType == ShowUIType.Menu);
        m_scoresRoot.SetActive(uiType == ShowUIType.Scores);
        m_ingameRoot.SetActive(uiType == ShowUIType.Ingame);
        m_pausedRoot.SetActive(uiType == ShowUIType.Paused);
        m_levelBackgroundRoot.SetActive(uiType == ShowUIType.Ingame || uiType == ShowUIType.Paused);
    }

    public static Vector3 WorldToUIPos(Vector3 worldPos)
    {
        return new Vector3(worldPos.x * 16, (worldPos.y - 1) * 16, 0);
    }
}
