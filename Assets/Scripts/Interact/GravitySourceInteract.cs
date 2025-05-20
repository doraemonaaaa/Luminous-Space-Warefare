using System.Collections.Generic;
using UnityEngine;

public class GravitySourceInteract : InteractableObject
{
    [Header("Gravity Settings")]
    public Vector2 orbitRadiusRange = new Vector2(1.1f, 1.4f);
    public Color orbitColor = Color.red;
    public float orbitLineWidth = 10f;
    public int ringCount = 16;
    public int lineSegmentCount = 100;
    public float slingshotForce = 50f;

    private GameObject orbitVisualizationOut;
    private GameObject orbitVisualizationIn;

    private Dictionary<GameObject, Rigidbody> interactorsRigidbodies = new Dictionary<GameObject, Rigidbody>();


    protected override void Awake()
    {
        base.Awake();

        if (!(interactTrigger is SphereCollider))
        {
            Debug.LogError($"GravitySourceInteract Error: ��Ҫʹ�� SphereCollider ��Ϊ������������");
            return;
        }
        SphereCollider sphere = interactTrigger as SphereCollider;
        interactTrigger.isTrigger = true;
        sphere.radius = orbitRadiusRange.y;  // radius�ǻ��� local scale�� multiple

        // ���ý�����Χ
        interactionRange = transform.localScale.x * interactionRange;
    }

    private void FixedUpdate()
    {
        if(interactorsRigidbodies.Count > 0)
        {
            foreach (var obj_rb in interactorsRigidbodies)
            {
                Vector3 forward = obj_rb.Key.transform.forward.normalized;
                obj_rb.Value.AddForce(forward * slingshotForce, ForceMode.VelocityChange);
            }
        }
    }

    private void OnDestroy()
    {
        DeleteInteractRange();
    }


    public void UpdateInteractingRigidBody()
    {
        foreach(var kvp in interactors)
        {
            GameObject obj = kvp.Key.interactorParentObject;
            if (!interactorsRigidbodies.ContainsKey(obj))
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                interactorsRigidbodies.Add(obj, rb);
            }

        }
    }

    public void RemoveInteractingRigidBody()
    {
        // 1. ���ռ���ǰ���л�Ծ�� interactorObject
        HashSet<GameObject> activeObjects = new HashSet<GameObject>();
        foreach (var kvp in interactors)
        {
            activeObjects.Add(kvp.Key.interactorParentObject);
        }

        // 2. �������е� rigidbody ӳ�䣬�ҳ���Ч�Ķ���
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in interactorsRigidbodies)
        {
            if (!activeObjects.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        // 3. ִ���Ƴ�
        foreach (var obj in toRemove)
        {
            interactorsRigidbodies.Remove(obj);
        }
    }

    public void ShowInteractRange()
    {
        if (orbitVisualizationOut != null) Destroy(orbitVisualizationOut);
        if (orbitVisualizationIn != null) Destroy(orbitVisualizationIn);

        float minRadius = orbitRadiusRange.x * transform.localScale.x;
        float maxRadius = orbitRadiusRange.y * transform.localScale.x;

        orbitVisualizationIn = CreateOrbitVisualization(minRadius);
        orbitVisualizationOut = CreateOrbitVisualization(maxRadius);
    }

    public void DeleteInteractRange()
    {
        if (orbitVisualizationOut != null) Destroy(orbitVisualizationOut);
        if (orbitVisualizationIn != null) Destroy(orbitVisualizationIn);
        orbitVisualizationOut = null;
        orbitVisualizationIn = null;
    }

    public override void ShowInteractionPrompt()
    {
        var data = new InteractionPromptData($"Under GravitySource {gameObject.name} Interact Range", 0f, transform.position);
        EventCenter.Instance.TriggerEvent("ShowInteractionPrompt", data);
    }


    // ����������ӻ��Ĵ���
    private GameObject CreateOrbitVisualization(float orbit_radius)
    {
        GameObject orbit_visualization = new GameObject("Orbit Visualization");
        orbit_visualization.transform.parent = transform;
        orbit_visualization.transform.localPosition = Vector3.zero;
        orbit_visualization.transform.localRotation = Quaternion.identity;

        Material lineMat = new Material(Shader.Find("Sprites/Default"));
        lineMat.SetColor("_Color", orbitColor);
        lineMat.EnableKeyword("_EMISSION");
        lineMat.SetColor("_EmissionColor", orbitColor * 2f);

        // ����γ�ߺ;���
        for (int i = 0; i < ringCount; i++)
        {
            // γ�߻���
            float theta = Mathf.PI * (i + 1) / (ringCount + 1);
            CreateOrbitRing(orbit_visualization, lineMat, orbit_radius, theta, true);
        }

        for (int i = 0; i < ringCount; i++)
        {
            // ���߻���
            float phi = 2 * Mathf.PI * i / ringCount;
            CreateOrbitRing(orbit_visualization, lineMat, orbit_radius, phi, false);
        }

        return orbit_visualization;
    }

    private void CreateOrbitRing(GameObject parent, Material mat, float orbit_radius, float angle, bool isLatitude)
    {
        GameObject ring = new GameObject(isLatitude ? "Latitude Ring" : "Longitude Ring");
        ring.transform.parent = parent.transform;
        ring.transform.localPosition = Vector3.zero;

        LineRenderer lineRenderer = ring.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = lineSegmentCount + 1;
        lineRenderer.startWidth = orbitLineWidth;
        lineRenderer.endWidth = orbitLineWidth;
        lineRenderer.material = mat;
        lineRenderer.startColor = orbitColor;
        lineRenderer.endColor = orbitColor;

        for (int j = 0; j <= lineSegmentCount; j++)
        {
            float phi = j * 2 * Mathf.PI / lineSegmentCount;
            float x = Mathf.Sin(angle) * Mathf.Cos(phi) * orbit_radius;
            float y = Mathf.Cos(angle) * orbit_radius;
            float z = Mathf.Sin(angle) * Mathf.Sin(phi) * orbit_radius;
            lineRenderer.SetPosition(j, new Vector3(x, y, z));
        }
    }
}
