using UnityEngine;

public class ShipAttributes : MonoBehaviour
{
    public float maxHP = 100f;
    public float HP = 100f;
    public float HPRatio => Mathf.Clamp01(HP / maxHP) * 100;

    public float maxThrusterCapacity = 100f;
    public float thrusterCapacity = 100f;
    public float ThrusterCapacityRatio => Mathf.Clamp01(thrusterCapacity / maxThrusterCapacity) * 100;

    public float maxEnergy = 100f;
    public float energy = 100f;
    public float EnergyRatio => Mathf.Clamp01(energy / maxEnergy) * 100;

    public void TakeDamage(float amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            HP = 0;
            Debug.Log($"{gameObject.name} Destroyed.");
        }
    }
}
