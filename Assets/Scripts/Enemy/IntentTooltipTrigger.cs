using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 적 전체의 호버는 EnemyHoverInfo가 받고, 여기서는 의도별 툴팁 생성·배치만 맡는다.
public class IntentTooltipTrigger : MonoBehaviour
{
    [SerializeField] private IntentTooltipView tooltipPrefab;
    [SerializeField] private RectTransform tooltipGroupPrefab;

    private EnemyInstance _enemy;
    private IReadOnlyList<IntentType> _intents = System.Array.Empty<IntentType>();
    private RectTransform _anchor;
    private int? _damage;
    private bool _shown;
    private RectTransform _group;
    private readonly List<IntentTooltipView> _items = new();

    // EnemyView가 실제로 표시한 의도 목록을 전달하여 아이콘과 설명을 일치시킨다.
    public void SetOwner(EnemyInstance enemy, int? damage, IReadOnlyList<IntentType> intents, RectTransform anchor)
    {
        _enemy = enemy;
        _damage = damage;
        _intents = intents;
        _anchor = anchor;
        if (_shown) Show();
    }

    public void Show()
    {
        if (_enemy == null || _enemy.IsDead || tooltipPrefab == null || tooltipGroupPrefab == null ||
            _intents == null || _intents.Count == 0 || _anchor == null || !_anchor.gameObject.activeInHierarchy)
        {
            Hide();
            return;
        }
        var canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        if (canvas == null) return;
        EnsureGroup(canvas);
        _shown = true;
        var view = GetComponentInParent<EnemyView>();
        int count = 0;
        foreach (var type in _intents)
        {
            if (count == _items.Count) _items.Add(Instantiate(tooltipPrefab, _group));
            var item = _items[count++];
            Describe(type, type == IntentType.Attack ? _damage : null, out string title, out string description);
            item.Bind(view != null ? view.GetIntentSprite(type) : null, title, description);
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
        var anchor = RectTransformUtility.CalculateRelativeRectTransformBounds(root, _anchor);
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

    private static void Describe(IntentType type, int? damage, out string title, out string description)
    {
        switch (type)
        {
            case IntentType.Attack:
                title = "공격";
                description = "이 적은 플레이어를 공격하려 하고 있습니다.";
                if (damage.HasValue) description += $" <b>{damage.Value}</b>의 피해를 줍니다.";
                break;
            case IntentType.Defend:
                title = "방어";
                description = "이 적은 방어도를 얻으려 하고 있습니다.";
                break;
            case IntentType.Buff:
                title = "강화";
                description = "이 적은 스스로를 강화하려 하고 있습니다.";
                break;
            case IntentType.Debuff:
                title = "약화";
                description = "이 적은 플레이어에게 약화 효과를 부여하려 하고 있습니다.";
                break;
            default:
                title = "대기";
                description = "이 적은 아무 행동도 하지 않습니다.";
                break;
        }
    }

    private void OnDestroy()
    {
        if (_group == null) return;
        if (Application.isPlaying) Destroy(_group.gameObject);
        else DestroyImmediate(_group.gameObject);
    }
}
