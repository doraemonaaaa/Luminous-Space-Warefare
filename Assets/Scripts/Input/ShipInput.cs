//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using MyPhysics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using static ShipInput;
using static ShipSkills;

/// <summary>
/// Class specifically to deal with input.
/// </summary>
public class ShipInput : MonoBehaviour
{
    [Tooltip("When true, the mouse and mousewheel are used for ship input and A/D can be used for strafing like in many arcade space sims.\n\nOtherwise, WASD/Arrows/Joystick + R/T are used for flying, representing a more traditional style space sim.")]
    public bool useMouseInput = true;
    [Tooltip("When using Keyboard/Joystick input, should roll be added to horizontal stick movement. This is a common trick in traditional space sims to help ships roll into turns and gives a more plane-like feeling of flight.")]
    public bool addRoll = true;

    [Tooltip("Radius (0~1) within which mouse movement is ignored for pitch/yaw input.")]
    [Range(0, 1)]
    public float deadZoneRadius = 0.05f; // 5% 屏幕半径的死区

    public RectTransform mouseIndicator;      // 鼠标方向UI指示器（例如箭头、圆点）
    public RectTransform centerPoint;         // 屏幕中心点UI（例如小圆点）
    public UILineDrawer uiLineDrawer;

    [Space]

    [Range(-1, 1)]
    public float pitch;
    [Range(-1, 1)]
    public float yaw;
    [Range(-1, 1)]
    public float roll;
    [Range(-1, 1)]
    public float strafe;
    [Range(0, 1)]
    public float throttle;

    // Ship skills 
    [System.Serializable]
    public class SkillBinding
    {
        public KeyCode key;
        public ShipSkillEvent action;

        public float delay_time;
        public EffectWrapper effect;
        public float skill_val;
        public Transform transform;

    }
    public List<SkillBinding> skillBindings = new List<SkillBinding>();

    [Header("Emergency Brake")]
    public KeyCode brakeKey = KeyCode.B;

    // How quickly the throttle reacts to input.
    private const float THROTTLE_SPEED = 0.5f;
    private const float BOOST_MULTIPLIER = 2.0f; // Shift 加速倍数

    // Keep a reference to the ship this is attached to just in case.
    private Ship ship;

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }

    private void Update()
    {
        if (useMouseInput)
        {
            strafe = Input.GetAxis("Horizontal");
            SetStickCommandsUsingMouse();
            UpdateMouseWheelThrottle();
            UpdateKeyboardThrottle(KeyCode.W, KeyCode.S, KeyCode.LeftShift);

            HandleSkills();
        }
        else
        {            
            pitch = Input.GetAxis("Vertical");
            yaw = Input.GetAxis("Horizontal");

            if (addRoll)
                roll = -Input.GetAxis("Horizontal") * 0.5f;

            strafe = 0.0f;
            UpdateKeyboardThrottle(KeyCode.R, KeyCode.F, KeyCode.LeftShift);

            HandleSkills();
        }

        ship.SetBrake(Input.GetKey(brakeKey));

    }

    /// <summary>
    /// Freelancer style mouse controls. This uses the mouse to simulate a virtual joystick.
    /// When the mouse is in the center of the screen, this is the same as a centered stick.
    /// </summary>
    private void SetStickCommandsUsingMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        // 转换成以屏幕中心为原点，范围 [-1, 1]
        float dx = (mousePos.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
        float dy = (mousePos.y - Screen.height * 0.5f) / (Screen.height * 0.5f);

        Vector2 delta = new Vector2(dx, dy);
        float magnitude = delta.magnitude;

        // 始终显示 mouseIndicator
        if (mouseIndicator != null)
        {
            mouseIndicator.gameObject.SetActive(true);  // 确保指示器始终可见
            mouseIndicator.anchoredPosition = new Vector2(dx * (Screen.width * 0.5f), dy * (Screen.height * 0.5f)); // 设置指示器位置
        }

        // 如果鼠标在 dead zone 内
        if (magnitude < deadZoneRadius)
        {
            pitch = 0f;
            yaw = 0f;

            if (uiLineDrawer != null) uiLineDrawer.enabled = false;  // 在死区内不绘制连线
        }
        else
        {
            // 死区外的鼠标控制
            Vector2 adjusted = delta.normalized * ((magnitude - deadZoneRadius) / (1 - deadZoneRadius));
            pitch = -Mathf.Clamp(adjusted.y, -1f, 1f);
            yaw = Mathf.Clamp(adjusted.x, -1f, 1f);

            if (uiLineDrawer != null)
            {
                uiLineDrawer.enabled = true;  // 在死区外启用连线渲染
                uiLineDrawer.SetLine(centerPoint.anchoredPosition, mouseIndicator.anchoredPosition);
            }
        }
    }



    /// <summary>
    /// Updates the throttle based on given input keys. Holding boostKey increases throttle speed.
    /// </summary>
    private void UpdateKeyboardThrottle(KeyCode increaseKey, KeyCode decreaseKey, KeyCode boostKey)
    {
        float target = throttle;

        if (Input.GetKey(increaseKey))
            target = 1.0f;
        else if (Input.GetKey(decreaseKey))
            target = 0.0f;
        else 
            target = 0.0f;

        float speedMultiplier = Input.GetKey(boostKey) ? BOOST_MULTIPLIER : 1.0f;

        throttle = Mathf.MoveTowards(throttle, target, Time.deltaTime * THROTTLE_SPEED * speedMultiplier);
    }

    /// <summary>
    /// Uses the mouse wheel to control the throttle.
    /// </summary>
    private void UpdateMouseWheelThrottle()
    {
        throttle += Input.GetAxis("Mouse ScrollWheel");
        throttle = Mathf.Clamp(throttle, 0.0f, 1.0f);
    }

    private void HandleSkills()
    {
        foreach (SkillBinding binding in skillBindings)
        {
            if (Input.GetKeyDown(binding.key))
            {
                binding.action.Invoke(binding.delay_time, binding.effect, binding.skill_val, binding.transform);
            }
        }
    }



}