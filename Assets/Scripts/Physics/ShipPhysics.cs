using MyPhysics.Core;
using System.Collections;
using Unity.Collections;
using UnityEngine;

namespace MyPhysics
{
    [RequireComponent(typeof(Rigidbody))]
    public class ShipPhysics : MonoBehaviour
    {
        [Header("主推进系统")]
        [Tooltip("X: 横向推力\nY: 垂直推力\nZ: 纵向推力")]
        public Vector3 linearForce = new Vector3(100.0f, 100.0f, 100.0f);

        [Tooltip("X: 俯仰\nY: 偏航\nZ: 横滚")]
        public Vector3 angularForce = new Vector3(100.0f, 100.0f, 100.0f);

        [Tooltip("加速时的推进器特效")]
        public ParticleSystem mainThrusterVFX;
        [Tooltip("操控时的RCS推进器特效")]
        public ParticleSystem[] rcsThrustersVFX;

        [Header("制动系统")]
        [Tooltip("紧急制动时施加的最大线性制动力(在本地空间)")]
        public float maxBrakeForce = 500.0f;
        public float maxBrakeTorque = 50f; // 控制角速度制动的最大力矩
        public bool isBrake = false;
        [Tooltip("制动时的视觉效果")]
        public ParticleSystem brakeVFX;

        [Header("引擎设置")]
        [Range(0.0f, 1.0f)]
        [Tooltip("反向推进的效率乘数")]
        public float reverseMultiplier = 0.7f;

        [Tooltip("所有力的乘数。可用于保持力数值较小且更易读")]
        public float forceMultiplier = 100.0f;

        [Header("高级物理设置")]
        [Tooltip("推进器热量积累速率")]
        public float heatAccumulationRate = 0.1f;
        [Tooltip("推进器冷却速率")]
        public float coolingRate = 0.05f;
        [Tooltip("推进器过热阈值")]
        public float overheatThreshold = 100f;

        public float linearDamping = 0.05f;
        public float angularDamping = 2f;

        [Header("控制优化")]

        [Tooltip("角输入平滑度 - 值越高，输入变化越平滑")]
        [Range(0.01f, 0.5f)]
        public float angularInputSmoothing = 0.1f;

        [Tooltip("角加速度曲线 - 控制输入映射到实际力的方式")]
        public AnimationCurve angularResponseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // 新增内部变量
        private Vector3 targetAngularInput = Vector3.zero;
        private Vector3 currentAngularInput = Vector3.zero;
        private Vector3 previousAngularInput = Vector3.zero;

        [Header("飞船状态")]
        [ReadOnly] public float currentHeat = 0f;
        [ReadOnly] public bool isOverheated = false;
        [ReadOnly] public float fuelAmount = 100f;
        [ReadOnly] public float shieldStrength = 100f;

        // 系统状态
        public enum ShipSystemStatus { Operational, Damaged, Critical, Offline }
        [ReadOnly] public ShipSystemStatus propulsionStatus = ShipSystemStatus.Operational;
        [ReadOnly] public ShipSystemStatus navigationStatus = ShipSystemStatus.Operational;
        [ReadOnly] public ShipSystemStatus powerStatus = ShipSystemStatus.Operational;

        // 公开访问刚体
        public Rigidbody Rigidbody => rb;

        // 内部变量
        private Vector3 appliedLinearForce = Vector3.zero;
        private Vector3 appliedAngularForce = Vector3.zero;
        private Rigidbody rb;
        private float thrusterEfficiency = 1.0f;
        private bool isInWarp = false;
        private Vector3 warpDirection;
        private float warpSpeed;

        private void OnValidate()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError(name + ": ShipPhysics缺少Rigidbody组件！");
                enabled = false;
                return;
            }

