using UnityEngine;

namespace MyPhysics
{
    public class GravityZone : MonoBehaviour
    {
        // 自定义的重力向量，例如双倍重力向下
        public Vector3 customGravity = new Vector3(0, -19.62f, 0);

        // 在Zone内的刚体对象将使用自定义重力
        private void OnTriggerStay(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                // 可以使用 ForceMode.Acceleration 确保重力不受质量影响
                rb.AddForce(customGravity, ForceMode.Acceleration);
            }
        }
    }

}