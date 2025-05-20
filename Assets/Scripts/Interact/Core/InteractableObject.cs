using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// once意味着只会触发一次，Duration是有持续性的，Forever是一直触发
/// </summary>
public enum InteractType
{
    Once,
    Duration,
    Forever
}

public enum InteractTriggerType
{
    Active,     // Player initiates it
    Passive     // Triggered by system or environment
}

/// <summary>
/// 抽象的可交互物体类，其他具体的交互物体应该继承此类并实现交互行为。
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactionMessage = "Press 'E' to interact";
    public float interactionRange = 3f;
    public Collider interactTrigger;

    public InteractType interactType = InteractType.Once;
    public InteractTriggerType interactTriggerType = InteractTriggerType.Passive;
    public float interactDuration = 0f;
    private Dictionary<Interactor, float> interactionTimers = new Dictionary<Interactor, float>();
    public Dictionary<Interactor, bool> interactors = new Dictionary<Interactor, bool>();

    public UnityEvent onInteracting;
    public UnityEvent onInteractionCompleted;
    public UnityEvent onInteractionInterrupted;
    public UnityEvent onEnterInteractRange;
    public UnityEvent onExitInteractRange;

    protected virtual void Awake()
    {
        if (!interactTrigger)
        {
            Debug.LogError("InteractableObject: Can't find Collider");
        }
        interactTrigger.isTrigger = true;
    }


    public void Interact(Interactor interactor)
    {
        switch (interactType)
        {
            case InteractType.Once: InteractOnce(interactor); break;
            case InteractType.Duration: InteractInDuration(interactor); break;
            case InteractType.Forever: InteractForever(interactor); break;
        }
#if UNITY_EDITOR
        Debug.Log($"Interact with objects : {gameObject.name}");
#endif
    }

    private void InteractForever(Interactor interactor)
    {
        if (!interactors.ContainsKey(interactor))
            interactors[interactor] = true;
        else
            interactors[interactor] = true;

        interactor.currentStatus = InteractStatus.Interacting;
    }

    private void InteractInDuration(Interactor interactor)
    {
        if (!interactors.ContainsKey(interactor))
            interactors.Add(interactor, true);

        if (!interactionTimers.ContainsKey(interactor))
            interactionTimers[interactor] = 0f;

        interactor.currentStatus = InteractStatus.Interacting;
    }
    private void InteractOnce(Interactor interactor)
    {
        interactor.currentStatus = InteractStatus.Interacting;
        onInteracting?.Invoke();
        onInteractionCompleted?.Invoke();
        interactor.currentStatus = InteractStatus.Completed;
        interactor.interactableObjects.Remove(this);
    }

    /// <summary>
    /// 主动结束交互（例如玩家走开或按取消键）
    /// </summary>
    public virtual void EndInteraction(Interactor interactor)
    {
        if (interactor != null && interactors.ContainsKey(interactor))
        {
            interactors[interactor] = false;
            interactor.currentStatus = InteractStatus.Idle;
            interactor.interactableObjects.Remove(this);
            onInteractionInterrupted?.Invoke();
            if(interactType == InteractType.Forever) onInteractionCompleted?.Invoke();
        }
    }


    /// <summary>
    /// 显示交互提示
    /// </summary>
    public virtual void ShowInteractionPrompt()
    {
        var data = new InteractionPromptData(interactionMessage, 0f, transform.position);
        EventCenter.Instance.TriggerEvent("ShowInteractionPrompt", data);
    }

    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    public virtual void HideInteractionPrompt()
    {
        var data = new InteractionPromptData("Exited The Interact Range", 1f, transform.position);
        EventCenter.Instance.TriggerEvent("HideInteractionPrompt", data);
    }

    /// <summary>
    /// 如果使用 Collider + Trigger，则检测进入交互范围
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        Interactor interactor;
        if (!other.TryGetComponent(out interactor)) return;

        if (!interactors.ContainsKey(interactor))
            interactors.Add(interactor, false);

        interactor.currentStatus = InteractStatus.InRange;
        interactor.interactableObjects.Add(this);
        onEnterInteractRange?.Invoke();

        if (interactor.isPlayer)
        {
            ShowInteractionPrompt();
        }

        if (interactTriggerType == InteractTriggerType.Passive || !interactor.isPlayer) // 被动交互或者AI主动交互
        {
            Interact(interactor);
        }

        //Debug.Log($"{interactor.interactorObject.name}进入了交互范围");
    }


    /// <summary>
    /// 离开交互范围
    /// </summary>
    protected virtual void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Interactor>(out var interactor)) return;

        if (interactors.ContainsKey(interactor))
        {
            EndInteraction(interactor);
            interactors.Remove(interactor);
        }

        if (interactors.Count == 0)
        {
            interactor.currentStatus = InteractStatus.Idle;
            interactor.interactableObjects.Remove(this);
        }

        if (interactor.isPlayer)
        {
            HideInteractionPrompt();
        }

        onExitInteractRange?.Invoke();
    }


    protected virtual void OnTriggerStay(Collider other)
    {
        if (interactType == InteractType.Duration)
        {
            List<Interactor> completed = new List<Interactor>();

            foreach (var kvp in interactors)
            {
                if (!kvp.Value) continue;

                if (!interactionTimers.ContainsKey(kvp.Key))
                    interactionTimers[kvp.Key] = 0f;

                interactionTimers[kvp.Key] += Time.deltaTime;

                onInteracting?.Invoke();

                if (interactionTimers[kvp.Key] >= interactDuration)
                {
                    onInteractionCompleted?.Invoke();
                    completed.Add(kvp.Key);
                }
            }

            foreach (var interactor in completed)
            {
                EndInteraction(interactor);
                interactionTimers.Remove(interactor);
            }
        }

        else if (interactType == InteractType.Forever)
        {
            foreach (var kvp in interactors)
            {
                if (kvp.Value)
                    onInteracting?.Invoke();
            }
        }
    }
}
