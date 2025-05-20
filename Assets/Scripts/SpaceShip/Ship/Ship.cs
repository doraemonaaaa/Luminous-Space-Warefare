using UnityEngine;
using MyPhysics;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ShipPhysics))]
[RequireComponent(typeof(ShipAttributes))]
[RequireComponent(typeof(Collider))]
public class Ship : MonoBehaviour
{
    protected ShipPhysics physics;
    protected Collider shipCollider;
    protected ShipAttributes attributes;

    public virtual Rigidbody Rigidbody => physics.Rigidbody;
    public virtual ShipAttributes Attributes => attributes;

    [Header("碰撞参数")]
    public GameObject collisionEffectPrefab;
    public AudioClip collisionSound;
    public float damageThreshold = 5f;
    public float damageMultiplier = 10f;

    protected virtual void Awake()
    {
        physics = GetComponent<ShipPhysics>();
        attributes = GetComponent<ShipAttributes>();
        shipCollider = GetComponent<Collider>();
        shipCollider.isTrigger = true;
    }

    protected virtual void Update()
    {
        
    }

    public void SetBrake(bool isBrake)
    {
        physics.isBrake = isBrake;
    }

    public void Drive(Vector3 linearInput, Vector3 angularInput)
    {
        physics.SetPhysicsInput(linearInput, angularInput);
    }


    protected virtual void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < damageThreshold) return;

        float damage = (impactForce - damageThreshold) * damageMultiplier;
        attributes.TakeDamage(damage);

        if (collisionEffectPrefab != null)
            Instantiate(collisionEffectPrefab, collision.contacts[0].point, Quaternion.identity);
        if (collisionSound != null)
            AudioSource.PlayClipAtPoint(collisionSound, transform.position);
    }
}
