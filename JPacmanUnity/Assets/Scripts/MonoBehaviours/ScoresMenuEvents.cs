using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ScoresMenuEvents : MenuEvents
{
    public Button m_backButton;
    public GameObject m_scoreEntry;
    public GameObject m_scoreListRoot;
    public Button m_mapButton;
    public TMP_Text m_mapButtonText;
    public Button m_roundButton;
    public TMP_Text m_roundButtonText;
    public TMP_Text m_messageText;
    public GameConfig m_gameConfig;

    private int m_mapIdx = 0;
    private int m_roundIdx = 0;
    private Coroutine m_getScoresRoutine;

    void OnEnable()
    {
        m_backButton.onClick.AddListener(OnBack);
        m_roundButton.onClick.AddListener(OnRound);
        m_mapButton.onClick.AddListener(OnMap);

        StartGetScores();

        m_uiEvents.OnFadeAnimation(true, 0.5f);
    }

    private void StartGetScores()
    {
        DeleteAllScores();
        if (m_getScoresRoutine != null)
        {
            StopCoroutine(m_getScoresRoutine);
            m_getScoresRoutine = null;
        }

        m_getScoresRoutine = StartCoroutine(GetUpdatedScoresAsync());
    }

    private IEnumerator GetUpdatedScoresAsync()
    {
        var mapId = m_gameConfig.MapConfigs[m_mapIdx].name;
        var roundId = m_gameConfig.RoundConfigs[m_roundIdx].RoundNumber;
        m_mapButtonText.text = m_gameConfig.GetMapDisplayName(mapId);
        m_roundButtonText.text = m_gameConfig.GetRoundDisplayName(roundId);
        m_messageText.text = "Obteniendo puntajes...";
        var task = BackendClient.Instance.GetScores(mapId, roundId);
        while (!task.IsCompleted)
        {
            yield return null;
        }

        var result = task.Result;
        if (result == null)
        {
            m_messageText.text = "Error al obtener puntajes!";
            yield break;
        }
        m_messageText.text = "";

        foreach (var entry in result)
        {
            AddScore(entry);
        }

        m_getScoresRoutine = null;
    }

    private void DeleteAllScores()
    {
        var scoresListTransform = m_scoreListRoot.transform;
        var scoresCount = scoresListTransform.childCount;
        if (scoresCount == 0)
        {
            return;
        }
        while (scoresCount > 0)
        {
            scoresCount--;
            Destroy(scoresListTransform.GetChild(scoresCount).gameObject);
        }
    }

    private void AddScore(ScoreData data)
    {
        var obj = Instantiate(m_scoreEntry, m_scoreListRoot.transform);
        var entry = obj.GetComponent<ScoreEntryUI>();
        entry.Set(data);
        obj.SetActive(true);
    }

    void OnDisable()
    {
        m_backButton.onClick.RemoveListener(OnBack);
        m_roundButton.onClick.RemoveListener(OnRound);
        m_mapButton.onClick.RemoveListener(OnMap);
    }

    public void OnBack()
    {
        StartCoroutine(OnBackAsync());
    }

    private IEnumerator OnBackAsync()
    {
        yield return OnClickButtonFadeAsync(AudioEvents.SoundType.PlayerEatDot);
        m_uiEvents.OnShowUI(UIEvents.ShowUIType.Menu);
    }

    public void OnRound()
    {
        m_roundIdx = (m_roundIdx + 1) % m_gameConfig.RoundConfigs.Length;
        StartGetScores();
    }

    public void OnMap()
    {
        m_mapIdx = (m_mapIdx + 1) % m_gameConfig.MapConfigs.Length;
        StartGetScores();
    }
}
