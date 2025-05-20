using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CameraLag : MonoBehaviour
{
    public CinemachineCamera cinemachineCam;

    [Header("Distance Settings")]
    public float baseDistance = 5f;
    public float maxExtraDistance = 10f;
    public float accelerationToMaxDistance = 50f;

    [Header("Smoothing")]
    public float accelerationSmoothTime = 0.2f;
    public float distanceSmoothSpeed = 5f;

    private CinemachineThirdPersonFollow followComponent;
    private Rigidbody rb;

    private Vector3 lastVelocity;
    private Vector3 smoothedAcceleration;
    private float currentDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        followComponent = cinemachineCam.GetComponent<CinemachineThirdPersonFollow>();
        if (followComponent == null)
        {
            Debug.LogError("Missing CinemachineThirdPersonFollow on Virtual Camera");
        }

        currentDistance = baseDistance;
        lastVelocity = rb.linearVelocity;
    }

    void LateUpdate()
    {
        if (followComponent == null) return;

        // 当前速度
        Vector3 velocity = rb.linearVelocity;

        // 原始加速度
        Vector3 rawAccel = (velocity - lastVelocity) / Time.deltaTime;

        // 平滑加速度（低通滤波）
        smoothedAcceleration = Vector3.Lerp(smoothedAcceleration, rawAccel, Time.deltaTime / accelerationSmoothTime);
        float accelMag = smoothedAcceleration.magnitude;

        // 目标距离
        float targetDistance = baseDistance + Mathf.Clamp01(accelMag / accelerationToMaxDistance) * maxExtraDistance;

        // 平滑相机距离变化
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * distanceSmoothSpeed);
        followComponent.CameraDistance = currentDistance;

        // 更新历史速度
        lastVelocity = velocity;
    }
}
