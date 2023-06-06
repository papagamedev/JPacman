using System.Collections;
using Unity.Entities;
using UnityEngine.UI;


public class PausedMenuEvents : MenuEvents
{
    public Button m_continueButton;
    public Button m_exitButton;

    void OnEnable()
    {
        m_continueButton.onClick.AddListener(OnContinue);
        m_exitButton.onClick.AddListener(OnExit);
    }

    void OnDisable()
    {
        m_continueButton.onClick.RemoveListener(OnContinue);
        m_exitButton.onClick.RemoveListener(OnExit);
    }

    private void OnExit()
    {
        StartCoroutine(OnExitAsync());
    }

    private IEnumerator OnExitAsync()
    {
        yield return OnClickButtonFadeAsync(AudioEvents.SoundType.PlayerEatFruit);
        if (World.DefaultGameObjectInjectionWorld != null)
        {
            var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
            hudSystem.OnPausedExit();
        }
    }

    private void OnContinue()
    {
        if (World.DefaultGameObjectInjectionWorld != null)
        {
            var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
            hudSystem.OnPausedContinue();
        }
    }

}
