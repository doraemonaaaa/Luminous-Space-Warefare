using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VerletSolver
{
    /// <summary>
    /// 质点系统的的物理运动模拟
    /// </summary>
    /// <param name="points"></param>
    /// <param name="damping"></param>
    /// <param name="gravity"></param>
    public static void PointsSolver(ref Point[] points, float damping, float gravity)
    {
        for (int i = 0; i < points.Length; i++)
        {
            // 判断当前点是否固定
            bool is_fixed = points[i].IsFixed;
            if (!is_fixed)
            {
                // 更新点的位置
                Vector3 curr = points[i].Position;
                points[i].Position += (points[i].Position - points[i].OldPosition) * damping + Vector3.down * gravity * Time.deltaTime * Time.deltaTime;
                points[i].OldPosition = curr;
            }
        }
    }
}


// 定义 Point 结构
public struct Point
{
    public Vector3 Position;
    public Vector3 OldPosition;
    public bool IsFixed;

    public Point(Vector3 position, bool isFixed = false)
    {
        Position = position;
        OldPosition = position;
        IsFixed = isFixed;
    }
}