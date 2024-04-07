using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;


public class MenuEvents : MonoBehaviour
{
    public AudioEvents m_audioEvents;
    public UIEvents m_uiEvents;
    public float m_fadeTime;
    public Button m_cancelButton;
    public Selectable m_defaultSelected;
    private InputAction m_cancelAction;

    protected virtual void OnEnable()
    {
        EnableCancelAction();
        SetDefaultSelected();
    }

    protected virtual void OnDisable()
    {
        DisableCancelAction();
    }

    private void EnableCancelAction()
    {
        var inputModule = EventSystem.current.currentInputModule as InputSystemUIInputModule;
        m_cancelAction = inputModule.actionsAsset.FindAction("Cancel");
        m_cancelAction.performed += OnCancelPerformed;
    }

    private void DisableCancelAction()
    {
        m_cancelAction.performed -= OnCancelPerformed;
    }

    private void OnCancelPerformed(InputAction.CallbackContext obj)
    {
        m_cancelButton.onClick.Invoke();
    }

    private void SetDefaultSelected()
    {
        var eventSystem = EventSystem.current;
        var currentSelected = eventSystem.currentSelectedGameObject;
        if (currentSelected != null && currentSelected.activeInHierarchy
            && currentSelected.transform.IsChildOf(transform))
        {
            return;
        }
        eventSystem.SetSelectedGameObject(m_defaultSelected.gameObject);
    }

    protected IEnumerator OnClickButtonFadeAsync(AudioEvents.SoundType clickSound)
    {
        m_audioEvents.OnPlaySound(clickSound);
        m_audioEvents.OnFadeMusic(false, m_fadeTime);
        m_uiEvents.OnFadeAnimation(false, 0.5f);
        yield return new WaitForSeconds(m_fadeTime);
    }

    protected void SwitchToMenuUI(UIEvents.ShowUIType uiType)
    {
        m_audioEvents.OnPlaySound(AudioEvents.SoundType.PlayerEatDot);
        m_uiEvents.OnShowUI(uiType);
        UISystem.Instance?.OnSwitchToMenuUI(uiType);
    }
}
