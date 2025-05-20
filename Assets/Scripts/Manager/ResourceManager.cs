using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceManager
{
    /// <summary>
    /// ͨ�õ���Դ������������ڼ�����Դ������һ�������ĸ�����
    /// </summary>
    /// <typeparam name="T">��Դ���ͣ�����̳��� ScriptableObject</typeparam>
    /// <param name="resourcePath">��Դ�� Resources �ļ����е�·��</param>
    /// <returns>���ؼ��ص���Դ�����</returns>
    public static T DeepCopyResource<T>(string resourcePath) where T : ScriptableObject
    {
        // �� Resources �ļ��м�����Դ
        T loadedResource = Resources.Load<T>(resourcePath);

        // �����Դ�Ƿ���سɹ�
        if (loadedResource == null)
        {
            Debug.LogError($"Failed to load resource from path: {resourcePath}");
            return null;
        }

        // ʹ�� Instantiate ���� ScriptableObject�������޸�ԭʼ����
        T copiedResource = ScriptableObject.Instantiate(loadedResource);

        return copiedResource;
    }

}
