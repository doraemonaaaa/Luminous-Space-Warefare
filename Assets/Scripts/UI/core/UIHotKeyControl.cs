using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;



public class UIHotKeyControl : MonoBehaviour
{
    [Tooltip("在这里配置每个按键和它的触发事件")]
    public List<HotKeyEventMapping> hotKeyEventList;

    private void Awake()
    {
        EnableInput();
    }

    private void OnDisable()
    {
        DisableInput();
    }

    private void OnEnable()
    {
        EnableInput();
    }

    public void EnableInput()
    {
        foreach (var hke in hotKeyEventList)
        {
            hke.Enable();
        }
    }

    public void DisableInput()
    {
        foreach (var hke in hotKeyEventList)
        {
            hke.Disable();
        }
    }
}
