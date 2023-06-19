using System.Collections;
using UnityEngine;
using TMPro;


public class ScoreEntryUI : MonoBehaviour
{
    public TMP_Text m_nameText;
    public TMP_Text m_scoreText;

    public void Set(ScoreData data)
    {
        m_nameText.text = data.Name;
        m_scoreText.text = data.Score.ToString();
    }
}
