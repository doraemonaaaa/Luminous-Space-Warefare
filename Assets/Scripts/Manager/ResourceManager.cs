using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceManager
{
    /// <summary>
    /// 通用的资源深拷贝方法，用于加载资源并返回一个独立的副本。
    /// </summary>
    /// <typeparam name="T">资源类型，必须继承自 ScriptableObject</typeparam>
    /// <param name="resourcePath">资源在 Resources 文件夹中的路径</param>
    /// <returns>返回加载的资源的深拷贝</returns>
    public static T DeepCopyResource<T>(string resourcePath) where T : ScriptableObject
    {
        // 从 Resources 文件夹加载资源
        T loadedResource = Resources.Load<T>(resourcePath);

        // 检查资源是否加载成功
        if (loadedResource == null)
        {
            Debug.LogError($"Failed to load resource from path: {resourcePath}");
            return null;
        }

        // 使用 Instantiate 复制 ScriptableObject，避免修改原始数据
        T copiedResource = ScriptableObject.Instantiate(loadedResource);

        return copiedResource;
    }

}
