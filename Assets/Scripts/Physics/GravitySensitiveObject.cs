using UnityEngine;

public class GravitySensitiveObject : MonoBehaviour
{
    public GameObject targetObject;
    public Rigidbody target_rb;

    private void Awake()
    {
        if (target_rb == null)
        {
            target_rb = targetObject.GetComponent<Rigidbody>();
            if (target_rb == null)
            {
                Debug.LogError("GravitySensitiveObject’“≤ªµΩRigidbody£¨«ÎºÏ≤È£°", this);
            }
        }
    }

    void Start()
    {
        PhysicsManager.Instance.OnGravitySensitiveObjectAdded(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if(PhysicsManager.Instance != null)
            PhysicsManager.Instance.OnGravitySensitiveObjectDestroyed(this);
    }

    private void OnEnable()
    {
        if (PhysicsManager.Instance != null)
            PhysicsManager.Instance.OnGravitySensitiveObjectAdded(this);
    }

    private void OnDisable()
    {
        if (PhysicsManager.Instance != null)
            PhysicsManager.Instance.OnGravitySensitiveObjectDestroyed(this);
    }
}
