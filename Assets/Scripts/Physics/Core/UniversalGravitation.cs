using MyPhysics.Core;
using UnityEngine;

namespace MyPhysics
{
    public class UniversalGravitation : GravitySource
    {
        private float minDistance = 1f; // 防止距离过小导致引力过大

        /// <summary>
        /// 计算重力F
        /// </summary>
        /// <param name="objectPosition"></param>
        /// <param name="objectMass"></param>
        /// <returns></returns>
        public override Vector3 CalculateGravity(Vector3 objectPosition, float objectMass)
        {
            Vector3 direction = transform.position - objectPosition;
            float distance = direction.magnitude;

            if (distance > influenceRadius)
                return Vector3.zero;

            if (gravityMode == GravityMode.ConstantAcceleration)
            {
                // 固定加速度模式：a = surfaceGravitationalAcceleration
                return direction.normalized * surfaceGravitationalAcceleration * objectMass;
            }
            else
            {
                // 万有引力模式：a = G * M / r²
                float clampedDistance = Mathf.Max(distance, minDistance); // 防止除零或过大
                float forceMagnitude = UniverseParameters.G * mass * objectMass / (clampedDistance * clampedDistance);
                return direction.normalized * forceMagnitude;
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, influenceRadius);
        }
    }
}
