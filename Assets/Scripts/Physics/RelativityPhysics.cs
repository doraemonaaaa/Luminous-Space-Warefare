using UnityEngine;

namespace MyPhysics
{
    public static class RelativityPhysics
    {
        /// <summary>
        /// 计算当前速度下的洛伦兹因子 gamma。
        /// </summary>
        public static float CalculateLorentzFactor(Vector3 velocity)
        {
            float v = velocity.magnitude;
            float ratio = v / UniverseParameters.C;
            if (ratio >= 1f) ratio = 0.999999f; // 防止除以零或虚数
            return 1f / Mathf.Sqrt(1f - ratio * ratio);
        }

        /// <summary>
        /// 计算相对论质量
        /// </summary>
        public static float GetRelativisticMass(float restMass, Vector3 velocity)
        {
            return restMass * CalculateLorentzFactor(velocity);
        }
    }
}
