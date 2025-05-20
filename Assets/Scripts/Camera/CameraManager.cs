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

        // ���������������������ӵ��б���
        CinemachineCamera[] foundCams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        cameras.AddRange(foundCams);

        // ����������������ȼ�Ϊ 0��������һ��Ϊ��ǰ����
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].Priority = (i == currentIndex) ? 10 : 0;
        }
    }

    public void SwitchToCamera(string cam_name)
    {
        // ��������ƥ��������
        CinemachineCamera targetCamera = cameras.Find(cam => cam.name == cam_name);

        if (targetCamera != null)
        {
            // �л���ָ���������
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].Priority = 0;
            }

            targetCamera.Priority = 10; // ����������
        }
        else
        {
            Debug.LogWarning("���������δ�ҵ�: " + cam_name);
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
