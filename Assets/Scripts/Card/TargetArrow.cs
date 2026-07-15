// ============================================================
// filename   : TargetArrow.cs
// 작성자     : -
// 작성일     : 2026-07-10
// description: 두 점(origin -> pointer)을 잇는 타겟팅 화살표를 그리는
//              범용 UI 그래픽. 카드/아이템 등 타겟 지정 UI에서 공용으로 사용.
//              (구 DebugCardTargetArrow 를 CardInteractionView 에서 분리)
// ============================================================

using UnityEngine;
using UnityEngine.UI;

public class TargetArrow : MaskableGraphic
{
    [SerializeField] private float shaftWidth = 10f;
    [SerializeField] private float headLength = 28f;
    [SerializeField] private float headWidth = 30f;

    private Vector2 startPoint;
    private Vector2 endPoint;

    public void SetPoints(Vector2 start, Vector2 end)
    {
        startPoint = start;
        endPoint = end;
        enabled = true;
        SetVerticesDirty();
    }

    public void Hide()
    {
        enabled = false;
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Vector2 delta = endPoint - startPoint;
        if (delta.sqrMagnitude < 1f) return;

        Vector2 direction = delta.normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        float effectiveHeadLength = Mathf.Min(headLength, delta.magnitude * 0.5f);
        Vector2 headBase = endPoint - direction * effectiveHeadLength;

        int shaftStart = vertexHelper.currentVertCount;
        AddVertex(vertexHelper, startPoint + perpendicular * shaftWidth * 0.5f);
        AddVertex(vertexHelper, startPoint - perpendicular * shaftWidth * 0.5f);
        AddVertex(vertexHelper, headBase - perpendicular * shaftWidth * 0.5f);
        AddVertex(vertexHelper, headBase + perpendicular * shaftWidth * 0.5f);
        vertexHelper.AddTriangle(shaftStart, shaftStart + 1, shaftStart + 2);
        vertexHelper.AddTriangle(shaftStart, shaftStart + 2, shaftStart + 3);

        int headStart = vertexHelper.currentVertCount;
        AddVertex(vertexHelper, headBase + perpendicular * headWidth * 0.5f);
        AddVertex(vertexHelper, headBase - perpendicular * headWidth * 0.5f);
        AddVertex(vertexHelper, endPoint);
        vertexHelper.AddTriangle(headStart, headStart + 1, headStart + 2);
    }

    private void AddVertex(VertexHelper vertexHelper, Vector2 position)
    {
        vertexHelper.AddVert(position, color, Vector2.zero);
    }
}