            // 设置物理参数
            rb.useGravity = false; // 我们会自己处理重力
            rb.interpolation = RigidbodyInterpolation.Interpolate; // 平滑运动
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // 高速碰撞检测
            rb.angularDamping = angularDamping;
            rb.linearDamping = linearDamping;
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError(name + ": ShipPhysics缺少Rigidbody组件！");
                enabled = false;
                return;
            }

            // 设置物理参数
            rb.useGravity = false; // 我们会自己处理重力
            rb.interpolation = RigidbodyInterpolation.Interpolate; // 平滑运动
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // 高速碰撞检测
            rb.angularDamping = angularDamping;
            rb.linearDamping = linearDamping;
        }

        void Update()
        {
            // 处理热量系统
            ManageHeatSystem();

            // 更新推进器视觉效果
            UpdateThrusterVFX();
        }

        // 当飞船距离原点太远时，把整个场景（除了飞船）全部往反方向移动，让飞船始终处于世界中心附近。否则会出现浮点精度丢失，子物体出现抖动
        void FixedUpdate()
        {
            if (rb == null || isOverheated) return;

            if (isInWarp)
            {
                ApplyWarpMovement();
                return;
            }

            // 相对论修正：计算当前相对论质量
            float gamma = RelativityPhysics.CalculateLorentzFactor(rb.linearVelocity);
            float relativisticMass = rb.mass * gamma;

            // 应用主推力（考虑效率）
            Vector3 adjustedLinearForce = appliedLinearForce * thrusterEfficiency;

            // 反向推力应用乘数
            if (adjustedLinearForce.z < 0)
                adjustedLinearForce.z *= reverseMultiplier;

            rb.AddRelativeForce(adjustedLinearForce * forceMultiplier, ForceMode.Force);

            // 使用平滑处理过的角输入
            rb.AddRelativeTorque(currentAngularInput * forceMultiplier, ForceMode.Force);
            Vector3 desiredVelocity = transform.forward * rb.linearVelocity.magnitude;
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, Time.fixedDeltaTime * 2f);


            // 应用环境物理效果
            ApplyEnvironmentalPhysics();

            // 应用制动
            if (isBrake) ApplyBrake();
        }

        private void ApplyEnvironmentalPhysics()
        {
            // 来自重力源的引力
            var gravitySources = PhysicsManager.Instance.gravitySources;
            Vector3 totalGravity = Vector3.zero;

            foreach (var source in gravitySources)
            {
                totalGravity += source.CalculateGravity(transform.position, rb.mass);
            }

            rb.AddForce(totalGravity, ForceMode.Force);

            // 这里可以添加更多环境因素，比如磁场、辐射压力等
        }

        public void ApplyBrake()
        {
            if (rb == null) return;

            // --- 线性制动 ---
            Vector3 velocity = rb.linearVelocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            Vector3 brakeForce = Vector3.zero;

            float brakeStrength = forceMultiplier;

            // 判断每个方向是否需要刹车
            brakeForce.x = BrakeAxis(localVelocity.x, rb.mass, maxBrakeForce);
            brakeForce.y = BrakeAxis(localVelocity.y, rb.mass, maxBrakeForce);
            brakeForce.z = BrakeAxis(localVelocity.z, rb.mass, maxBrakeForce);

            // 应用刹车力
            rb.AddRelativeForce(brakeForce * brakeStrength, ForceMode.Force);

            // --- 角速度制动 ---
            Vector3 localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
            Vector3 angularBrake = Vector3.zero;

            angularBrake.x = BrakeAxis(localAngularVelocity.x, rb.mass, maxBrakeTorque);
            angularBrake.y = BrakeAxis(localAngularVelocity.y, rb.mass, maxBrakeTorque);
            angularBrake.z = BrakeAxis(localAngularVelocity.z, rb.mass, maxBrakeTorque);

            rb.AddRelativeTorque(angularBrake, ForceMode.Force);

            // --- 小于阈值时强制清零 ---
            const float stopThreshold = 0.2f;
            if (rb.linearVelocity.magnitude < stopThreshold)
                rb.linearVelocity = Vector3.zero;

            if (rb.angularVelocity.magnitude < stopThreshold)
                rb.angularVelocity = Vector3.zero;

            // --- VFX 激活 ---
            if (brakeVFX != null && !brakeVFX.isPlaying && rb.linearVelocity.magnitude > 1f)
                brakeVFX.Play();
        }

        // 帮助函数：单轴刹车力计算
        private float BrakeAxis(float velocity, float mass, float maxForce)
        {
            float sign = Mathf.Sign(velocity);
            float magnitude = Mathf.Abs(velocity);
            float force = magnitude * mass; // 线性惯性
            force = Mathf.Min(force, maxForce); // 限制最大刹车力
            return -sign * force;
        }



        public void SetPhysicsInput(Vector3 linearInput, Vector3 angularInput)
        {
            appliedLinearForce = MultiplyByComponent(linearInput, linearForce);

            // 保存上一帧的角输入
            previousAngularInput = targetAngularInput;

            // 设置目标角输入，并应用响应曲线
            targetAngularInput = new Vector3(
                angularResponseCurve.Evaluate(Mathf.Abs(angularInput.x)) * Mathf.Sign(angularInput.x),
                angularResponseCurve.Evaluate(Mathf.Abs(angularInput.y)) * Mathf.Sign(angularInput.y),
                angularResponseCurve.Evaluate(Mathf.Abs(angularInput.z)) * Mathf.Sign(angularInput.z)
            );

            // 使用线性内插平滑角输入变化
            currentAngularInput = Vector3.Lerp(currentAngularInput, MultiplyByComponent(targetAngularInput, angularForce), angularInputSmoothing);

            // 增加热量
            if (linearInput.magnitude > 0.1f || angularInput.magnitude > 0.1f)
            {
                currentHeat += heatAccumulationRate *
                               (linearInput.magnitude + angularInput.magnitude) * Time.deltaTime;
            }

            // 消耗燃料
            if (linearInput.magnitude > 0.1f)
            {
                fuelAmount -= linearInput.magnitude * 0.01f * Time.deltaTime;
                fuelAmount = Mathf.Max(0, fuelAmount);
            }
        }

        private void ManageHeatSystem()
        {
            // 自然冷却
            currentHeat = Mathf.Max(0, currentHeat - coolingRate * Time.deltaTime);

            // 检查过热状态
            if (currentHeat >= overheatThreshold && !isOverheated)
            {
                isOverheated = true;
                StartCoroutine(CooldownPeriod());
            }

            // 基于热量调整推进器效率
            thrusterEfficiency = Mathf.Lerp(1.0f, 0.6f, currentHeat / overheatThreshold);
        }

        private IEnumerator CooldownPeriod()
        {
            propulsionStatus = ShipSystemStatus.Critical;
            Debug.Log("推进系统过热，启动紧急冷却程序");

            // 等待热量降到阈值的一半
            yield return new WaitUntil(() => currentHeat <= overheatThreshold * 0.5f);

            isOverheated = false;
            propulsionStatus = ShipSystemStatus.Operational;
            Debug.Log("推进系统已冷却，恢复正常操作");
        }

        private void UpdateThrusterVFX()
        {
            // 主推进器效果
            if (mainThrusterVFX != null)
            {
                var emission = mainThrusterVFX.emission;
                if (appliedLinearForce.z > 0.1f && !isOverheated)
                {
                    if (!mainThrusterVFX.isPlaying)
                        mainThrusterVFX.Play();

                    // 调整粒子数量基于推力大小和效率
                    emission.rateOverTimeMultiplier = appliedLinearForce.z * thrusterEfficiency;
                }
                else if (mainThrusterVFX.isPlaying)
                {
                    mainThrusterVFX.Stop();
                }
            }

            // RCS推进器效果 - 需要根据实际运动方向激活相应的RCS
            if (rcsThrustersVFX != null && rcsThrustersVFX.Length > 0)
            {
                // 这里需要根据您的RCS布局来正确激活
                // 简化示例：
                bool anyRcsActive = false;
                if (Mathf.Abs(appliedAngularForce.magnitude) > 0.1f ||
                    Mathf.Abs(appliedLinearForce.x) > 0.1f ||
                    Mathf.Abs(appliedLinearForce.y) > 0.1f)
                {
                    anyRcsActive = true;
                }

                foreach (var rcs in rcsThrustersVFX)
                {
                    if (anyRcsActive && !isOverheated)
                    {
                        if (!rcs.isPlaying) rcs.Play();
                    }
                    else if (rcs.isPlaying)
                    {
                        rcs.Stop();
                    }
                }
            }
        }

        // 曲速/跃迁系统
        public void InitiateWarp(Vector3 direction, float speed, float duration)
        {
            if (isInWarp || isOverheated) return;

            StartCoroutine(WarpSequence(direction, speed, duration));
        }

        private IEnumerator WarpSequence(Vector3 direction, float speed, float duration)
        {
            // 预热
            Debug.Log("曲速引擎预热中...");
            yield return new WaitForSeconds(2.0f);

            // 激活曲速
            isInWarp = true;
            warpDirection = direction.normalized;
            warpSpeed = speed;

            // 播放曲速效果
            // 这里可以添加屏幕扭曲、粒子效果等

            Debug.Log("进入曲速！");

            // 持续曲速移动
            yield return new WaitForSeconds(duration);

            // 退出曲速
            isInWarp = false;
            Debug.Log("曲速结束，恢复正常航行");

            // 曲速后的热量增加和短暂冷却期
            currentHeat += duration * 10;
            thrusterEfficiency *= 0.7f;

            yield return new WaitForSeconds(1.0f);
            thrusterEfficiency = 1.0f;
        }

        private void ApplyWarpMovement()
        {
            // 在曲速中，直接移动飞船，忽略正常物理
            transform.position += warpDirection * warpSpeed * Time.fixedDeltaTime;
        }

        // 受到伤害和修复系统
        public void TakeDamage(float amount, Vector3 impactPoint)
        {
            // 先影响护盾
            if (shieldStrength > 0)
            {
                float absorbedDamage = Mathf.Min(shieldStrength, amount);
                shieldStrength -= absorbedDamage;
                amount -= absorbedDamage;
            }

            // 如果还有剩余伤害，随机影响一个系统
            if (amount > 0)
            {
                int randomSystem = Random.Range(0, 3);

                switch (randomSystem)
                {
                    case 0:
                        DamagePropulsionSystem(amount);
                        break;
                    case 1:
                        DamageNavigationSystem(amount);
                        break;
                    case 2:
                        DamagePowerSystem(amount);
                        break;
                }
            }
        }

        private void DamagePropulsionSystem(float amount)
        {
            // 降低推进器效率
            thrusterEfficiency = Mathf.Max(0.3f, thrusterEfficiency - amount * 0.01f);

            // 更新系统状态
            if (thrusterEfficiency < 0.4f)
                propulsionStatus = ShipSystemStatus.Critical;
            else if (thrusterEfficiency < 0.7f)
                propulsionStatus = ShipSystemStatus.Damaged;

            Debug.Log($"推进系统受损！当前效率: {thrusterEfficiency:P0}");
        }

        private void DamageNavigationSystem(float amount)
        {
            // 影响转向能力
            angularForce *= (1 - Mathf.Min(0.5f, amount * 0.01f));

            // 更新状态
            if (angularForce.magnitude < linearForce.magnitude * 0.4f)
                navigationStatus = ShipSystemStatus.Critical;
            else if (angularForce.magnitude < linearForce.magnitude * 0.7f)
                navigationStatus = ShipSystemStatus.Damaged;

            Debug.Log("导航系统受损！转向能力降低");
        }

        private void DamagePowerSystem(float amount)
        {
            // 影响整体性能，包括热量管理
            coolingRate *= (1 - Mathf.Min(0.5f, amount * 0.01f));

            // 更新状态
            if (coolingRate < 0.025f)
                powerStatus = ShipSystemStatus.Critical;
            else if (coolingRate < 0.04f)
                powerStatus = ShipSystemStatus.Damaged;

            Debug.Log("能源系统受损！冷却效率降低");
        }

        public void RepairSystem(string system, float repairAmount)
        {
            switch (system.ToLower())
            {
                case "propulsion":
                    thrusterEfficiency = Mathf.Min(1.0f, thrusterEfficiency + repairAmount);
                    UpdateSystemStatus();
                    Debug.Log($"推进系统已修复，当前效率: {thrusterEfficiency:P0}");
                    break;

                case "navigation":
                    angularForce += Vector3.one * repairAmount * 10;
                    UpdateSystemStatus();
                    Debug.Log("导航系统已修复，转向能力提升");
                    break;

                case "power":
                    coolingRate = Mathf.Min(0.05f, coolingRate + repairAmount * 0.01f);
                    UpdateSystemStatus();
                    Debug.Log("能源系统已修复，冷却效率提升");
                    break;
            }
        }

        private void UpdateSystemStatus()
        {
            // 更新推进系统状态
            if (thrusterEfficiency > 0.9f)
                propulsionStatus = ShipSystemStatus.Operational;
            else if (thrusterEfficiency > 0.6f)
                propulsionStatus = ShipSystemStatus.Damaged;
            else if (thrusterEfficiency > 0.3f)
                propulsionStatus = ShipSystemStatus.Critical;
            else
                propulsionStatus = ShipSystemStatus.Offline;

            // 更新导航系统状态
            float navRatio = angularForce.magnitude / (linearForce.magnitude + 0.01f);
            if (navRatio > 0.9f)
                navigationStatus = ShipSystemStatus.Operational;
            else if (navRatio > 0.6f)
                navigationStatus = ShipSystemStatus.Damaged;
            else if (navRatio > 0.3f)
                navigationStatus = ShipSystemStatus.Critical;
            else
                navigationStatus = ShipSystemStatus.Offline;

            // 更新能源系统状态
            if (coolingRate > 0.045f)
                powerStatus = ShipSystemStatus.Operational;
            else if (coolingRate > 0.035f)
                powerStatus = ShipSystemStatus.Damaged;
            else if (coolingRate > 0.02f)
                powerStatus = ShipSystemStatus.Critical;
            else
                powerStatus = ShipSystemStatus.Offline;
        }

        private Vector3 MultiplyByComponent(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        // 用于调试显示
        void OnGUI()
        {
            if (!Debug.isDebugBuild) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"速度: {rb.linearVelocity.magnitude:F1} m/s");
            GUILayout.Label($"推进器热量: {currentHeat:F1}/{overheatThreshold}");
            GUILayout.Label($"推进器效率: {thrusterEfficiency:P0}");
            GUILayout.Label($"燃料: {fuelAmount:F1}%");
            GUILayout.Label($"护盾: {shieldStrength:F1}%");
            GUILayout.Label($"推进系统: {propulsionStatus}");
            GUILayout.Label($"导航系统: {navigationStatus}");
            GUILayout.Label($"能源系统: {powerStatus}");
            GUILayout.EndArea();
        }
    }
}