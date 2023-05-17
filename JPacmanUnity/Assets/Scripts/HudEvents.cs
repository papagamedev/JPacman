using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class HudEvents : MonoBehaviour
{
    public TMP_Text m_scoreLabel;
    public TMP_Text m_livesLabel;
    public TMP_Text m_messageLabel;

    private void OnEnable()
    {
        var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
        hudSystem.OnSetLabelText += OnSetLabelText;
        hudSystem.OnSetLivesText += OnSetLivesText;
        hudSystem.OnSetScoreText += OnSetScoreText;
        hudSystem.OnStartScoreAnimation += OnStartScoreAnimation;

        Application.targetFrameRate = 60;
    }

    private void OnDisable()
    {
        KillAllScoreAnimations();

        if (World.DefaultGameObjectInjectionWorld != null)
        {
            var hudSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HudSystem>();
            hudSystem.OnSetLabelText -= OnSetLabelText;
            hudSystem.OnSetLivesText -= OnSetLivesText;
            hudSystem.OnSetScoreText -= OnSetScoreText;
            hudSystem.OnStartScoreAnimation -= OnStartScoreAnimation;
        }
    }

    private void OnStartScoreAnimation(int arg1, float3 arg2)
    {
        
    }

    private void KillAllScoreAnimations()
    {

    }

    private void OnSetScoreText(int obj)
    {
        m_scoreLabel.text = obj.ToString();
    }

    private void OnSetLivesText(int obj)
    {
        m_livesLabel.text = obj.ToString();
    }

    private void OnSetLabelText(LevelStartPhaseSystem.LabelMode obj)
    {
        switch (obj)
        {
            case LevelStartPhaseSystem.LabelMode.None:
                m_messageLabel.text = "";
                return;
            case LevelStartPhaseSystem.LabelMode.Round:
                m_messageLabel.text = "Ronda 1";
                return;
            case LevelStartPhaseSystem.LabelMode.Message:
                m_messageLabel.text = "Nivel 1";
                return;
        }
    }
}
