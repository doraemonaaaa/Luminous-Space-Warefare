using UnityEngine;

namespace MyPhysics
{
    public class GravityZone : MonoBehaviour
    {
        // �Զ������������������˫����������
        public Vector3 customGravity = new Vector3(0, -19.62f, 0);

        // ��Zone�ڵĸ������ʹ���Զ�������
        private void OnTriggerStay(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                // ����ʹ�� ForceMode.Acceleration ȷ��������������Ӱ��
                rb.AddForce(customGravity, ForceMode.Acceleration);
            }
        }
    }

}