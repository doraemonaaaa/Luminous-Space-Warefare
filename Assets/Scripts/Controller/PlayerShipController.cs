using MyPhysics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static ShipSkills;

[RequireComponent(typeof(CameraLag))]
public class PlayerShipController : MonoBehaviour
{
    private Ship ship;

    [Header("Input Settings")]
    public bool useMouseInput = true;
    public bool addRoll = true;
    [Range(0, 1)]
    public float deadZoneRadius = 0.2f;

    public RectTransform mouseIndicator;
    public RectTransform centerPoint;
    public UILineDrawer uiLineDrawer;

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

    [System.Serializable]
    public class SkillBinding
    {
        public InputAction inputAction;
        public ShipSkillEvent action;
        public float delay_time;
        public EffectWrapper effect;
        public float skill_val;
        public Transform transform;
    }
    public List<SkillBinding> skillBindings = new List<SkillBinding>();

    [System.Serializable]
    public class CombatBinding
    {
        public InputAction inputAction;
        public UnityEvent onClick;      
        public UnityEvent onPressing;   
        public UnityEvent onRelease;   
    }
    public List<CombatBinding> combatBindings = new List<CombatBinding>();

    public InputAction brakeInputAction;

    private const float THROTTLE_SPEED = 0.5f;
    private const float BOOST_MULTIPLIER = 2.0f;

    [Header("Landing/Takeoff Settings")]
    public InputAction landingInputAction;
    public float landingDistance = 50f;      // 检测地面的最大距离
    public float landingSpeed = 2f;          // 着陆下降速度
    public float alignmentThreshold = 0.95f; // 与法线对齐的阈值(0-1)，值越高要求越精确
    public float rotationSpeed = 1f;         // 旋转对齐速度
    public float takeoffHeight = 20f;        // 起飞高度
    public float takeoffSpeed = 5f;          // 起飞速度
    public LayerMask landingSurfaceMask;     // 地面检测的图层

    private Transform targetSurfaceTransform;
    private Vector3 landingNormal;

    public enum LandingState
    {
        NotLanding,      // 正常飞行
        Aligning,        // 正在与地面法线对齐
        Descending,      // 正在降落
        Landed,          // 已着陆
        TakingOff        // 正在起飞
    }

    public LandingState landingState = LandingState.NotLanding;
    private Vector3 takeoffStartPosition;

    private void Awake()
    {
        ship = GetComponent<Ship>();

        brakeInputAction.Enable();
        landingInputAction.Enable();

        foreach (var binding in skillBindings)
        {
            binding.inputAction.Enable();
        }

        foreach (var binding in combatBindings)
        {
            binding.inputAction.Enable();
        }
    }

    private void Update()
    {

        if (landingInputAction.WasPressedThisFrame() && landingState == LandingState.Aligning)
        {
            AbortLanding();
        }

        // 处理着陆键
        if (landingInputAction.WasPressedThisFrame() && landingState == LandingState.NotLanding)
        {
            TryStartLanding();
        }

        // 处理起飞键
        if (landingInputAction.WasPressedThisFrame() && landingState == LandingState.Landed)
        {
            StartTakeoff();
        }

        // 根据状态执行相应操作
        switch (landingState)
        {
            case LandingState.NotLanding:
                HandleInput();
                break;
            case LandingState.Aligning:
                HandleAlignment();
                break;
            case LandingState.Descending:
                HandleDescent();
                break;
            case LandingState.Landed:
                // 飞船已着陆，等待起飞指令
                break;
            case LandingState.TakingOff:
                HandleTakeoff();
                break;
        }
    }

