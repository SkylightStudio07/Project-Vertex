using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 적 위에 마우스를 올리면(스프라이트·HP바·인텐트 어디든) 슬더스처럼 HP바 아래에 이름을 띄우고,
// 인텐트 툴팁도 같이 띄운다. 적 프리팹 루트(EnemyView)에 붙인다.
// 포인터 Enter/Exit는 자식 그래픽 사이를 오가도 루트 기준으로 한 번씩만 오므로 깜빡이지 않는다.
[RequireComponent(typeof(EnemyView))]
public class EnemyHoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject nameLabel;      // HP바 아래 이름 (평소 숨김)
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private IntentTooltipTrigger intentTooltip;

    private EnemyView _view;
    private bool _hovering;

    private void Awake()
    {
        _view = GetComponent<EnemyView>();
        if (nameLabel != null) nameLabel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var enemy = _view.Instance;
        if (enemy == null || enemy.IsDead) return;
        _hovering = true;

        // 이름 밑에 속성 한 줄 (기록 2편 전엔 '속성 미상 — 추가 탐색 필요')
        if (nameText != null) nameText.text = enemy.Data != null
            ? $"{enemy.Data.enemyName}\n<size=72%><color=#9AA3AA>{EnemyTraits.HoverLine(enemy.Data)}</color></size>"
            : string.Empty;
        if (nameLabel != null) nameLabel.SetActive(true);
        FitNameLabel();
        // 카드로 겨냥 중엔 툴팁이 조준선·예상 피해를 가리므로 이름만 보여준다
        if (intentTooltip != null && !CardHandler.IsAnyDragging) intentTooltip.Show();
    }

    public void OnPointerExit(PointerEventData eventData) => Hide();

    // 속성 줄이 붙어 두 줄이 되면 이름표를 아래로 늘린다 (위 끝은 HP 바 밑 그대로)
    private bool _labelHomeSet;
    private Vector2 _labelHomeSize, _labelHomePos;
    private void FitNameLabel()
    {
        if (nameLabel == null || nameText == null) return;
        var r = (RectTransform)nameLabel.transform;
        if (!_labelHomeSet) { _labelHomeSize = r.sizeDelta; _labelHomePos = r.anchoredPosition; _labelHomeSet = true; }
        nameText.ForceMeshUpdate();
        float pad = Mathf.Max(0f, _labelHomeSize.y - nameText.GetPreferredValues(nameText.text.Split('\n')[0]).y);
        float h = Mathf.Max(_labelHomeSize.y, nameText.preferredHeight + pad);
        r.sizeDelta = new Vector2(_labelHomeSize.x, h);
        r.anchoredPosition = _labelHomePos + new Vector2(0f, (1f - r.pivot.y) * (_labelHomeSize.y - h));
        // 이름 뒤 어두운 판(Shadow)은 고정 크기라 같이 늘린다
        var shadow = r.Find("Shadow") as RectTransform;
        if (shadow != null)
        {
            if (_shadowHomeH < 0f) _shadowHomeH = shadow.sizeDelta.y;
            shadow.sizeDelta = new Vector2(shadow.sizeDelta.x, _shadowHomeH + (h - _labelHomeSize.y));
        }
    }
    private float _shadowHomeH = -1f;

    // 사망 연출 시작·비활성화 때도 정리
    public void Hide()
    {
        if (!_hovering) return;
        _hovering = false;
        if (nameLabel != null) nameLabel.SetActive(false);
        if (intentTooltip != null) intentTooltip.Hide();
    }

    private void OnDisable() => Hide();
}
