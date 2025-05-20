using UnityEngine;

namespace MyPhysics
{
    public class TimeWarpZone : MonoBehaviour
    {
        // Ť�����ڵ�ʱ�������������ٵ� 0.5 ��
        public float warpTimeScale = 0.5f;

        // �����ж��Ƿ��Ѿ������ı䣨�����ظ����ã�
        private bool isWarped = false;

        private void OnTriggerEnter(Collider other)
        {
            // ������Ƕ�ȫ�����ã�Ҳ�����������ж�ֻ��ĳ����ǩ�Ķ�����Ӱ��
            if (!isWarped)
            {
                Time.timeScale = warpTimeScale;
                isWarped = true;
                // ��ѡ������ FixedDeltaTime ��֤�������ͬ��
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // �ָ�������ʱ�����
            Time.timeScale = 1.0f;
            isWarped = false;
            Time.fixedDeltaTime = 0.02f;
        }
    }
}