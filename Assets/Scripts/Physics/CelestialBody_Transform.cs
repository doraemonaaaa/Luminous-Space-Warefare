using UnityEngine;
using MyPhysics.Core;

namespace MyPhysics
{
    public class CelestialBody_Transform : UniversalGravitation
    {
        public Vector3 originSpeed;

        protected void OnValidate()
        {
            _currentVelocity = originSpeed;
        }

        private void Awake()
        {
            _currentVelocity = originSpeed;
        }

        private void FixedUpdate()
        {
            UpdatePosition();
        }

        public Vector3 GetVelocity()
        {
            return _currentVelocity;
        }
        private void UpdatePosition()
        {
            // x = x0 + vt
            transform.position += _currentVelocity * Time.fixedDeltaTime;
        }
    }
}
