using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractManager : MonoBehaviour
{

    public InputAction interactHotkey;
    public HashSet<InteractableObject> interactableObjects = new HashSet<InteractableObject>();

    public List<string> interactTag = new List<string> { "Player" };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InteractableObject[] interactable_objects = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        foreach(var obj in interactable_objects)
        {
            interactableObjects.Add(obj);
        }

        interactHotkey.Enable();
    }

    // Update is called once per frame
    void Update()
    {

    }

}