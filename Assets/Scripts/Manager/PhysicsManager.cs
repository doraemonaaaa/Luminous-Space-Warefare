using MyPhysics;
using MyPhysics.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class PhysicsManager : SingletonMono<PhysicsManager>
{
    protected override bool isDontDestroyOnLoad => false;

    public HashSet<GravitySource> gravitySources = new HashSet<GravitySource>();
    public HashSet<GravitySensitiveObject> gravitySensitiveObjects = new HashSet<GravitySensitiveObject>();

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ApplySourceGravity();
    }

    private void FixedUpdate()
    {
        ApplyGravityToRigidBody();
    }

    public void OnGravitySourceAdded(GravitySource gs)
    {
        gravitySources.Add(gs);
    }

    public void OnGravitySourceDestroyed(GravitySource gs)
    {
        gravitySources.Remove(gs);
    }

    public void OnGravitySensitiveObjectAdded(GravitySensitiveObject gso)
    {
        gravitySensitiveObjects.Add(gso);
    }

    public void OnGravitySensitiveObjectDestroyed(GravitySensitiveObject gso)
    {
        gravitySensitiveObjects.Remove(gso);
    }

    /// <summary>
    /// 计算所有天体之间的引力作用, Transform方法
    public void ApplySourceGravity()
    {
        foreach (var A in gravitySources)
        {
            if (A == null) continue;

            Vector3 totalForce = Vector3.zero;

            foreach (var source in gravitySources)
            {
                if (source == null || source.gameObject == A.gameObject) continue;

                Vector3 force = source.CalculateGravity(A.transform.position, A.mass);
                totalForce += force;
            }

            Vector3 acceleration = totalForce / A.mass;
            A.Velocity += acceleration * Time.fixedDeltaTime;
        }
    }

    public void ApplyGravityToRigidBody()
    {
        foreach(GravitySensitiveObject A in gravitySensitiveObjects)
        {
            Vector3 totalForce = Vector3.zero;

            foreach (var source in gravitySources)
            {
                Vector3 force = source.CalculateGravity(A.transform.position, A.target_rb.mass);
                totalForce += force;
            }

            // 将总引力施加到刚体上
            A.target_rb.AddForce(totalForce);
        }
    }

}
