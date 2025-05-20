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
    public float landingDistance = 50f;      // �������������
    public float landingSpeed = 2f;          // ��½�½��ٶ�
    public float alignmentThreshold = 0.95f; // �뷨�߶������ֵ(0-1)��ֵԽ��Ҫ��Խ��ȷ
    public float rotationSpeed = 1f;         // ��ת�����ٶ�
    public float takeoffHeight = 20f;        // ��ɸ߶�
    public float takeoffSpeed = 5f;          // ����ٶ�
    public LayerMask landingSurfaceMask;     // �������ͼ��

    private Transform targetSurfaceTransform;
    private Vector3 landingNormal;

    public enum LandingState
    {
        NotLanding,      // ��������
        Aligning,        // ��������淨�߶���
        Descending,      // ���ڽ���
        Landed,          // ����½
        TakingOff        // �������
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

        // ������½��
        if (landingInputAction.WasPressedThisFrame() && landingState == LandingState.NotLanding)
        {
            TryStartLanding();
        }

        // ������ɼ�
        if (landingInputAction.WasPressedThisFrame() && landingState == LandingState.Landed)
        {
            StartTakeoff();
        }

        // ����״ִ̬����Ӧ����
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
                // �ɴ�����½���ȴ����ָ��
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

        Vector3 linearInput = new Vector3(strafe, 0f, throttle); // x: ���ƣ�y: ���£�z: �ƽ�
        Vector3 angularInput = new Vector3(pitch, yaw, roll);    // x: ������y: ƫ����z: ����

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

                // ������������
                throttle = 0f;
                strafe = 0f;
                roll = 0f;
                yaw = 0f;
                pitch = 0f;

                // ȷ��ɲ�����ܹر�
                ship.SetBrake(false);

                // ��ʼ��ת����׶�
                landingState = LandingState.Aligning;
                // ���ɴ���Ϊ������Ӷ����Ա���������˶�
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
        // ���ȼ���Ƿ���Ȼ���Կ�������
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, -transform.up, out hit, landingDistance, landingSurfaceMask))
        {
            Debug.LogWarning("Lost sight of landing surface, aborting landing.");
            AbortLanding();
            return;
        }

        // ���µ��淨��
        landingNormal = hit.normal;

        // ���������Ķ���̶� (���Ϊ1ʱ��ȫ����)
        float alignment = Vector3.Dot(-transform.up, landingNormal);

        // ��ת������淨��
        Quaternion targetRotation = Quaternion.FromToRotation(-transform.up, landingNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // ����ת�����У����ַɴ��ڿ��У���Ҫ�½�
        ship.Drive(Vector3.zero, Vector3.zero);

        // ������ȴﵽ��ֵʱ�������½��׶�
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

        // ������������淨�ߵĶ���
        landingNormal = hit.normal;
        Quaternion targetRotation = Quaternion.FromToRotation(-transform.up, landingNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // ����ǳ��ӽ����棬�����½
        if (hit.distance < 1.0f)
        {
            CompleteLanding(hit);
            return;
        }

        // �����½�
        Vector3 descentVector = new Vector3(0, -landingSpeed, 0); // ʹ�þֲ�Y����½�����
        ship.Drive(descentVector, Vector3.zero);
    }

    private void CompleteLanding(RaycastHit hit)
    {
        // ֹͣ������������
        ship.Drive(Vector3.zero, Vector3.zero);

        // ��������λ�ã�ȷ���ɴ��ײ������Ӵ�
        Vector3 finalPosition = hit.point + landingNormal * 0.5f; // 0.5f�Ƿɴ������ĵ��ײ��ľ���
        transform.position = finalPosition;

        // ����������ת��ʹ�ɴ��ײ������ƽ��
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
        // �����ѷ��еĸ߶�
        float currentHeight = Vector3.Distance(transform.position, takeoffStartPosition);

        if (currentHeight >= takeoffHeight)
        {
            // �ﵽĿ��߶ȣ�������
            CompleteTakeoff();
            return;
        }

        // ������� - ���ŷɴ����Ϸ�������
        Vector3 upwardVector = new Vector3(0, takeoffSpeed, 0); // �ֲ�Y�����������
        ship.Drive(upwardVector, Vector3.zero);
    }

    private void CompleteTakeoff()
    {
        Debug.Log("Takeoff completed successfully.");
        landingState = LandingState.NotLanding;
        ship.Drive(Vector3.zero, Vector3.zero);
    }
}