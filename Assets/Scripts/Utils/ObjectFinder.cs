using UnityEngine;

public static class ObjectFinder
{
    /// <summary>
    /// �����������в��ң����� inactive ����ĳ���ű����
    /// </summary>
    public static T FindIncludingInactive<T>() where T : Component
    {
        T[] all = Resources.FindObjectsOfTypeAll<T>();

        foreach (var t in all)
        {
            // �����ų��༭���еĶ�����Ԥ����Ԥ���ȣ�
            if (t.hideFlags == HideFlags.None && t.gameObject.scene.IsValid())
            {
                return t;
            }
        }

        return null;
    }
}
