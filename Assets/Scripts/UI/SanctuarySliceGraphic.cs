using UnityEngine;

// A screen-aligned trapezoid: the artwork stays upright while only its clipping edges lean.
[RequireComponent(typeof(CanvasRenderer))]
public class SanctuarySliceGraphic : UnityEngine.UI.MaskableGraphic
{
    // Bottom left/right, then top left/right, normalized inside this RectTransform.
    [SerializeField] private Vector4 edges = new Vector4(0f, 1f, 0f, 1f);
    [SerializeField] private bool outlineOnly;
    [SerializeField] private bool drawLeft;
    [SerializeField] private bool drawRight;
    [SerializeField] private float lineWidth = 3f;

    public void Configure(Vector4 bounds, bool outline, bool left, bool right)
    {
        edges = bounds;
        outlineOnly = outline;
        drawLeft = left;
        drawRight = right;
        raycastTarget = false;
        SetVerticesDirty();
    }

    public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, screenPoint, eventCamera, out Vector2 local)) return false;

        Rect rect = rectTransform.rect;
        if (rect.width <= 0f || rect.height <= 0f || local.y < rect.yMin || local.y > rect.yMax)
            return false;

        float height = (local.y - rect.yMin) / rect.height;
        float left = rect.xMin + Mathf.Lerp(edges.x, edges.z, height) * rect.width;
        float right = rect.xMin + Mathf.Lerp(edges.y, edges.w, height) * rect.width;
        return local.x >= left && local.x <= right;
    }

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = rectTransform.rect;
        Vector2 bottomLeft = new Vector2(rect.xMin + edges.x * rect.width, rect.yMin);
        Vector2 bottomRight = new Vector2(rect.xMin + edges.y * rect.width, rect.yMin);
        Vector2 topLeft = new Vector2(rect.xMin + edges.z * rect.width, rect.yMax);
        Vector2 topRight = new Vector2(rect.xMin + edges.w * rect.width, rect.yMax);

        if (!outlineOnly)
        {
            AddQuad(mesh, bottomLeft, topLeft, topRight, bottomRight);
            return;
        }

        // Each strip owns the half of the divider inside its own shape.
        if (drawLeft)
        {
            Vector2 direction = (topLeft - bottomLeft).normalized;
            Vector2 inset = new Vector2(direction.y, -direction.x) * lineWidth;
            AddQuad(mesh, bottomLeft, topLeft, topLeft + inset, bottomLeft + inset);
        }
        if (drawRight)
        {
            Vector2 direction = (topRight - bottomRight).normalized;
            Vector2 inset = new Vector2(-direction.y, direction.x) * lineWidth;
            AddQuad(mesh, bottomRight + inset, topRight + inset, topRight, bottomRight);
        }
    }

    private void AddQuad(UnityEngine.UI.VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        int first = mesh.currentVertCount;
        mesh.AddVert(a, color, Vector2.zero);
        mesh.AddVert(b, color, Vector2.zero);
        mesh.AddVert(c, color, Vector2.zero);
        mesh.AddVert(d, color, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2);
        mesh.AddTriangle(first, first + 2, first + 3);
    }
}
