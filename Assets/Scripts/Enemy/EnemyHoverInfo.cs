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

        if (nameText != null) nameText.text = enemy.Data != null ? enemy.Data.enemyName : string.Empty;
        if (nameLabel != null) nameLabel.SetActive(true);
        if (intentTooltip != null) intentTooltip.Show();
    }

    public void OnPointerExit(PointerEventData eventData) => Hide();

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
