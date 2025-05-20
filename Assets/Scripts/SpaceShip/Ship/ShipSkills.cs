using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class ShipSkills : MonoBehaviour
{
    // �Զ��� UnityEvent ���ͣ�֧�� float, VisualEffect, ParticleSystem, float
    [System.Serializable]
    public class ShipSkillEvent : UnityEvent<float, EffectWrapper, float, Transform> { }
    public void SpaceJump(float delay_time, EffectWrapper effect, float jump_distance, Transform tf)
    {
        EffectPlayer.Play(effect);

        // ִ����Ծ
        tf.position += tf.forward * jump_distance;
        Debug.Log("Space Jump");
    }

}
