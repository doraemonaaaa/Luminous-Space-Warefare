using UnityEngine;
using VSX.Vehicles;

public class BlackHoleInteract : InteractableObject
{
    public float eventHorizonDistance = 10f;

    void Start()
    {

    }

    public void EventHorizon()
    {
        foreach (var kvp in interactors)
        {
            if (kvp.Value)
            {
                GameObject target_obj = kvp.Key.interactorParentObject;
                Transform target_transform = target_obj.transform;
                float distance = Vector3.Distance(transform.position, target_transform.position);
                if (distance <= eventHorizonDistance)
                {
                    Vehicle v;
                    if (target_obj.TryGetComponent<Vehicle>(out v))
                    {
                        v.Destroy(); // �¼��ӽ�ݻ������ؾ�
                    }
                }
            }
        }
    }

    // ���ӻ��¼��ӽ緶Χ
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f); // ��͸����ɫ
        Gizmos.DrawSphere(transform.position, eventHorizonDistance);

        Gizmos.color = Color.red; // ʵ�ĺ�ɫ�߽���
        Gizmos.DrawWireSphere(transform.position, eventHorizonDistance);
    }
}