    private void HandleInput()
    {
        if (useMouseInput)
        {
            strafe = Input.GetAxis("Horizontal");
            SetStickCommandsUsingMouse();
            UpdateMouseWheelThrottle();
            UpdateKeyboardThrottle(KeyCode.W, KeyCode.S, KeyCode.LeftShift);
        }
        else
        {
            pitch = Input.GetAxis("Vertical");
            yaw = Input.GetAxis("Horizontal");

            roll = addRoll ? -Input.GetAxis("Horizontal") * 0.5f : 0f;
            strafe = 0.0f;

            UpdateKeyboardThrottle(KeyCode.R, KeyCode.F, KeyCode.LeftShift);
        }

        HandleSkills();
        HandleCombat();

        Vector3 linearInput = new Vector3(strafe, 0f, throttle); // x: 横移，y: 上下，z: 推进
        Vector3 angularInput = new Vector3(pitch, yaw, roll);    // x: 俯仰，y: 偏航，z: 翻滚

        ship.Drive(linearInput, angularInput);
        ship.SetBrake(brakeInputAction.IsPressed());
    }

    private void SetStickCommandsUsingMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        float dx = (mousePos.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
        float dy = (mousePos.y - Screen.height * 0.5f) / (Screen.height * 0.5f);

        Vector2 delta = new Vector2(dx, dy);
        float magnitude = delta.magnitude;

        if (mouseIndicator != null)
        {
            mouseIndicator.gameObject.SetActive(true);
            mouseIndicator.anchoredPosition = new Vector2(dx * (Screen.width * 0.5f), dy * (Screen.height * 0.5f));
        }

        if (magnitude < deadZoneRadius)
        {
            pitch = 0f;
            yaw = 0f;

            if (uiLineDrawer != null)
                uiLineDrawer.enabled = false;
        }
        else
        {
            Vector2 adjusted = delta.normalized * ((magnitude - deadZoneRadius) / (1 - deadZoneRadius));
            pitch = -Mathf.Clamp(adjusted.y, -1f, 1f);
            yaw = Mathf.Clamp(adjusted.x, -1f, 1f);

            if (uiLineDrawer != null)
            {
                uiLineDrawer.enabled = true;
                uiLineDrawer.SetLine(centerPoint.anchoredPosition, mouseIndicator.anchoredPosition);
            }
        }
    }

    private void UpdateKeyboardThrottle(KeyCode increaseKey, KeyCode decreaseKey, KeyCode boostKey)
    {
        float target = 0.0f;
        if (Input.GetKey(increaseKey))
            target = 1.0f;
        else if (Input.GetKey(decreaseKey))
            target = 0.0f;

        float speedMultiplier = Input.GetKey(boostKey) ? BOOST_MULTIPLIER : 1.0f;
        throttle = Mathf.MoveTowards(throttle, target, Time.deltaTime * THROTTLE_SPEED * speedMultiplier);
    }

    private void UpdateMouseWheelThrottle()
    {
        throttle += Input.GetAxis("Mouse ScrollWheel");
        throttle = Mathf.Clamp(throttle, 0.0f, 1.0f);
    }

    private void HandleSkills()
    {
        foreach (var binding in skillBindings)
        {
            if (binding.inputAction.triggered)
            {
                binding.action.Invoke(binding.delay_time, binding.effect, binding.skill_val, binding.transform);
            }
        }
    }

    private void HandleCombat()
    {
        foreach (var binding in combatBindings)
        {
            if (binding.inputAction.triggered)
            {
                binding.onClick?.Invoke();
            }
            else if (binding.inputAction.WasReleasedThisFrame())
            {
                binding.onRelease?.Invoke();
            }
            else if (binding.inputAction.IsPressed())
            {
                binding.onPressing?.Invoke();
            }
        }
    }

