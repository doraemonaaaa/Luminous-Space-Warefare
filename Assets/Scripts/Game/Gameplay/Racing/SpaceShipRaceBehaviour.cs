using UnityEngine;
using UnityEngine.Events;
using VSX.AI;
using VSX.Engines3D;
using VSX.SpaceCombatKit;
using VSX.Vehicles;

namespace VSX.SpaceCombatKit
{
    public class SpaceshipRaceBehaviour : AISpaceshipBehaviour
    {
        [SerializeField]
        protected float fullThrottle = 1f;

        [SerializeField]
        protected float minThrottleWhileTurning = 0.6f; // ������С����ֵ

        [SerializeField]
        protected float lookAheadDistance = 50f; // ǰհ����

        [SerializeField]
        protected float corneringSpeed = 0.8f; // �����ٶ�ϵ��

        [SerializeField]
        protected float boostChance = 0.2f; // ������ٸ���

        [SerializeField]
        protected float boostDuration = 2f; // ���ٳ���ʱ��

        [SerializeField]
        protected float boostMultiplier = 1.2f; // ���ٱ���

        protected float currentBoostTime = 0f;
        protected bool isBoosting = false;
        protected Vector3 targetPosition
        {
            get
            {
                if (racePlayer.curTargetWaypoint == null)
                {
                    return Vector3.zero;
                }

                return racePlayer.curTargetWaypoint.position;
            }
        }
        protected Vector3 nextCheckpointPosition
        {
            get
            {
                if (racePlayer.nextTargetWaypoint == null)
                {
                    return Vector3.zero;
                }

                return racePlayer.nextTargetWaypoint.position;
            }
        }

        protected float turnAnticipation = 0f; // ת��Ԥ��

        private RaycastHit[] competitorHitsBuffer = new RaycastHit[30];
        private bool raceEnded = false;

        public RacePlayer racePlayer;

        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            engines = vehicle.GetComponent<VehicleEngines3D>();
            if (engines == null) { return false; }

            return true;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        // ����ǰհ��
        protected Vector3 CalculateLookAheadPoint()
        {
            // ��ʵ�֣���ǰĿ�����һ��Ŀ��֮��Ĳ�ֵ��
            float distanceToTarget = Vector3.Distance(vehicle.transform.position, targetPosition);

            // ���ӽ���ǰ����ʱ����ʼ������һ������
            float blendFactor = Mathf.Clamp01(1 - (distanceToTarget / lookAheadDistance));
            return Vector3.Lerp(targetPosition, nextCheckpointPosition, blendFactor * 0.5f);
        }

        public override bool BehaviourUpdate()
        {
            if (!base.BehaviourUpdate()) return false;
            if (racePlayer.curTargetWaypoint == null) return false;
            UpdateBoostState();

            float distanceToTarget = Vector3.Distance(vehicle.transform.position, targetPosition);

            // ʹ��Ԥ��λ����ǰת��
            Vector3 predictiveTarget = targetPosition;

            Maneuvring.TurnToward(vehicle.transform, predictiveTarget, maxRotationAngles, shipPIDController.steeringPIDController);
            engines.SetSteeringInputs(shipPIDController.steeringPIDController.GetControlValues());

            float turnIntensity = shipPIDController.steeringPIDController.GetControlValues().magnitude;
            float turnDifficulty = Vector3.Angle((targetPosition - vehicle.transform.position), (nextCheckpointPosition - targetPosition)) / 180f;

            // ���ݾ����ת���Ѷ����ܵ�������
            float throttle = fullThrottle;
            bool needCornerSlow = distanceToTarget < 100f || turnDifficulty > 0.3f;             // �������� ���� ת��Ƕȴ󣬲ż���

            if (needCornerSlow)
            {
                // ʹ�õ������ intensity ���٣�ָ�����Ƽ��ٱȸ�������
                float adjustedIntensity = Mathf.Clamp01(turnIntensity * turnIntensity);
                throttle = Mathf.Lerp(corneringSpeed, fullThrottle, 1f - adjustedIntensity);

                // �������ֱ����Ϊ��С����
                if (distanceToTarget < 50f && turnDifficulty > 0.5f)
                {
                    throttle = minThrottleWhileTurning;
                }
            }

            if (isBoosting)
                throttle *= boostMultiplier;

            engines.SetMovementInputs(new Vector3(0, 0, Mathf.Clamp01(throttle)));

            // ��ǰ�ж��Ƿ���Ҫ����ɲ������ֹ��ȦȦ��
            if (distanceToTarget < 50f && Vector3.Angle(vehicle.transform.forward, targetPosition - vehicle.transform.position) > 45f)
            {
                engines.SetMovementInputs(new Vector3(0, 0, minThrottleWhileTurning * 0.5f)); // �����������
            }

            //Debug.Log("AI��������:" + throttle);

            // �������ԣ������߼���
            //HandleCompetitorAvoidance();

            return true;
        }


        // �����;����д���
        protected void HandleCompetitorAvoidance()
        {
            int hitCount = Physics.SphereCastNonAlloc(vehicle.transform.position, 15f, vehicle.transform.forward, competitorHitsBuffer, 100f);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = competitorHitsBuffer[i];
                Vehicle otherVehicle = hit.collider.GetComponentInParent<Vehicle>();
                if (otherVehicle != null && otherVehicle != this.vehicle)
                {
                    Vector3 avoidDir = Vector3.Cross(vehicle.transform.forward, hit.point - vehicle.transform.position).normalized;
                    Vector3 targetAvoidance = vehicle.transform.position + avoidDir * 30f;
                    Maneuvring.TurnToward(vehicle.transform, targetAvoidance, maxRotationAngles, shipPIDController.steeringPIDController);
                    engines.SetSteeringInputs(shipPIDController.steeringPIDController.GetControlValues());
                    engines.SetMovementInputs(new Vector3(0, 0, fullThrottle));
                    TryActivateBoost();
                    break; // �ҵ�һ���ͱܿ��������˳�
                }
            }
        }


        // ���������Լ������
        public void TryActivateBoost()
        {
            if (!isBoosting && Random.value < boostChance)
            {
                // ���ݱ��������̬�������ٸ���
                float dynamicBoostChance = boostChance;
                if (racePlayer.currentLap >= racePlayer.totalLaps - 1) dynamicBoostChance *= 1.5f; // ���һȦ������
                if (dynamicBoostChance >= Random.value)
                {
                    isBoosting = true;
                    currentBoostTime = boostDuration;
                }
            }
        }

        // ���������¼���״̬
        protected void UpdateBoostState()
        {
            if (isBoosting)
            {
                currentBoostTime -= Time.deltaTime;
                if (currentBoostTime <= 0)
                {
                    isBoosting = false;
                }
            }
        }

        // ��ʾ������Ϣ
        protected void OnDrawGizmos()
        {
            if (!Application.isPlaying || vehicle == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 2f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(vehicle.transform.position, targetPosition);
        }
    }
}