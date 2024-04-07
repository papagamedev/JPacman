using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine.UI;


public class GameOverEvents : MenuEvents
{
    public Button m_postScoreButton;
    public Button m_exitButton;
    public TMP_Text m_scoreText;
    public TMP_InputField m_inputField;

    private string m_message;
    private int m_score;
    private string m_mapId;
    private int m_round;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_postScoreButton.onClick.AddListener(OnPostScore);
        m_exitButton.onClick.AddListener(OnExit);
        m_inputField.onValueChanged.AddListener(OnMessageChanged);

        UISystem.Instance?.GetGameOverParams(out m_score, out m_mapId, out m_round);
        m_scoreText.text = "Puntaje: " + ScoreEntryUI.GetFormattedScore(m_score); 
        m_inputField.text = "";
        UpdateButtons();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        m_postScoreButton.onClick.RemoveListener(OnPostScore);
        m_exitButton.onClick.RemoveListener(OnExit);
        m_inputField.onValueChanged.RemoveListener(OnMessageChanged);
    }

    private void OnExit()
    {
        StartCoroutine(OnExitAsync());
    }

    private IEnumerator OnExitAsync()
    {
        yield return OnClickButtonFadeAsync(AudioEvents.SoundType.PlayerEatFruit);
        UISystem.Instance?.OnGameOverExit();
    }

    private bool IsValid()
    {
        if (m_score <= 0)
        {
            return false;
        }
        if (string.IsNullOrEmpty(m_message))
        {
            return false;
        }
        return true;
    }

    private void OnPostScore()
    {
        if (!IsValid())
        {
            return;
        }

        UpdateButtons(false);
        StartCoroutine(PostScoreAsync(m_mapId, m_round, m_message, m_score));
    }

    private IEnumerator PostScoreAsync(string mapId, int round, string message, int score)
    {
        var task = BackendClient.Instance.AddScore(m_mapId, m_round, m_message, m_score);
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.IsFaulted)
        {
            UpdateButtons();
            yield break;
        }
        UISystem.Instance?.OnGameOverPostScore();
    }

    private void OnMessageChanged(string message)
    {
        m_message = message.Trim();
        UpdateButtons();
    }

    private void UpdateButtons(bool enabled = true)
    {
        m_postScoreButton.interactable = enabled && IsValid();
        m_exitButton.interactable = enabled;
    }
}
