using UnityEngine;

namespace MyPhysics.Core
{
    public enum GravityMode
    {
        ConstantAcceleration,  // 固定加速度模式（如地球表面重力）
        MassBased      // 万有引力公式模式
    }


    /// <summary>
    /// GravitySource 为所有可参与引力交互的对象提供统一接口。
    /// 所有继承此类的具体对象必须实现 CalculateGravity 方法，用于根据物体位置和质量计算作用力。
    /// </summary>
    public abstract class GravitySource : MonoBehaviour
    {
        [Tooltip("引力模式：Acceleration 为恒定加速度，MassBased 为万有引力计算")]
        public GravityMode gravityMode = GravityMode.MassBased;

        [Tooltip("引力源的质量，仅在 MassBased 模式下生效")]
        public float mass = 100f;

        [Tooltip("表面重力加速度，仅在 Acceleration 模式下生效")]
        public float surfaceGravitationalAcceleration = 9.81f;

        [Tooltip("影响范围，在此范围内物体才会受到引力")]
        public float influenceRadius = 20f;
        public Vector3 Velocity
        {
            get { return _currentVelocity; }
            set { _currentVelocity = value; }
        }
        protected Vector3 _currentVelocity;

        protected virtual void Start()
        {
            if (PhysicsManager.Instance != null)
                PhysicsManager.Instance.OnGravitySourceAdded(this);
        }

        protected virtual void OnDestroy()
        {
            if (PhysicsManager.Instance != null)
                PhysicsManager.Instance.OnGravitySourceDestroyed(this);
        }

        protected virtual void OnEnable()
        {
            if (PhysicsManager.Instance != null)
                PhysicsManager.Instance.OnGravitySourceAdded(this);
        }

        protected virtual void OnDisable()
        {
            if(PhysicsManager.Instance != null)
                PhysicsManager.Instance.OnGravitySourceDestroyed(this);
        }

        /// <summary>
        /// 根据传入物体的位置和质量，计算该引力源对该物体施加的引力。
        /// </summary>
        /// <param name="objectPosition">物体当前的位置</param>
        /// <param name="objectMass">物体质量</param>
        /// <returns>返回施加在物体上的引力向量</returns>
        public abstract Vector3 CalculateGravity(Vector3 objectPosition, float objectMass);

        public virtual void ApplyGravity(GravitySource A, GravitySource B) { }

        /// <summary>
        /// 绘制影响区域，便于在编辑器中预览效果
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, influenceRadius);
        }
    }
}
