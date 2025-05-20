using MyPhysics;
using System.Collections.Generic;
using UnityEngine;

namespace MyOptimize
{
    public class EndlessManager : MonoBehaviour
    {
        public float distanceThreshold = 1000f;
        private List<Transform> physicsObjects = new List<Transform>();
        private Camera playerCamera;
        private Vector3 previousOffset = Vector3.zero; // 记录上一次的偏移量

        public event System.Action PostFloatingOriginUpdate;

        void Awake()
        {
            // 获取 player 和 physicsObjects 相关的组件
            var ships = Object.FindObjectsByType<Ship>(FindObjectsSortMode.None);
            playerCamera = Camera.main;

            foreach (var ship in ships)
            {
                if (ship != null)
                    physicsObjects.Add(ship.transform);
            }

            // 获取 CelestialBody_RigidBody 类型的所有对象并添加到物理对象列表中
            var bodies = Object.FindObjectsByType<CelestialBody_RigidBody>(FindObjectsSortMode.None);
            foreach (var body in bodies)
            {
                physicsObjects.Add(body.transform);
            }

            // 获取 CelestialBody_Transform 类型的所有对象并添加到物理对象列表中
            var celestialTransforms = Object.FindObjectsByType<CelestialBody_Transform>(FindObjectsSortMode.None);
            foreach (var celestial in celestialTransforms)
            {
                physicsObjects.Add(celestial.transform);
            }

            // 添加场景中其他重要对象
            AddOtherImportantObjects();
        }

        void AddOtherImportantObjects()
        {
            // 添加可能被遗漏的其他重要物体
            // 例如：粒子效果、子弹、特效等
            var particles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var particle in particles)
            {
                if (!physicsObjects.Contains(particle.transform))
                    physicsObjects.Add(particle.transform);
            }

            // 添加所有带有Rigidbody的对象
            var rigidbodies = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            foreach (var rb in rigidbodies)
            {
                if (!physicsObjects.Contains(rb.transform))
                    physicsObjects.Add(rb.transform);
            }
        }

        void LateUpdate()
        {
            UpdateFloatingOrigin();

            // 确保在事件不为空时触发
            PostFloatingOriginUpdate?.Invoke();
        }

        void UpdateFloatingOrigin()
        {
            if (playerCamera == null)
                return;

            Vector3 cameraPosition = playerCamera.transform.position;
            float distanceFromOrigin = cameraPosition.magnitude;

            // 只有当相机位置超出阈值时才更新
            if (distanceFromOrigin > distanceThreshold)
            {
                Debug.Log($"浮动原点更新：距离 {distanceFromOrigin} 超过阈值 {distanceThreshold}");

                // 计算需要偏移的量
                Vector3 offsetToApply = cameraPosition;

                // 对所有物体应用偏移
                foreach (Transform t in physicsObjects)
                {
                    if (t != null) // 防止空引用
                    {
                        t.position -= offsetToApply;
                    }
                }

                // 保存这次应用的偏移量
                previousOffset = offsetToApply;

                // 输出日志以便调试
                Debug.Log($"已将所有物体位置偏移 {offsetToApply}");
            }
        }

        // 添加一个方法用于在游戏运行时注册新物体
        public void RegisterPhysicsObject(Transform obj)
        {
            if (obj != null && !physicsObjects.Contains(obj))
            {
                physicsObjects.Add(obj);
            }
        }
    }
}