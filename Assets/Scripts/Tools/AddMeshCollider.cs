
//EditorWindow类无法打包,主意添加这个判断
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class AddMeshCollider : EditorWindow
{
    [MenuItem("Tools/添加移除碰撞体")]

    public static void Open()
    {
        EditorWindow.GetWindow(typeof(AddMeshCollider));
    }
    void OnGUI()
    {
        if (GUILayout.Button("添加碰撞体"))
        {
            Add();
        }

        if (GUILayout.Button("移除碰撞体"))
        {
            Remove();
        }
    }
    public static void Remove()
    {
        //寻找Hierarchy面板下所有的MeshRenderer
        var tArray = Resources.FindObjectsOfTypeAll(typeof(MeshRenderer));
        for (int i = 0; i < tArray.Length; i++)
        {
            MeshRenderer t = tArray[i] as MeshRenderer;
            //这个很重要，博主发现如果没有这个代码，unity是不会察觉到编辑器有改动的，自然设置完后直接切换场景改变是不被保存
            //的  如果不加这个代码  在做完更改后 自己随便手动修改下场景里物体的状态 在保存就好了 
            Undo.RecordObject(t, t.gameObject.name);

            MeshCollider meshCollider = t.gameObject.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                DestroyImmediate(meshCollider);
            }

            //相当于让他刷新下 不然unity显示界面还不知道自己的东西被换掉了  还会呆呆的显示之前的东西
            EditorUtility.SetDirty(t);
        }
        Debug.Log("remove Succed");
    }

    public static void Add()
    {
        //寻找Hierarchy面板下所有的MeshRenderer
        var tArray = Resources.FindObjectsOfTypeAll(typeof(MeshRenderer));
        for (int i = 0; i < tArray.Length; i++)
        {
            MeshRenderer t = tArray[i] as MeshRenderer;
            //这个很重要，博主发现如果没有这个代码，unity是不会察觉到编辑器有改动的，自然设置完后直接切换场景改变是不被保存
            //的  如果不加这个代码  在做完更改后 自己随便手动修改下场景里物体的状态 在保存就好了 
            Undo.RecordObject(t, t.gameObject.name);

            MeshCollider meshCollider = t.gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                t.gameObject.AddComponent<MeshCollider>();
            }

            //相当于让他刷新下 不然unity显示界面还不知道自己的东西被换掉了  还会呆呆的显示之前的东西
            EditorUtility.SetDirty(t);
        }
        Debug.Log("Add Succed");
    }
}

#endif