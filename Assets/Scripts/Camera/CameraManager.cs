using GLTFast.Schema;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : SingletonMono<CameraManager>
{
    public List<CinemachineCamera> cameras;
    private int currentIndex = 0;
    protected override bool isDontDestroyOnLoad => false;
    protected override void Awake()
    {
        base.Awake();

        // 查找所有虚拟摄像机并添加到列表中
        CinemachineCamera[] foundCams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        cameras.AddRange(foundCams);

        // 设置所有摄像机优先级为 0，保留第一个为当前激活
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].Priority = (i == currentIndex) ? 10 : 0;
        }
    }

    public void SwitchToCamera(string cam_name)
    {
        // 查找名字匹配的摄像机
        CinemachineCamera targetCamera = cameras.Find(cam => cam.name == cam_name);

        if (targetCamera != null)
        {
            // 切换到指定的摄像机
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].Priority = 0;
            }

            targetCamera.Priority = 10; // 激活该摄像机
        }
        else
        {
            Debug.LogWarning("摄像机名称未找到: " + cam_name);
        }
    }
    public void SwitchToNextCamera()
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].Priority = 0;
        }

        currentIndex = (currentIndex + 1) % cameras.Count;
        cameras[currentIndex].Priority = 10;
    }
}
