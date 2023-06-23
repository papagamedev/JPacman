using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class IngameEvents : MonoBehaviour
{
    public TMP_Text m_scoreLabel;
    public TMP_Text m_livesLabel;
    public TMP_Text m_messageLabel;
    public GameObject m_scoreAnimationPrefab;
    public GameObject m_levelIconTemplate;
    public GameObject m_levelIconRoot;
    public GameConfig m_gameConfig;

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
            m_startPos = UIEvents.WorldToUIPos(worldPos);
            m_targetPos = targetPos;
            m_time = 0.0f;
            m_transform.localPosition = m_startPos;
            var textComponent = gameObject.GetComponent<TMP_Text>();
            textComponent.text = ScoreEntryUI.GetFormattedScore(score);
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

    private List<ScoreAnimState> m_scoreAnimations;
    private int m_score;

    public enum LabelMessage
    {
        Round,
        Level,
        LevelFinal,
        LevelUltimate,
        Bonus,
        None
    }

    private void OnEnable()
    {
        var hudSystem = UISystem.Instance;
        hudSystem.OnSetLabelText += OnSetLabelText;
        hudSystem.OnSetLabelPos += OnSetLabelPos;
        hudSystem.OnSetLivesText += OnSetLivesText;
        hudSystem.OnSetLevelIcon += OnSetLevelIcon;
        hudSystem.OnSetScoreText += OnSetScoreText;
        hudSystem.OnKillAllScoreAnimations += OnKillAllScoreAnimations;
        m_scoreAnimations = new List<ScoreAnimState>();
    }

    private void Update()
    {
        UpdateScoreAnimations();
    }

    private void OnDisable()
    {
        OnKillAllScoreAnimations();

        var hudSystem = UISystem.Instance;
        if (hudSystem != null)
        {
            hudSystem.OnSetLabelText -= OnSetLabelText;
            hudSystem.OnSetLabelPos -= OnSetLabelPos;
            hudSystem.OnSetLivesText -= OnSetLivesText;
            hudSystem.OnSetLevelIcon -= OnSetLevelIcon;
            hudSystem.OnSetScoreText -= OnSetScoreText;
        }
    }

    private void UpdateScoreText()
    {
        var animScoresTotal = m_scoreAnimations.Select(x => x.Score).Sum();
        m_scoreLabel.text = ScoreEntryUI.GetFormattedScore(m_score - animScoresTotal);
    }

    private void OnSetScoreText(bool hasAnimation, int score, int scoreDelta, float3 worldPos)
    {
        m_score = score;

        if (hasAnimation)
        {
            var gameObject = Instantiate(m_scoreAnimationPrefab, transform);
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
                m_messageLabel.text = "Ronda " + m_gameConfig.GetRoundDisplayName(value);
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
        }
    }

    private void OnSetLabelPos(float3 worldPos)
    {
        m_messageLabel.rectTransform.localPosition = UIEvents.WorldToUIPos(worldPos);
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
}
