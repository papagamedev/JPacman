using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;


public class IntroEvents : MonoBehaviour
{
    IDisposable m_anyButtonHandler;

    void OnEnable()
    {
        m_anyButtonHandler = InputSystem.onAnyButtonPress.Call(x => OnSkip());
    }

    private void OnDisable()
    {
        m_anyButtonHandler.Dispose();
    }

    private void OnSkip()
    {
        UISystem.Instance?.OnIntroSkip();
    }

}
