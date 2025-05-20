using UnityEngine;


[RequireComponent(typeof(Collider))]
public class WaypointDetector : MonoBehaviour
{
    private Collider reachDetector;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        reachDetector = GetComponent<Collider>();
        reachDetector.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out RacePlayer racer))
        {
            racer.OnReachWaypoint(this.transform);
        }
    }
}
