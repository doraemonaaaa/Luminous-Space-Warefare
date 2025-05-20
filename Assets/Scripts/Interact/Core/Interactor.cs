using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum InteractorType
{
    People,
    Vehicle
}

/// <summary>
/// 表示交互状态
/// </summary>
public enum InteractStatus
{
    Idle,           // 未交互
    InRange,        // 玩家在交互范围内
    Interacting,    // 正在交互中
    Completed,      // 交互完成
    Exiting         // 主动退出交互
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
    /// 主动交互
    /// </summary>
    public void Interact()
    {
        foreach(var obj in interactableObjects)
            obj.Interact(this);
    }
    
    /// <summary>
    /// 主动交互特定物体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Interact<T>() where T : InteractableObject
    {
        foreach (var obj in interactableObjects)
        {
            if (obj.GetType() == typeof(T))
            {
                obj.Interact(this); // 确保 this 是合法的参数
            }
        }
    }

}
