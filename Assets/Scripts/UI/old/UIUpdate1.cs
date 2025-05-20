//using Michsky.UI.Heat;
//using TMPro;
//using UnityEngine;

//public class UIUpdate : MonoBehaviour
//{
//    public Ship playerShip;

//    public float speed => playerShip.Rigidbody.linearVelocity.magnitude;
//    public float HPRatio => playerShip.Attributes.HPRatio;
//    public float energyRatio => playerShip.Attributes.EnergyRatio;
//    public float thrusterCapacityRatio => playerShip.Attributes.ThrusterCapacityRatio;  // Magnetoplasmadynamic thruster 等粒子推进器

//    public TextMeshProUGUI speedText;
//    public ProgressBar HPBar;
//    public ProgressBar energyBar;
//    public ProgressBar thrusterCapacityBar;
//    public NotificationManager notificationManager;

//    float _lastSpeed = -1;
//    float _lastHPRatio = -1;
//    float _lastEnergyRatio = -1;
//    float _lastThrusterRatio = -1;

//    void Start()
//    {
//        if (speedText == null || HPBar == null || thrusterCapacityBar == null)
//        {
//            Debug.LogError("UIData: references are not assigned.");
//            return;
//        }

//        EventCenter.Instance.AddListener<InteractionPromptData>("ShowInteractionPrompt", ShowInteractionPrompt);
//        EventCenter.Instance.AddListener<InteractionPromptData>("HideInteractionPrompt", HideInteractionPrompt);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (Mathf.Abs(speed - _lastSpeed) > 0.1f)
//        {
//            speedText.text = $"Speed: {speed:F1} KM/s";
//            _lastSpeed = speed;
//        }

//        if (Mathf.Abs(HPRatio - _lastHPRatio) > 0.01f)
//        {
//            HPBar.SetValue(HPRatio);
//            _lastHPRatio = HPRatio;
//        }

//        if (Mathf.Abs(energyRatio - _lastEnergyRatio) > 0.01f)
//        {
//            energyBar.SetValue(energyRatio);
//            _lastEnergyRatio = energyRatio;
//        }

//        if (Mathf.Abs(thrusterCapacityRatio - _lastThrusterRatio) > 0.01f)
//        {
//            thrusterCapacityBar.SetValue(thrusterCapacityRatio);
//            _lastThrusterRatio = thrusterCapacityRatio;
//        }
//    }

//    private void ShowInteractionPrompt(InteractionPromptData data)
//    {
//        notificationManager.gameObject.SetActive(true);
//        notificationManager.notificationText = data.promptText;
//        notificationManager.minimizeAfter = data.promptDuration;
//        notificationManager.AnimateNotification();
//    }
//    private void HideInteractionPrompt(InteractionPromptData data)
//    {
//        notificationManager.notificationText = data.promptText;
//        notificationManager.minimizeAfter = data.promptDuration;
//        notificationManager.MinimizeNotification();
//    }
//}