using TMPro;
using UnityEngine;
using UnityEngine.UI;


// "Informacion" screen from the original game: version and credits.
public class InfoMenuEvents : MenuEvents
{
    public Button m_backButton;
    public TMP_Text m_versionText;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_backButton.onClick.AddListener(OnBack);
        m_versionText.text = "v " + Application.version;
        m_uiEvents.OnFadeAnimation(true, 0.5f);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        m_backButton.onClick.RemoveListener(OnBack);
    }

    private void OnBack()
    {
        SwitchToMenuUI(UIEvents.ShowUIType.Options);
    }
}
