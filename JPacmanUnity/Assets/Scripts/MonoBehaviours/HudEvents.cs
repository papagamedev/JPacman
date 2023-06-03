using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HudEvents : MonoBehaviour
{
    public GameObject m_ingameRoot;
    public GameObject m_menuRoot;
    public GameObject m_pausedRoot;
    public TMP_Text m_scoreLabel;
    public TMP_Text m_livesLabel;
    public TMP_Text m_messageLabel;
    public GameObject m_scoreAnimationPrefab;
    public Image m_fade;
    public GameObject m_levelIconTemplate;
    public GameObject m_levelIconRoot;
    public GameConfig m_gameConfig;

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

    private class ScoreAnimState
    {
        const float kWaitTime = 2.0f;
        const float kAnimSpeed = 1000.0f;

        private RectTransform m_transform;
        private Vector3 m_startPos;
        private Vector3 m_targetPos;
        private float m_time;

        public int Score { get; private set; }

        public bool Ready => (m_targetPos - m_transform.localPosition).magnitude < kAnimSpeed * 0.05f;

        public ScoreAnimState(GameObject gameObject, int score, Vector3 worldPos, Vector3 targetPos)
        {
            Score = score;
            m_transform = gameObject.GetComponent<RectTransform>();
            m_startPos = WorldToUIPos(worldPos);
            m_targetPos = targetPos;
            m_time = 0.0f;
            m_transform.localPosition = m_startPos;
            var textComponent = gameObject.GetComponent<TMP_Text>();
            textComponent.text = score.ToString();
        }

        public void Update(float deltaTime)
        {
            m_time += deltaTime;
            if (m_time < kWaitTime)
            {
                return;
            }

            var currentPos = m_transform.localPosition;
            var dir = (m_targetPos - currentPos).normalized;
            currentPos += dir * deltaTime * kAnimSpeed;
            m_transform.localPosition = currentPos;
        }

        public void Kill()
        {
            GameObject.Destroy(m_transform.gameObject);
        }
    }

    private FadeState m_fadeAnimation;
    private List<ScoreAnimState> m_scoreAnimations;
    private int m_score;

    public enum LabelMessage
    {
        Round,
        Level,
        LevelFinal,
        LevelUltimate,
        Bonus,
        None,
        GameOver
    }

    public enum ShowUIType
    {
        Menu,
        Ingame,
        Paused,
        None
    }

    private void OnEnable()
    {
        OnShowUI(ShowUIType.None);

        var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
        hudSystem.OnSetLabelText += OnSetLabelText;
        hudSystem.OnSetLabelPos += OnSetLabelPos;
        hudSystem.OnSetLivesText += OnSetLivesText;
        hudSystem.OnSetLevelIcon += OnSetLevelIcon;
        hudSystem.OnSetScoreText += OnSetScoreText;
        hudSystem.OnKillAllScoreAnimations += OnKillAllScoreAnimations;
        hudSystem.OnFadeAnimation += OnFadeAnimation;
        hudSystem.OnShowUI += OnShowUI;

        Application.targetFrameRate = 60;

        m_scoreAnimations = new List<ScoreAnimState>();
    }

    private void Update()
    {
        UpdateFade();
        UpdateScoreAnimations();
    }

    private void OnDisable()
    {
        OnKillAllScoreAnimations();

        if (World.DefaultGameObjectInjectionWorld != null)
        {
            var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
            hudSystem.OnSetLabelText -= OnSetLabelText;
            hudSystem.OnSetLabelPos -= OnSetLabelPos;
            hudSystem.OnSetLivesText -= OnSetLivesText;
            hudSystem.OnSetLevelIcon -= OnSetLevelIcon;
            hudSystem.OnSetScoreText -= OnSetScoreText;
            hudSystem.OnFadeAnimation -= OnFadeAnimation;
            hudSystem.OnShowUI += OnShowUI;
        }
    }

    private void UpdateScoreText()
    {
        var animScoresTotal = m_scoreAnimations.Select(x => x.Score).Sum();
        m_scoreLabel.text = (m_score - animScoresTotal).ToString();
    }

    private void OnSetScoreText(bool hasAnimation, int score, int scoreDelta, float3 worldPos)
    {
        m_score = score;

        if (hasAnimation)
        {
            var gameObject = Instantiate(m_scoreAnimationPrefab, m_ingameRoot.transform);
            m_scoreAnimations.Add(new ScoreAnimState(gameObject, scoreDelta, worldPos, m_scoreLabel.rectTransform.localPosition));
        }
        else
        {
            UpdateScoreText();
        }
    }

    private void OnKillAllScoreAnimations()
    {
        foreach (var anim in m_scoreAnimations)
        {
            anim.Kill();
        }
        m_scoreAnimations.Clear();
    }

    private void UpdateScoreAnimations()
    {
        var scoreToAdd = 0;
        var animsToKill = new List<ScoreAnimState>();
        foreach (var anim in m_scoreAnimations)
        {
            anim.Update(Time.deltaTime);
            if (anim.Ready)
            {
                scoreToAdd += anim.Score;
                anim.Kill();
                animsToKill.Add(anim);
            }
        }
        foreach (var anim in animsToKill)
        {
            m_scoreAnimations.Remove(anim);
        }
        if (scoreToAdd > 0)
        {
            UpdateScoreText();
        }
    }

    private void OnSetLivesText(int value)
    {
        m_livesLabel.text = value.ToString();
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
            case LabelMessage.LevelFinal:
                m_messageLabel.text = "Nivel Final";
                return;
            case LabelMessage.LevelUltimate:
                m_messageLabel.text = "JA JA JA JA";
                return;
            case LabelMessage.GameOver:
                m_messageLabel.text = "Fin del Juego";
                break;
        }
    }

    private void OnSetLabelPos(float3 worldPos)
    {
        m_messageLabel.rectTransform.localPosition = WorldToUIPos(worldPos);
    }

    private void OnSetLevelIcon(int iconIdx)
    {
        var levelIconRootTransform = m_levelIconRoot.transform;
        for (int i = 0; i < levelIconRootTransform.childCount; i++)
        {
            var child = levelIconRootTransform.GetChild(i);
            Destroy(child.gameObject);
        }

        foreach (var config in m_gameConfig.RoundConfigs)
        {
            foreach (var level in config.LevelConfigs)
            {
                var child = Instantiate(m_levelIconTemplate, levelIconRootTransform);
                var image = child.GetComponent<Image>();
                image.sprite = level.FruitConfig.Sprite;
                child.SetActive(true);

                iconIdx--;
                if (iconIdx < 0)
                {
                    break;
                }
            }
            if (iconIdx < 0)
            {
                break;
            }
        }
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
        m_pausedRoot.SetActive(uiType == ShowUIType.Paused);
    }

    private static Vector3 WorldToUIPos(Vector3 worldPos)
    {
        return new Vector3(worldPos.x * 16, (worldPos.y - 1) * 16, 0);
    }
}
