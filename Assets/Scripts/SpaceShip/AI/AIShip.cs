using UnityEngine;
using System.Collections.Generic;

public class AIShip : Ship
{
    [Header("路径点导航")]
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    public float waypointReachDistance = 10f;

    [Header("避障设置")]
    public float raycastDistance = 20f;
    public LayerMask obstacleLayer;
    public float avoidanceStrength = 1f;

    [Header("攻击设置")]
    public float attackRange = 60f;
    public float attackCooldown = 2f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    private float lastAttackTime = -999f;

    [Header("漂移模拟")]
    public float driftFactor = 0.3f;

    [Header("加速设置")]
    public float boostMultiplier = 2f;
    private bool isBoosting = false;

    protected override void Update()
    {
        base.Update();

        if (waypoints == null || waypoints.Count == 0) return;

        // 1. 导航到当前路径点
        Vector3 targetPos = waypoints[currentWaypointIndex].position;
        Vector3 toTarget = targetPos - transform.position;
        float distance = toTarget.magnitude;

        if (distance < waypointReachDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }

        Vector3 desiredDirection = toTarget.normalized;

        // 2. 避障方向调整
        desiredDirection = AvoidObstacle(desiredDirection);

        // 3. 驱动飞船移动
        Vector3 localDirection = transform.InverseTransformDirection(desiredDirection);
        Vector3 linear = new Vector3(0, 0, 1f);
        Vector3 angular = new Vector3(0, localDirection.x, 0f);

        Drive(linear, angular);

        // 4. 漂移模拟
        SimulateDrift();

        // 5. 攻击
        TryAttack();
    }

    private Vector3 AvoidObstacle(Vector3 forward)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, raycastDistance, obstacleLayer))
        {
            Vector3 avoidDir = Vector3.Cross(hit.normal, Vector3.up).normalized;
            return (forward + avoidDir * avoidanceStrength).normalized;
        }
        return forward;
    }

    private void SimulateDrift()
    {
        if (Rigidbody == null) return;

        Vector3 forwardVel = transform.forward * Vector3.Dot(Rigidbody.linearVelocity, transform.forward);
        Vector3 sideVel = transform.right * Vector3.Dot(Rigidbody.linearVelocity, transform.right) * driftFactor;
        Rigidbody.linearVelocity = forwardVel + sideVel;
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown || projectilePrefab == null || firePoint == null)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        Transform target = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            Ship ship = hit.GetComponent<Ship>();
            if (ship != null)
            {
                float d = Vector3.Distance(transform.position, hit.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    target = hit.transform;
                }
            }
        }

        if (target != null)
        {
            Vector3 dir = (target.position - firePoint.position).normalized;
            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));
            lastAttackTime = Time.time;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("BoostZone"))
    //    {
    //        isBoosting = true;
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("BoostZone"))
    //    {
    //        isBoosting = false;
    //    }
    //}
}
