using UnityEngine;
using UnityEngine.UI;

// 1920×1080 판 전체를 덮는 RectTransform 안에서, 꼭짓점 안쪽만 남기는 다각형 마스크.
// 같은 오브젝트에 Mask(showMaskGraphic 끔)를 함께 붙이고 잘라낼 그림을 자식으로 둔다.
// 성소 후보 칸처럼 사선으로 잘린 칸 밖으로 그림이 삐져나오지 않게 할 때 쓴다 (꼭짓점은 layout.json 좌표 그대로).
[RequireComponent(typeof(CanvasRenderer))]
public class UIPolygonMask : MaskableGraphic
{
    private const float CanvasW = 1920f, CanvasH = 1080f;

    [Tooltip("꼭짓점 (1920×1080, 좌상단 원점, 볼록 다각형)")]
    public Vector2[] polygon;

    public void SetPolygon(Vector2[] points)
    {
        polygon = points;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (polygon == null || polygon.Length < 3) return;
        Rect r = rectTransform.rect;
        foreach (var p in polygon)
            vh.AddVert(new Vector3(r.xMin + p.x / CanvasW * r.width, r.yMax - p.y / CanvasH * r.height), color, Vector2.zero);
        for (int i = 1; i < polygon.Length - 1; i++) vh.AddTriangle(0, i, i + 1);
    }
}
