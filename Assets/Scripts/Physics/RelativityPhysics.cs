using UnityEngine;

namespace MyPhysics
{
    public static class RelativityPhysics
    {
        /// <summary>
        /// ���㵱ǰ�ٶ��µ����������� gamma��
        /// </summary>
        public static float CalculateLorentzFactor(Vector3 velocity)
        {
            float v = velocity.magnitude;
            float ratio = v / UniverseParameters.C;
            if (ratio >= 1f) ratio = 0.999999f; // ��ֹ�����������
            return 1f / Mathf.Sqrt(1f - ratio * ratio);
        }

        /// <summary>
        /// �������������
        /// </summary>
        public static float GetRelativisticMass(float restMass, Vector3 velocity)
        {
            return restMass * CalculateLorentzFactor(velocity);
        }
    }
}
