using UnityEngine;
using MyPhysics.Core;

namespace MyPhysics
{
    [RequireComponent(typeof(Rigidbody))]
    public class CelestialBody_RigidBody : UniversalGravitation
    {
        private Rigidbody _rb;
        public Vector3 originSpeed;

        // 在编辑器中实时同步质量
        protected void OnValidate()
        {
            rb.mass = mass;
            rb.useGravity = false;
            rb.linearVelocity = originSpeed;
        }

        private void Awake()
        {
            rb.mass = mass;
            rb.useGravity = false;
            rb.linearVelocity = originSpeed;
        }

        private void FixedUpdate()
        {
            // 更新引力作用
            ApplyGravity();
        }

        // 计算并应用来自所有引力源的总引力
        private void ApplyGravity()
        {
            Vector3 totalForce = Vector3.zero;

            // 查找所有引力源
            var sources = PhysicsManager.Instance.gravitySources;

            foreach (var source in sources)
            {
                if (source.gameObject == gameObject) continue;

                Vector3 force = source.CalculateGravity(transform.position, rb.mass);
                totalForce += force;
            }

            // 将总引力施加到刚体上
            rb.AddForce(totalForce);
        }

        public Rigidbody rb
        {
            get
            {
                if (!_rb)
                {
                    _rb = GetComponent<Rigidbody>();
                }
                return _rb;
            }
        }
    }
}
