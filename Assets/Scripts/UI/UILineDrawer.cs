using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineDrawer : Graphic
{
    public Vector2 start;
    public Vector2 end;
    public float thickness = 2f;

    // 缓存上一帧的 start, end, 和 color
    private Vector2 lastStart;
    private Vector2 lastEnd;
    private Color lastColor;

    // 通过此方法设置线条的起始点和结束点
    public void SetLine(Vector2 start, Vector2 end)
    {
        // 只有在 start 或 end 改变时才需要重绘
        if (this.start != start || this.end != end || color != lastColor)
        {
            this.start = start;
            this.end = end;
            lastStart = start;
            lastEnd = end;
            lastColor = color;  // 缓存当前的颜色

            SetVerticesDirty(); // 通知重绘
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // 计算线段的方向与法线
        Vector2 direction = (end - start).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x) * thickness * 0.5f;

        Vector2 v0 = start + normal;
        Vector2 v1 = start - normal;
        Vector2 v2 = end - normal;
        Vector2 v3 = end + normal;

        // 设置顶点颜色和UV
        vh.AddVert(v0, color, Vector2.zero);
        vh.AddVert(v1, color, Vector2.zero);
        vh.AddVert(v2, color, Vector2.zero);
        vh.AddVert(v3, color, Vector2.zero);

        // 设置三角形
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
