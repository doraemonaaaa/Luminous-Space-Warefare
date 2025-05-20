using UnityEngine;

public static class TransformUtils
{
    public static Transform GetTopParent(Transform child)
    {
        while (child.parent != null)
        {
            child = child.parent;
        }
        return child;
    }
}
