using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : SingletonMono<ObjectPoolManager>
{
    protected override bool isDontDestroyOnLoad => false;

    [Serializable]
    public class PoolSettings
    {
        public string poolName;
        public GameObject prefab;
        public int defaultSize = 10;
        public int maxSize = 100;
    }

    [SerializeField] private List<PoolSettings> poolSettings = new List<PoolSettings>();

    // 使用StringComparer.Ordinal提高字符串比较性能
    private readonly Dictionary<string, ObjectPool<GameObject>> _pools = new Dictionary<string, ObjectPool<GameObject>>(StringComparer.Ordinal);
    private readonly Dictionary<GameObject, string> _objectToPoolMap = new Dictionary<GameObject, string>(256);
    private Transform _transform;

    private static readonly WaitForSeconds _wait1s = new WaitForSeconds(1f);
    private static readonly Quaternion _identityRotation = Quaternion.identity;

    protected override void Awake()
    {
        base.Awake();
        _transform = transform;
        InitializePools();
    }

    private void InitializePools()
    {
        for (int i = 0; i < poolSettings.Count; i++)
        {
            var settings = poolSettings[i];
            if (settings.prefab != null)
            {
                InitializePool(settings.poolName, settings.prefab, settings.defaultSize, settings.maxSize);
            }
        }
    }

    public void InitializePool(string poolName, GameObject prefab, int defaultSize = 10, int maxSize = 100)
    {
        if (string.IsNullOrEmpty(poolName) || prefab == null || _pools.ContainsKey(poolName))
            return;

        // 创建容器
        Transform container = new GameObject(poolName).transform;
        container.SetParent(_transform, false);

        // 创建对象池
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => {
                var obj = Instantiate(prefab, container);
                _objectToPoolMap[obj] = poolName;
                return obj;
            },
            actionOnGet: obj => {
                obj.SetActive(true);
                obj.transform.SetParent(null);
            },
            actionOnRelease: obj => {
                obj.SetActive(false);
                obj.transform.SetParent(container);
            },
            actionOnDestroy: obj => {
                _objectToPoolMap.Remove(obj);
                Destroy(obj);
            },
            collectionCheck: false, // 禁用集合检查以提高性能
            defaultCapacity: defaultSize,
            maxSize: maxSize
        );

        _pools[poolName] = pool;

        // 预热对象池
        var tempObjects = new List<GameObject>(defaultSize);
        for (int i = 0; i < defaultSize; i++)
        {
            tempObjects.Add(pool.Get());
        }

        for (int i = 0; i < tempObjects.Count; i++)
        {
            pool.Release(tempObjects[i]);
        }
    }

    public GameObject GetObject(string poolName, Vector3 position, Quaternion rotation = default)
    {
        if (string.IsNullOrEmpty(poolName) || !_pools.TryGetValue(poolName, out var pool))
            return null;

        GameObject obj = pool.Get();
        if (obj == null)
            return null;

        Transform objTransform = obj.transform;
        objTransform.position = position;
        objTransform.rotation = rotation == default ? _identityRotation : rotation;

        return obj;
    }

    public void ReleaseObject(GameObject obj)
    {
        if (obj == null)
            return;

        if (_objectToPoolMap.TryGetValue(obj, out string poolName) && _pools.TryGetValue(poolName, out var pool))
        {
            pool.Release(obj);
        }
    }

    public void ReleaseAfterDelay(GameObject obj, float delay)
    {
        if (obj == null)
            return;

        StartCoroutine(ReleaseAfterDelayCoroutine(obj, delay));
    }

    private IEnumerator ReleaseAfterDelayCoroutine(GameObject obj, float delay)
    {
        if (delay <= 0)
        {
            ReleaseObject(obj);
            yield break;
        }

        if (Mathf.Approximately(delay, 1f))
            yield return _wait1s;
        else
            yield return new WaitForSeconds(delay);

        ReleaseObject(obj);
    }

    public void ClearAllPools()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Clear();
        }

        // 清理所有容器子物体
        for (int i = 0; i < _transform.childCount; i++)
        {
            Destroy(_transform.GetChild(i).gameObject);
        }

        _pools.Clear();
        _objectToPoolMap.Clear();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ClearAllPools();
    }
}