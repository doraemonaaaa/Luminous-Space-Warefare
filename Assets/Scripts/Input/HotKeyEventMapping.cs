using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class HotKeyEventMapping
{
    [Tooltip("Ҫ���������붯��")]
    public InputAction inputAction;

    [Header("�׶����¼�")]
    public UnityEvent onStarted;     // �����ձ�����
    public UnityEvent onPressed;     // �ﵽ��������ʱ�����簴�µ�λ��
    public UnityEvent onReleased;    // �ɿ�ʱ����
    public UnityEvent onHeld;        // ������סʱÿ֡����

    private bool isHeld = false;
    private bool isInitialized = false;

    /// <summary>
    /// ��ʼ�������������
    /// </summary>
    public void Enable()
    {
        if (inputAction == null) return;
        if (!isInitialized)
        {
            inputAction.started += OnStarted;
            inputAction.performed += OnPerformed;
            inputAction.canceled += OnCanceled;
            isInitialized = true;
        }

        inputAction.Enable();
    }

    /// <summary>
    /// �������벢����¼�
    /// </summary>
    public void Disable()
    {
        if (inputAction == null) return;

        inputAction.Disable();
        isHeld = false;
    }

    private void OnStarted(InputAction.CallbackContext ctx)
    {
        if (GlobalInputBlocker.InputBlocked)
            return;

        onStarted?.Invoke();
    }

    private void OnPerformed(InputAction.CallbackContext ctx)
    {
        if (GlobalInputBlocker.InputBlocked)
            return;

        if (ctx.phase == InputActionPhase.Performed)
        {
            onPressed?.Invoke();
            isHeld = true;
        }
    }

    private void OnCanceled(InputAction.CallbackContext ctx)
    {
        if (GlobalInputBlocker.InputBlocked)
            return;
        onReleased?.Invoke();
        isHeld = false;
    }

    /// <summary>
    /// ÿ֡���ã����������ס�¼�
    /// </summary>
    public void Update()
    {
        if (GlobalInputBlocker.InputBlocked)
            return;
        if (isHeld)
        {
            onHeld?.Invoke();
        }
    }
}
