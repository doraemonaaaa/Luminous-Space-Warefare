using UnityEngine;
using System.Collections.Generic;
using MyPhysics.Core;

namespace MyPhysics
{
    [ExecuteInEditMode]
    public class OrbitDebugDisplay : MonoBehaviour
    {
        public Color orbitColor = Color.cyan;
        public int orbitSteps = 500;
        public float orbitTimeStep = 0.1f;
        public bool showOrbitInGizmos = true;

        private void OnDrawGizmos()
        {
            if (!showOrbitInGizmos)
                return;

            // 支持 CelestialBody_RigidBody
            var rigidBodies = Object.FindObjectsByType<CelestialBody_RigidBody>(FindObjectsSortMode.None);
            foreach (var body in rigidBodies)
            {
                DrawOrbit(body, body.transform.position, body.rb.linearVelocity, body.rb.mass);
            }

            // 支持 CelestialBody_Transform
            var transformBodies = Object.FindObjectsByType<CelestialBody_Transform>(FindObjectsSortMode.None);
            foreach (var body in transformBodies)
            {
                var velocity = body.GetVelocity(); // 提供一个公有方法或属性获取速度
                DrawOrbit(body, body.transform.position, velocity, body.mass);
            }
        }

        private void DrawOrbit(Component body, Vector3 position, Vector3 velocity, float mass)
        {
            List<Vector3> orbitPoints = SimulateOrbit(position, velocity, mass, body.gameObject);
            Gizmos.color = orbitColor;
            for (int i = 1; i < orbitPoints.Count; i++)
            {
                Gizmos.DrawLine(orbitPoints[i - 1], orbitPoints[i]);
            }
        }

        public List<Vector3> SimulateOrbit(Vector3 startPosition, Vector3 startVelocity, float mass, GameObject selfObject)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 currentPos = startPosition;
            Vector3 currentVel = startVelocity;
            points.Add(currentPos);

            for (int i = 0; i < orbitSteps; i++)
            {
                Vector3 totalForce = Vector3.zero;
                GravitySource[] sources = FindObjectsByType<GravitySource>(FindObjectsSortMode.None);

                foreach (var source in sources)
                {
                    if (source.gameObject == selfObject)
                        continue;

                    Vector3 force = source.CalculateGravity(currentPos, mass);
                    totalForce += force;
                }

                Vector3 acceleration = totalForce / mass;
                currentVel += acceleration * orbitTimeStep;
                currentPos += currentVel * orbitTimeStep;
                points.Add(currentPos);
            }

            return points;
        }
    }
}
