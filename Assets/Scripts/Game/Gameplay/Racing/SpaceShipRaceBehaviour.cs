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
        protected float minThrottleWhileTurning = 0.6f; // 增加最小油门值

        [SerializeField]
        protected float lookAheadDistance = 50f; // 前瞻距离

        [SerializeField]
        protected float corneringSpeed = 0.8f; // 过弯速度系数

        [SerializeField]
        protected float boostChance = 0.2f; // 随机加速概率

        [SerializeField]
        protected float boostDuration = 2f; // 加速持续时间

        [SerializeField]
        protected float boostMultiplier = 1.2f; // 加速倍率

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

        protected float turnAnticipation = 0f; // 转弯预判

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

        // 计算前瞻点
        protected Vector3 CalculateLookAheadPoint()
        {
            // 简单实现：当前目标和下一个目标之间的插值点
            float distanceToTarget = Vector3.Distance(vehicle.transform.position, targetPosition);

            // 当接近当前航点时，开始考虑下一个航点
            float blendFactor = Mathf.Clamp01(1 - (distanceToTarget / lookAheadDistance));
            return Vector3.Lerp(targetPosition, nextCheckpointPosition, blendFactor * 0.5f);
        }

        public override bool BehaviourUpdate()
        {
            if (!base.BehaviourUpdate()) return false;
            if (racePlayer.curTargetWaypoint == null) return false;
            UpdateBoostState();

            float distanceToTarget = Vector3.Distance(vehicle.transform.position, targetPosition);

            // 使用预测位置提前转向
            Vector3 predictiveTarget = targetPosition;

            Maneuvring.TurnToward(vehicle.transform, predictiveTarget, maxRotationAngles, shipPIDController.steeringPIDController);
            engines.SetSteeringInputs(shipPIDController.steeringPIDController.GetControlValues());

            float turnIntensity = shipPIDController.steeringPIDController.GetControlValues().magnitude;
            float turnDifficulty = Vector3.Angle((targetPosition - vehicle.transform.position), (nextCheckpointPosition - targetPosition)) / 180f;

            // 根据距离和转弯难度智能调整油门
            float throttle = fullThrottle;
            bool needCornerSlow = distanceToTarget < 100f || turnDifficulty > 0.3f;             // 如果距离近 或者 转弯角度大，才减速

            if (needCornerSlow)
            {
                // 使用调整后的 intensity 减速（指数控制减速比更灵敏）
                float adjustedIntensity = Mathf.Clamp01(turnIntensity * turnIntensity);
                throttle = Mathf.Lerp(corneringSpeed, fullThrottle, 1f - adjustedIntensity);

                // 极端情况直接设为最小油门
                if (distanceToTarget < 50f && turnDifficulty > 0.5f)
                {
                    throttle = minThrottleWhileTurning;
                }
            }

            if (isBoosting)
                throttle *= boostMultiplier;

            engines.SetMovementInputs(new Vector3(0, 0, Mathf.Clamp01(throttle)));

            // 提前判断是否需要紧急刹车（防止绕圈圈）
            if (distanceToTarget < 50f && Vector3.Angle(vehicle.transform.forward, targetPosition - vehicle.transform.position) > 45f)
            {
                engines.SetMovementInputs(new Vector3(0, 0, minThrottleWhileTurning * 0.5f)); // 紧急大幅减速
            }

            //Debug.Log("AI的油门量:" + throttle);

            // 竞争策略（超车逻辑）
            //HandleCompetitorAvoidance();

            return true;
        }


        // 超车和竞争感处理
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
                    break; // 找到一个就避开，立刻退出
                }
            }
        }


        // 新增：尝试激活加速
        public void TryActivateBoost()
        {
            if (!isBoosting && Random.value < boostChance)
            {
                // 根据比赛情况动态调整加速概率
                float dynamicBoostChance = boostChance;
                if (racePlayer.currentLap >= racePlayer.totalLaps - 1) dynamicBoostChance *= 1.5f; // 最后一圈更积极
                if (dynamicBoostChance >= Random.value)
                {
                    isBoosting = true;
                    currentBoostTime = boostDuration;
                }
            }
        }

        // 新增：更新加速状态
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

        // 显示调试信息
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