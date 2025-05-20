using UnityEngine;

public static class ObjectFinder
{
    /// <summary>
    /// 在整个场景中查找（包括 inactive 对象）某个脚本组件
    /// </summary>
    public static T FindIncludingInactive<T>() where T : Component
    {
        T[] all = Resources.FindObjectsOfTypeAll<T>();

        foreach (var t in all)
        {
            // 必须排除编辑器中的对象（如预制体预览等）
            if (t.hideFlags == HideFlags.None && t.gameObject.scene.IsValid())
            {
                return t;
            }
        }

        return null;
    }
}
