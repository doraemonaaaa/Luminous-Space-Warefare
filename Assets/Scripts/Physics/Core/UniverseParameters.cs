using UnityEngine;

namespace MyPhysics
{
    /// <summary>
    /// 存储宇宙参数，例如万有引力常数（G）。
    /// </summary>
    public static class UniverseParameters
    {
        // 万有引力常数 G，通常为6.67430e-11（单位：m^3 kg^-1 s^-2）
        //public const float G = 6.67430e-11f; // 单位：m^3 kg^-1 s^-2
        public const float G = 10f;  // 适应游戏中的尺度

        // 光速常量 (m/s)
        public const float C = 299792458f;
    }
}
