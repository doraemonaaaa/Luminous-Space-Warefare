using UnityEngine;

namespace MyPhysics
{
    public class TimeWarpZone : MonoBehaviour
    {
        // 扭曲区内的时间比例，例如减速到 0.5 倍
        public float warpTimeScale = 0.5f;

        // 用于判断是否已经触发改变（避免重复设置）
        private bool isWarped = false;

        private void OnTriggerEnter(Collider other)
        {
            // 如果不是对全局设置，也可以在这里判断只有某个标签的对象受影响
            if (!isWarped)
            {
                Time.timeScale = warpTimeScale;
                isWarped = true;
                // 可选：调整 FixedDeltaTime 保证物理更新同步
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // 恢复到正常时间比例
            Time.timeScale = 1.0f;
            isWarped = false;
            Time.fixedDeltaTime = 0.02f;
        }
    }
}