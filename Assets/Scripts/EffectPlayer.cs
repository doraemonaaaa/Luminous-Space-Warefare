using UnityEngine;
using UnityEngine.VFX;

[System.Serializable]
public class EffectWrapper
{
    public ParticleSystem particleEffect;
    public VisualEffect visualEffect;

}

public static class EffectPlayer
{
    public static void Play<T>(T effect)
    {
        if (effect == null) return;

        if (effect is ParticleSystem ps)
        {
            ps.Play();
        }
        else if (effect is VisualEffect vfx)
        {
            vfx.Play();
        }
        else if (effect is EffectWrapper wrapper)
        {
            // 自动播放内部的特效
            if (wrapper.particleEffect != null)
                wrapper.particleEffect.Play();
            if (wrapper.visualEffect != null)
                wrapper.visualEffect.Play();
        }
        else
        {
            Debug.LogWarning("Unsupported effect type: " + typeof(T));
        }
    }
}
