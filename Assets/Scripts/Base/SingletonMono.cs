using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    // ��̬ʵ�����ڴ洢����
    private static T _instance;

    // �������ԣ��������ʵ�ִ�����
    protected abstract bool isDontDestroyOnLoad { get; }

    // ������̬���ԣ����ڷ��ʵ���ʵ��
    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    // Awake �������ڳ�ʼ������
    protected virtual void Awake()
    {
        // ���ʵ���Ѿ������Ҳ��ǵ�ǰ���������ٵ�ǰ����
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // ���򣬽���ǰ��������Ϊ����ʵ��
            _instance = (T)this;
            if (isDontDestroyOnLoad)
            {
                // ȷ�������ڳ����л�ʱ���ᱻ����
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    // OnDestroy ��������������
    protected virtual void OnDestroy()
    {
        // �����ǰ�����ǵ���ʵ������ʵ������Ϊ null
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
