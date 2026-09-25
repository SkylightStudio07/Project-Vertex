// ============================================================
// filename   : CardView.cs
// description   : CardData를 카드 UI 프리팹에 바인딩하는 뷰 컴포넌트.
//             카드 프리팹 루트에 부착하고, Inspector에서 각 UI 슬롯을 연결.
// ============================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("카드 텍스트")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI energyCostText;
    [SerializeField] private TextMeshProUGUI ammoCostText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    // 카드 종류(공격/스킬/파워/상태). Player Card V3 프레임의 하단 헤더 바에 표시한다.
    // 비워두면 표시하지 않는다 — 프레임에 해당 자리가 없는 프리팹도 그대로 동작해야 해서 선택 필드로 둔다.
    [SerializeField] private TextMeshProUGUI typeText;

    [Header("카드 이미지 - 배경, 아트워크")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private Image backgroundImage;

    public CardData Data { get; private set; }

    // 강화된 카드는 이름과, 강화로 달라진 코스트를 강조색으로 칠한다 (설명문은 CardUpgradeHighlight).
    private static readonly Color UpgradeNameColor = new(1f, 0.32f, 0.28f, 1f);   // 검은 이름 바 위
    private static readonly Color UpgradeCostColor = new(0.82f, 0.16f, 0.14f, 1f); // 흰 코스트 원 위
    private Color _nameColor, _energyColor, _ammoColor;
    private bool _hasBaseColors;

    public void SetCard(CardData card)
    {
        Data = card;

        if (!_hasBaseColors)
        {
            _nameColor = nameText.color;
            _energyColor = energyCostText.color;
            _ammoColor = ammoCostText.color;
            _hasBaseColors = true;
        }

        nameText.text        = card.CardName;
        energyCostText.text  = card.EnergyCost.ToString();
        ammoCostText.text    = card.AmmoCost.ToString();

        bool up = card.isUpgraded;
        nameText.color       = up ? UpgradeNameColor : _nameColor;
        energyCostText.color = up && card.GetEnergyCost(true) != card.GetEnergyCost(false) ? UpgradeCostColor : _energyColor;
        ammoCostText.color   = up && card.GetAmmoCost(true) != card.GetAmmoCost(false) ? UpgradeCostColor : _ammoColor;
        if (typeText != null) typeText.text = GetTypeLabel(card.Type);
        RefreshDescription();

        artworkImage.sprite  = card.CardImage;
        artworkImage.enabled = card.CardImage != null; // 카드 이미지 없으면 컴포넌트 끄기. 퍼포먼스에 영향 있으면 차후 제거하셈

        backgroundImage.sprite = card.CardBackground;
        backgroundImage.enabled = card.CardBackground != null;
    }

    // 설명문 텍스트만 다시 그린다.
    // 전투 중에는 힘·민첩 등 패시브 보정이 반영된 수치로 표시하고,
    // 보상·덱 화면 등 비전투 맥락에서는 state가 null이라 원시값 그대로 나온다.
    // target을 넘기면(타겟팅 드래그 중, CardHandler가 호출) 취약·버퍼 등 대상 측 보정까지 반영.
    public void RefreshDescription(EnemyInstance target = null)
    {
        if (Data == null) return;

        BattleState battleState = BattleManager.Instance != null && BattleManager.Instance.IsInBattle
            ? BattleManager.Instance.State
            : null;
        descriptionText.text = CardUpgradeHighlight.Describe(Data, battleState, target);
    }

    private static string GetTypeLabel(CardData.CardType type) => type switch
    {
        CardData.CardType.Attack => "공격",
        CardData.CardType.Skill  => "스킬",
        CardData.CardType.Power  => "파워",
        CardData.CardType.Status => "상태",
        _                        => string.Empty,
    };
}
