using Michsky.UI.Heat;
using System.Collections.Generic;
using UnityEngine;

public class UIControlActions : MonoBehaviour
{
    [Header("UI 状态配置")]
    public List<UIConfig> uiConfigs;

    [Header("面板管理器")]
    public PanelManager panelManager;
    public UIHotKeyControl UIInput;

    private string _lastPanel;
    private Dictionary<string, int> _panelMappings;
    private Dictionary<int, string> _indexToName;

    public GameObject playerInput;
    void Awake()
    {
        _panelMappings = new Dictionary<string, int>();
        _indexToName = new Dictionary<int, string>();

        int i = 0;
        foreach (var panel in panelManager.panels)
        {
            _panelMappings[panel.panelName] = i;
            _indexToName[i] = panel.panelName;
            i++;
        }

        if(playerInput == null)
            playerInput = GameObject.Find("Player_InputSystem");

        panelManager.onPanelChanged.RemoveListener(SwitchUIState);
        panelManager.onPanelChanged.AddListener(SwitchUIState);
    }

    public void SwitchUIState(int index)
    {
        string panelName = _indexToName[index];
        var config = uiConfigs.Find(c => c.panelName == panelName);
        if (config == null)
        {
            Debug.LogWarning($"UI panel '{panelName}' not found in uiConfigs.");
            return;
        }

        SetPlayerInput(config.enablePlayerInput);
        SetUIInput(config.enableUIInput);
        LockCursor(config.lockCursor);
    }

    public void OpenPanel(string panel_name)
    {
        if (!_panelMappings.ContainsKey(panel_name))
        {
            Debug.LogWarning($"面板名称 {panel_name} 不存在于映射中！");
            return;
        }

        if (panelManager.currentPanelIndex != _panelMappings[panel_name])
        {
            _lastPanel = _indexToName[panelManager.currentPanelIndex];

            panelManager.OpenPanel(panel_name);
        }
    }

    public void TogglePanel(string panel_name)
    {
        if (!_panelMappings.ContainsKey(panel_name))
        {
            Debug.LogWarning($"面板名称 {panel_name} 不存在于映射中！");
            return;
        }

        if (panelManager.currentPanelIndex != _panelMappings[panel_name])
        {
            _lastPanel = _indexToName[panelManager.currentPanelIndex];

            panelManager.OpenPanel(panel_name);
        }
        else
        {
            string last_panel = _lastPanel;
            _lastPanel = _indexToName[panelManager.currentPanelIndex];
            panelManager.OpenPanel(last_panel);
        }
    }

    public void SetPlayerInput(bool state)
    {
        if (playerInput != null)
        {
            playerInput.SetActive(state);
        }
        else
        {
            Debug.LogWarning("未找到名为 Player_InputSystem 的物体！");
        }
    }

    public void SetUIInput(bool state)
    {
        if(UIInput != null)
        {
            if (state)
                UIInput.EnableInput();
            else UIInput.DisableInput();
        }
        else
        {
            Debug.LogWarning("未找到名为 UIInput 的物体！");
        }
    }

    public void LockCursor(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
