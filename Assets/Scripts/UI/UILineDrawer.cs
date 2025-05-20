using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineDrawer : Graphic
{
    public Vector2 start;
    public Vector2 end;
    public float thickness = 2f;

    // ������һ֡�� start, end, �� color
    private Vector2 lastStart;
    private Vector2 lastEnd;
    private Color lastColor;

    // ͨ���˷���������������ʼ��ͽ�����
    public void SetLine(Vector2 start, Vector2 end)
    {
        // ֻ���� start �� end �ı�ʱ����Ҫ�ػ�
        if (this.start != start || this.end != end || color != lastColor)
        {
            this.start = start;
            this.end = end;
            lastStart = start;
            lastEnd = end;
            lastColor = color;  // ���浱ǰ����ɫ

            SetVerticesDirty(); // ֪ͨ�ػ�
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // �����߶εķ����뷨��
        Vector2 direction = (end - start).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x) * thickness * 0.5f;

        Vector2 v0 = start + normal;
        Vector2 v1 = start - normal;
        Vector2 v2 = end - normal;
        Vector2 v3 = end + normal;

        // ���ö�����ɫ��UV
        vh.AddVert(v0, color, Vector2.zero);
        vh.AddVert(v1, color, Vector2.zero);
        vh.AddVert(v2, color, Vector2.zero);
        vh.AddVert(v3, color, Vector2.zero);

        // ����������
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
