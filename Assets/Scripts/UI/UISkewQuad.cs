using UnityEngine;
using UnityEngine.UI;

// 윗변을 오른쪽으로 skew만큼 민 평행사변형 (명일방주식 사선 띠).
// skew가 음수면 왼쪽으로 기운다. 텍스처 없이 단색으로 그린다.
public class UISkewQuad : MaskableGraphic
{
    [SerializeField] private float skew = 40f;

    public float Skew
    {
        get => skew;
        set { skew = value; SetVerticesDirty(); }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var r = GetPixelAdjustedRect();
        float half = skew * 0.5f;
        var c = (Color32)color;
        vh.AddVert(new Vector3(r.xMin - half, r.yMin), c, Vector2.zero);
        vh.AddVert(new Vector3(r.xMin + half, r.yMax), c, Vector2.up);
        vh.AddVert(new Vector3(r.xMax + half, r.yMax), c, Vector2.one);
        vh.AddVert(new Vector3(r.xMax - half, r.yMin), c, Vector2.right);
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
