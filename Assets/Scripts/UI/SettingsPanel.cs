using Michsky.UI.Heat;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public SwitchManager FPSSwitch;
    public Scrollbar audioVolumeScrollbar;

    public void Awake()
    {
        FPSSwitch.onValueChanged.RemoveListener(SetFPS);
        FPSSwitch.onValueChanged.AddListener(SetFPS);

        audioVolumeScrollbar.onValueChanged.RemoveListener(SetGlobalAudioVolume);
        audioVolumeScrollbar.onValueChanged.AddListener(SetGlobalAudioVolume);

    }

    public void OnEnable()
    {
        
    }

    public void OnDisable()
    {
        
    }

    public void SetFPS(bool is_on)
    {
        FPSUI ui = ObjectFinder.FindIncludingInactive<FPSUI>();
        ui.gameObject.SetActive(is_on);
        GameSettingManager.Instance.gameSettingsSO.showFPS = is_on;
    }

    public void SetGlobalAudioVolume(float volume)
    {
        AudioListener.volume = volume;
        GameSettingManager.Instance.gameSettingsSO.globalVolume = volume;
    }
}
