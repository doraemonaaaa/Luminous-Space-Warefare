using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class HotKeyEventMapping
{
    [Tooltip("要监听的输入动作")]
    public InputAction inputAction;

    [Header("阶段性事件")]
    public UnityEvent onStarted;     // 按键刚被按下
    public UnityEvent onPressed;     // 达到触发条件时（例如按下到位）
    public UnityEvent onReleased;    // 松开时触发
    public UnityEvent onHeld;        // 持续按住时每帧调用

    private bool isHeld = false;
    private bool isInitialized = false;

    /// <summary>
    /// 初始化并启用输入绑定
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
    /// 禁用输入并解绑事件
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
    /// 每帧调用，处理持续按住事件
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
