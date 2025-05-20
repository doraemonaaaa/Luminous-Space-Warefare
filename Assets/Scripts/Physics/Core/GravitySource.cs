using UnityEngine;

namespace MyPhysics.Core
{
    public enum GravityMode
    {
        ConstantAcceleration,  // �̶����ٶ�ģʽ����������������
        MassBased      // ����������ʽģʽ
    }


    /// <summary>
    /// GravitySource Ϊ���пɲ������������Ķ����ṩͳһ�ӿڡ�
    /// ���м̳д���ľ���������ʵ�� CalculateGravity ���������ڸ�������λ�ú�����������������
    /// </summary>
    public abstract class GravitySource : MonoBehaviour
    {
        [Tooltip("����ģʽ��Acceleration Ϊ�㶨���ٶȣ�MassBased Ϊ������������")]
        public GravityMode gravityMode = GravityMode.MassBased;

        [Tooltip("����Դ������������ MassBased ģʽ����Ч")]
        public float mass = 100f;

        [Tooltip("�����������ٶȣ����� Acceleration ģʽ����Ч")]
        public float surfaceGravitationalAcceleration = 9.81f;

        [Tooltip("Ӱ�췶Χ���ڴ˷�Χ������Ż��ܵ�����")]
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
        /// ���ݴ��������λ�ú����������������Դ�Ը�����ʩ�ӵ�������
        /// </summary>
        /// <param name="objectPosition">���嵱ǰ��λ��</param>
        /// <param name="objectMass">��������</param>
        /// <returns>����ʩ���������ϵ���������</returns>
        public abstract Vector3 CalculateGravity(Vector3 objectPosition, float objectMass);

        public virtual void ApplyGravity(GravitySource A, GravitySource B) { }

        /// <summary>
        /// ����Ӱ�����򣬱����ڱ༭����Ԥ��Ч��
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, influenceRadius);
        }
    }
}
