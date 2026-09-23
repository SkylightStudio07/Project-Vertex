using UnityEngine;

// 해상도와 패널 너비에 관계없이 선명한 절단형 프레임을 그린다.
[AddComponentMenu("UI/Vertex/Blessing Panel")]
[RequireComponent(typeof(CanvasRenderer))]
public class BlessingPanelGraphic : UnityEngine.UI.MaskableGraphic
{
    [SerializeField, Min(0f)] private float cornerCut = 12f;
    [SerializeField, Min(0f)] private float borderWidth = 1.5f;
    [SerializeField] private Color borderColor = new(0.14f, 0.17f, 0.2f, 1f);

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
    {
        vh.Clear();
        Rect outer = GetPixelAdjustedRect();
        float width = Mathf.Min(borderWidth, Mathf.Min(outer.width, outer.height) * 0.5f);
        Rect inner = new(outer.xMin + width, outer.yMin + width,
            Mathf.Max(0f, outer.width - width * 2f), Mathf.Max(0f, outer.height - width * 2f));
        AddPolygon(vh, outer, cornerCut, borderColor);
        AddPolygon(vh, inner, Mathf.Max(0f, cornerCut - width), color);
    }

    private static void AddPolygon(UnityEngine.UI.VertexHelper vh, Rect rect, float cut, Color tint)
    {
        cut = Mathf.Min(cut, Mathf.Min(rect.width, rect.height) * 0.5f);
        int start = vh.currentVertCount;
        vh.AddVert(new Vector3(rect.xMin + cut, rect.yMin), tint, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax, rect.yMin), tint, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax, rect.yMax - cut), tint, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax - cut, rect.yMax), tint, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin, rect.yMax), tint, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin, rect.yMin + cut), tint, Vector2.zero);
        for (int i = 1; i < 5; i++) vh.AddTriangle(start, start + i, start + i + 1);
    }
}