    private void TryStartLanding()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, landingDistance, landingSurfaceMask))
        {
            var body = hit.collider.GetComponentInParent<CelestialBody_RigidBody>();
            var bodyTransform = hit.collider.GetComponentInParent<CelestialBody_Transform>();

            if (body != null || bodyTransform != null)
            {
                targetSurfaceTransform = (bodyTransform != null) ? bodyTransform.transform : body.transform;
                landingNormal = hit.normal;

                // 重置所有输入
                throttle = 0f;
                strafe = 0f;
                roll = 0f;
                yaw = 0f;
                pitch = 0f;

                // 确保刹车功能关闭
                ship.SetBrake(false);

                // 开始旋转对齐阶段
                landingState = LandingState.Aligning;
                // 将飞船变为星球的子对象，以便跟随星球运动
                transform.SetParent(targetSurfaceTransform);
                ship.Rigidbody.isKinematic = true;

                Debug.Log($"Landing initiated on {targetSurfaceTransform.name}. Aligning with surface normal...");
            }
            else
            {
                Debug.Log("No valid celestial body detected.");
            }
        }
        else
        {
            Debug.Log("No landing surface in range.");
        }
    }

    private void HandleAlignment()
    {
        // 首先检查是否仍然可以看到地面
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, -transform.up, out hit, landingDistance, landingSurfaceMask))
        {
            Debug.LogWarning("Lost sight of landing surface, aborting landing.");
            AbortLanding();
            return;
        }

        // 更新地面法线
        landingNormal = hit.normal;

        // 计算与地面的对齐程度 (点积为1时完全对齐)
        float alignment = Vector3.Dot(-transform.up, landingNormal);

        // 旋转对齐地面法线
        Quaternion targetRotation = Quaternion.FromToRotation(-transform.up, landingNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // 在旋转过程中，保持飞船在空中，不要下降
        ship.Drive(Vector3.zero, Vector3.zero);

        // 当对齐度达到阈值时，进入下降阶段
        if (alignment >= alignmentThreshold)
        {
            Debug.Log("Alignment complete. Starting descent...");
            landingState = LandingState.Descending;
        }
    }

    private void HandleDescent()
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, -transform.up, out hit, landingDistance, landingSurfaceMask))
        {
            Debug.LogWarning("Lost sight of landing surface during descent, aborting landing.");
            AbortLanding();
            return;
        }

        // 继续保持与地面法线的对齐
        landingNormal = hit.normal;
        Quaternion targetRotation = Quaternion.FromToRotation(-transform.up, landingNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // 如果非常接近地面，完成着陆
        if (hit.distance < 1.0f)
        {
            CompleteLanding(hit);
            return;
        }

        // 控制下降
        Vector3 descentVector = new Vector3(0, -landingSpeed, 0); // 使用局部Y轴的下降向量
        ship.Drive(descentVector, Vector3.zero);
    }

    private void CompleteLanding(RaycastHit hit)
    {
        // 停止所有物理输入
        ship.Drive(Vector3.zero, Vector3.zero);

        // 设置最终位置，确保飞船底部与地面接触
        Vector3 finalPosition = hit.point + landingNormal * 0.5f; // 0.5f是飞船从中心到底部的距离
        transform.position = finalPosition;

        // 设置最终旋转，使飞船底部与地面平行
        Quaternion finalRotation = Quaternion.FromToRotation(-transform.up, landingNormal) * transform.rotation;
        transform.rotation = finalRotation;

        Debug.Log("Landing completed successfully.");
        landingState = LandingState.Landed;
    }

    private void AbortLanding()
    {
        landingState = LandingState.NotLanding;
        targetSurfaceTransform = null;
        ship.Drive(Vector3.zero, Vector3.zero);
        transform.SetParent(null);
        ship.Rigidbody.isKinematic = false;
    }

    private void StartTakeoff()
    {
        if (landingState != LandingState.Landed)
            return;

        Debug.Log("Initiating takeoff sequence...");

        takeoffStartPosition = transform.position;

        transform.SetParent(null);

        landingState = LandingState.TakingOff;
    }

    private void HandleTakeoff()
    {
        // 计算已飞行的高度
        float currentHeight = Vector3.Distance(transform.position, takeoffStartPosition);

        if (currentHeight >= takeoffHeight)
        {
            // 达到目标高度，完成起飞
            CompleteTakeoff();
            return;
        }

        // 控制起飞 - 沿着飞船的上方向上升
        Vector3 upwardVector = new Vector3(0, takeoffSpeed, 0); // 局部Y轴的上升向量
        ship.Drive(upwardVector, Vector3.zero);
    }

    private void CompleteTakeoff()
    {
        Debug.Log("Takeoff completed successfully.");
        landingState = LandingState.NotLanding;
        ship.Drive(Vector3.zero, Vector3.zero);
    }
}