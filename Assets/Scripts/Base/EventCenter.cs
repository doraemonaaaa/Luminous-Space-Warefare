using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 不同脚本或模块间解耦通信，无需知道对方的存在
/// </summary>
public class EventCenter : SingletonMono<EventCenter>
{
    // 指定是否在场景切换时不销毁
    protected override bool isDontDestroyOnLoad => false;

    // 事件表，key是事件名，value是对应的委托列表
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

    #region 注册事件监听
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

    #region 移除事件监听
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

    #region 发送事件
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

    #region 场景回调
    private void OnSceneUnloaded(Scene scene)
    {
        // 可选：在卸载场景时清空事件
        // eventTable.Clear();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 可选：在场景加载时触发某些事件
        TriggerEvent("OnSceneLoaded", scene.name);
    }
    #endregion
}
