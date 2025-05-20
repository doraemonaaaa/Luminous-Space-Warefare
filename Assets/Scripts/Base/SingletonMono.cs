using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    // 静态实例用于存储单例
    private static T _instance;

    // 抽象属性，子类必须实现此属性
    protected abstract bool isDontDestroyOnLoad { get; }

    // 公共静态属性，用于访问单例实例
    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    // Awake 方法用于初始化单例
    protected virtual void Awake()
    {
        // 如果实例已经存在且不是当前对象，则销毁当前对象
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // 否则，将当前对象设置为单例实例
            _instance = (T)this;
            if (isDontDestroyOnLoad)
            {
                // 确保单例在场景切换时不会被销毁
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    // OnDestroy 方法用于清理单例
    protected virtual void OnDestroy()
    {
        // 如果当前对象是单例实例，则将实例设置为 null
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
