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

        // ��ǰ�ٶ�
        Vector3 velocity = rb.linearVelocity;

        // ԭʼ���ٶ�
        Vector3 rawAccel = (velocity - lastVelocity) / Time.deltaTime;

        // ƽ�����ٶȣ���ͨ�˲���
        smoothedAcceleration = Vector3.Lerp(smoothedAcceleration, rawAccel, Time.deltaTime / accelerationSmoothTime);
        float accelMag = smoothedAcceleration.magnitude;

        // Ŀ�����
        float targetDistance = baseDistance + Mathf.Clamp01(accelMag / accelerationToMaxDistance) * maxExtraDistance;

        // ƽ���������仯
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * distanceSmoothSpeed);
        followComponent.CameraDistance = currentDistance;

        // ������ʷ�ٶ�
        lastVelocity = velocity;
    }
}
