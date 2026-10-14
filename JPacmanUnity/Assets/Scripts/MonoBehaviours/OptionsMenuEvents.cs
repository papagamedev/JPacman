using TMPro;
using UnityEngine.UI;


// Options from the original 1999 game: "Op. de Juego" (Ronda, Mov. Puntos) and "Op. de Audio" (Musica, Efectos),
// plus the "Informacion" screen, flattened into a single screen.
public class OptionsMenuEvents : MenuEvents
{
    public Button m_roundButton;
    public TMP_Text m_roundButtonText;
    public Button m_movingCollectiblesButton;
    public TMP_Text m_movingCollectiblesButtonText;
    public Button m_musicButton;
    public TMP_Text m_musicButtonText;
    public Button m_soundButton;
    public TMP_Text m_soundButtonText;
    public Button m_infoButton;
    public Button m_backButton;
    public GameConfig m_gameConfig;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_roundButton.onClick.AddListener(OnRound);
        m_movingCollectiblesButton.onClick.AddListener(OnMovingCollectibles);
        m_musicButton.onClick.AddListener(OnMusic);
        m_soundButton.onClick.AddListener(OnSound);
        m_infoButton.onClick.AddListener(OnInfo);
        m_backButton.onClick.AddListener(OnBack);

        UpdateTexts();
        m_uiEvents.OnFadeAnimation(true, 0.5f);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        m_roundButton.onClick.RemoveListener(OnRound);
        m_movingCollectiblesButton.onClick.RemoveListener(OnMovingCollectibles);
        m_musicButton.onClick.RemoveListener(OnMusic);
        m_soundButton.onClick.RemoveListener(OnSound);
        m_infoButton.onClick.RemoveListener(OnInfo);
        m_backButton.onClick.RemoveListener(OnBack);
    }

    private void UpdateTexts()
    {
        var roundName = m_gameConfig.RoundConfigs[GameSettings.StartRound % m_gameConfig.RoundConfigs.Length].DisplayName.Trim(' ', '-');
        m_roundButtonText.text = "Ronda: " + roundName;
        m_movingCollectiblesButtonText.text = "Mov. Puntos: " + YesNo(GameSettings.MovingCollectibles);
        m_musicButtonText.text = "Música: " + YesNo(GameSettings.MusicOn);
        m_soundButtonText.text = "Efectos: " + YesNo(GameSettings.SoundOn);
    }

    private static string YesNo(bool value) => value ? "Sí" : "No";

    private void OnOptionChanged()
    {
        UpdateTexts();
        m_audioEvents.OnPlaySound(AudioEvents.SoundType.PlayerEatDot);
    }

    private void OnRound()
    {
        GameSettings.StartRound = (GameSettings.StartRound + 1) % m_gameConfig.RoundConfigs.Length;
        OnOptionChanged();
    }

    private void OnMovingCollectibles()
    {
        GameSettings.MovingCollectibles = !GameSettings.MovingCollectibles;
        OnOptionChanged();
    }

    private void OnMusic()
    {
        GameSettings.MusicOn = !GameSettings.MusicOn;
        OnOptionChanged();
    }

    private void OnSound()
    {
        GameSettings.SoundOn = !GameSettings.SoundOn;
        OnOptionChanged();
    }

    private void OnInfo()
    {
        SwitchToMenuUI(UIEvents.ShowUIType.Info);
    }

    private void OnBack()
    {
        SwitchToMenuUI(UIEvents.ShowUIType.Menu);
    }
}
