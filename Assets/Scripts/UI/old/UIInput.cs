using Michsky.UI.Heat;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PanelMapping
{
    public string Name;
    public int Index;
}

public class UIInput : MonoBehaviour
{
    [Header("������ƺ�����ӳ��")]
    public List<PanelMapping> panelMappingsList;

    public CameraManager cameraManager;
    private Dictionary<string, int> _panelMappings;
    private Dictionary<int, string> _indexToName;
    private string _lastPanel;

    public GameObject playerInput;
    private bool _isMouseLocked = true;
    private bool _isPlayerInputEnabled = true;

    public PanelManager panelManager;
    void Awake()
    {
        _panelMappings = new Dictionary<string, int>();
        _indexToName = new Dictionary<int, string>();

        foreach (var mapping in panelMappingsList)
        {
            _panelMappings[mapping.Name] = mapping.Index;
            _indexToName[mapping.Index] = mapping.Name;
        }

        playerInput = GameObject.Find("Player_InputSystem");

        SetLockMouse(true);
        SetPlayerInput(true);
    }

    private void Start()
    {
        panelManager.OpenPanel("Third Gaming Panel");
        cameraManager.SwitchToCamera("CinemachineCamera Third");
    }

    public void ToggleExitPanel()
    {
        if (panelManager.currentPanelIndex != _panelMappings["ESC Panel"])
        {
            _lastPanel = _indexToName[panelManager.currentPanelIndex];
            panelManager.OpenPanel("ESC Panel");
        }
        else
        {
            string last_panel = _lastPanel;
            _lastPanel = _indexToName[panelManager.currentPanelIndex];
            panelManager.OpenPanel(last_panel);
        }
    }

    public void SwitchView()
    {
        if (panelManager.currentPanelIndex != _panelMappings["Third Gaming Panel"])
        {
            panelManager.OpenPanel("Third Gaming Panel");
            cameraManager.SwitchToCamera("CinemachineCamera Third");
        }
        else
        {
            panelManager.OpenPanel("First Gaming Panel");
            cameraManager.SwitchToCamera("CinemachineCamera First");
        }
    }

    public void SetLockMouse(bool state)
    {
        if (state)
        {
            Cursor.lockState = CursorLockMode.Locked; // ������굽��Ļ����
            Cursor.visible = false;                   // �������
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;   // �������
            Cursor.visible = true;                    // ��ʾ���
        }
        _isMouseLocked = state;
    }

    public void SetPlayerInput(bool state)
    {
        if (playerInput != null)
        {
            playerInput.SetActive(state);
        }
        else
        {
            Debug.LogWarning("δ�ҵ���Ϊ Player_InputSystem �����壡");
        }
        _isPlayerInputEnabled = state;

    }

    public void ToggleLockMouse()
    {
        _isMouseLocked = !_isMouseLocked;
        SetLockMouse(_isMouseLocked);
    }

    public void TogglePlayerInput()
    {
        _isPlayerInputEnabled = !_isPlayerInputEnabled;
        SetPlayerInput(_isPlayerInputEnabled);
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