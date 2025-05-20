using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum InteractorType
{
    People,
    Vehicle
}

/// <summary>
/// ��ʾ����״̬
/// </summary>
public enum InteractStatus
{
    Idle,           // δ����
    InRange,        // ����ڽ�����Χ��
    Interacting,    // ���ڽ�����
    Completed,      // �������
    Exiting         // �����˳�����
}


public class Interactor : MonoBehaviour
{
    public InteractorType interactorType;
    public bool isPlayer = false;
    public static Interactor Player;
    public GameObject interactorParentObject;
    private SphereCollider interactDetector;

    public InteractStatus currentStatus = InteractStatus.Idle;
    public List<InteractableObject> interactableObjects = new List<InteractableObject>();
    private void Awake()
    {
        if (interactorParentObject == null) Debug.LogError("Interactor : Can't find Interactor Object");

        if (isPlayer)
            Player = this;
        if(interactDetector == null) gameObject.AddComponent<SphereCollider>();
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void Interact()
    {
        foreach(var obj in interactableObjects)
            obj.Interact(this);
    }
    
    /// <summary>
    /// ���������ض�����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Interact<T>() where T : InteractableObject
    {
        foreach (var obj in interactableObjects)
        {
            if (obj.GetType() == typeof(T))
            {
                obj.Interact(this); // ȷ�� this �ǺϷ��Ĳ���
            }
        }
    }

}
