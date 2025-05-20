using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class ShipSkills : MonoBehaviour
{
    // 自定义 UnityEvent 类型，支持 float, VisualEffect, ParticleSystem, float
    [System.Serializable]
    public class ShipSkillEvent : UnityEvent<float, EffectWrapper, float, Transform> { }
    public void SpaceJump(float delay_time, EffectWrapper effect, float jump_distance, Transform tf)
    {
        EffectPlayer.Play(effect);

        // 执行跳跃
        tf.position += tf.forward * jump_distance;
        Debug.Log("Space Jump");
    }

}
