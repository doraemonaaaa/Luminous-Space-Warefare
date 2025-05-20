using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ��ͬ�ű���ģ������ͨ�ţ�����֪���Է��Ĵ���
/// </summary>
public class EventCenter : SingletonMono<EventCenter>
{
    // ָ���Ƿ��ڳ����л�ʱ������
    protected override bool isDontDestroyOnLoad => false;

    // �¼���key���¼�����value�Ƕ�Ӧ��ί���б�
    private Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    #region ע���¼�����
    public void AddListener<T>(string eventName, Action<T> callback)
    {
        if (eventTable.TryGetValue(eventName, out var existingDelegate))
        {
            eventTable[eventName] = Delegate.Combine(existingDelegate, callback);
        }
        else
        {
            eventTable[eventName] = callback;
        }
    }

    public void AddListener(string eventName, Action callback)
    {
        if (eventTable.TryGetValue(eventName, out var existingDelegate))
        {
            eventTable[eventName] = Delegate.Combine(existingDelegate, callback);
        }
        else
        {
            eventTable[eventName] = callback;
        }
    }
    #endregion

    #region �Ƴ��¼�����
    public void RemoveListener<T>(string eventName, Action<T> callback)
    {
        if (eventTable.TryGetValue(eventName, out var existingDelegate))
        {
            var newDelegate = Delegate.Remove(existingDelegate, callback);
            if (newDelegate == null)
                eventTable.Remove(eventName);
            else
                eventTable[eventName] = newDelegate;
        }
    }

    public void RemoveListener(string eventName, Action callback)
    {
        if (eventTable.TryGetValue(eventName, out var existingDelegate))
        {
            var newDelegate = Delegate.Remove(existingDelegate, callback);
            if (newDelegate == null)
                eventTable.Remove(eventName);
            else
                eventTable[eventName] = newDelegate;
        }
    }
    #endregion

    #region �����¼�
    public void TriggerEvent<T>(string eventName, T arg)
    {
        if (eventTable.TryGetValue(eventName, out var d))
        {
            if (d is Action<T> callback)
            {
                callback(arg);
            }
        }
    }

    public void TriggerEvent(string eventName)
    {
        if (eventTable.TryGetValue(eventName, out var d))
        {
            if (d is Action callback)
            {
                callback();
            }
        }
    }
    #endregion

    #region �����ص�
    private void OnSceneUnloaded(Scene scene)
    {
        // ��ѡ����ж�س���ʱ����¼�
        // eventTable.Clear();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ��ѡ���ڳ�������ʱ����ĳЩ�¼�
        TriggerEvent("OnSceneLoaded", scene.name);
    }
    #endregion
}
