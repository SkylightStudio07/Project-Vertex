using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 적 전체의 호버는 EnemyHoverInfo가 받고, 여기서는 의도별 툴팁 생성·배치만 맡는다.
[RequireComponent(typeof(Graphic))]
public class IntentTooltipTrigger : MonoBehaviour
{
    [SerializeField] private IntentTooltipView tooltipPrefab;
    [SerializeField] private RectTransform tooltipGroupPrefab;
    [SerializeField] private List<IntentDefinition> definitions = new();

    private EnemyInstance _enemy;
    private IntentType[] _intents = System.Array.Empty<IntentType>();
    private int? _damage;
    private bool _shown;
    private RectTransform _group;
    private readonly List<IntentTooltipView> _items = new();

    private void Awake() => GetComponent<Graphic>().raycastTarget = true;

    // EnemyView가 실제로 표시한 의도 목록을 전달하여 아이콘과 설명을 일치시킨다.
    public void SetOwner(EnemyInstance enemy, int? damage, params IntentType[] intents)
    {
        _enemy = enemy;
        _damage = damage;
        _intents = intents;
        if (_shown) Show();
    }

    public void Show()
    {
        if (_enemy == null || _enemy.IsDead || tooltipPrefab == null || tooltipGroupPrefab == null ||
            !TryGetComponent<Image>(out var icon) || !icon.enabled)
        {
            Hide();
            return;
        }
        var canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        if (canvas == null) return;
        EnsureGroup(canvas);
        _shown = true;
        int count = 0;
        foreach (var type in _intents)
        {
            var definition = definitions.Find(x => x != null && x.type == type);
            if (definition == null) continue;
            if (count == _items.Count) _items.Add(Instantiate(tooltipPrefab, _group));
            var item = _items[count++];
            item.Bind(definition, type == IntentType.Attack ? _damage : null);
            item.gameObject.SetActive(true);
        }
        for (int i = count; i < _items.Count; i++) _items[i].gameObject.SetActive(false);
        _group.gameObject.SetActive(count > 0);
        if (count > 0) PositionNear((RectTransform)canvas.transform);
    }

    private void EnsureGroup(Canvas canvas)
    {
        if (_group != null) return;
        _group = Instantiate(tooltipGroupPrefab, canvas.transform, false);
    }

    // 항목 정렬·크기는 프리팹의 LayoutGroup/ContentSizeFitter가 담당한다.
    // 여기서는 툴팁 묶음을 적 옆에 놓고 화면 밖으로 나가지 않도록 보정한다.
    private void PositionNear(RectTransform root)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_group);
        var anchor = RectTransformUtility.CalculateRelativeRectTransformBounds(root, transform);
        const float margin = 16f;
        float width = _group.rect.width;
        var center = new Vector2(anchor.min.x - margin - width * 0.5f, anchor.center.y);
        var half = _group.rect.size * 0.5f;
        var area = root.rect;
        center.x = Mathf.Clamp(center.x, area.xMin + margin + half.x, Mathf.Max(area.xMin + margin + half.x, area.xMax - margin - half.x));
        center.y = Mathf.Clamp(center.y, area.yMin + margin + half.y, Mathf.Max(area.yMin + margin + half.y, area.yMax - margin - half.y));
        _group.position = root.TransformPoint(center);
    }

    public void Hide()
    {
        _shown = false;
        if (_group != null) _group.gameObject.SetActive(false);
    }

    private void OnDisable() => Hide();

    private void OnDestroy()
    {
        if (_group == null) return;
        if (Application.isPlaying) Destroy(_group.gameObject);
        else DestroyImmediate(_group.gameObject);
    }
}
