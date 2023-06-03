using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class PausedEvents : MonoBehaviour
{
    public Button m_continueButton;
    public Button m_exitButton;

    void OnEnable()
    {
        m_continueButton.onClick.AddListener(OnContinue);
        m_exitButton.onClick.AddListener(OnExit);
    }

    private void OnExit()
    {
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

    void OnDisable()
    {
        m_continueButton.onClick.RemoveListener(OnContinue);
        m_exitButton.onClick.RemoveListener(OnExit);
    }
}
