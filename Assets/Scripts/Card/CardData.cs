// ============================================================
// filename   : CardData.cs
// 작성자    : SkylightStudio07 - 박영서
// 작성일    : 2026-04-12
// description   : 카드 기본 데이터를 정의하는 ScriptableObject
// ============================================================
// 업데이트 로그
// ------------------------------------------------------------
// 2026-04-12 | 박영서 | 최초 작성.
// 카드 효과를 별도의 ScriptableObject로 분리하는 방향으로 설계 변경. 헤더 컨벤션은 나중에 쓸지말지 결정...
// 2026-05-23 | 최성제 | 카드 기본 효과가 리스트가 아니였던 것 수정. 모든 카드가 강화 안하면 몽둥이질과 무적이 될 뻔함ㅋㅋㅋ
// 카드의 사용 방법도 추가함. 타켓 방식인지, 아니면 타깃이 필요 없는지...
// 2026-07-11 | 박영서 | 카드 이펙트를 SO 참조 → [SerializeReference] 인라인으로 전환.
// 수치 조합마다 이펙트 에셋이 늘어나는 문제 해소. 이름·코스트·키워드는 CardUpgradeState로 통합.
// 구 루트 필드(이름·코스트·키워드·이펙트 참조)는 전부 삭제 — 카드 데이터 원본은 노션 DB 기준,
// 에셋은 새 구조로 재작성한다.
// ============================================================


using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Game Asset/Card")]
public class CardData : ScriptableObject
{
    /*
    무슨 프로퍼티 넣을지 생각 또 생각
    이걸 기획서에 썼어야 하는데... 모르겠다.

    1. 카드 이름
    2. 에너지 코스트
    3. 탄약 코스트
    4. 카드 이미지
    5. 카드 타입
    6. 희귀도
    7. 소유자 - 무색, 합류 캐릭터 구분용
    8. 카드 설명 - 하단 설명문
    9. 카드 효과
    
    ---

    누군가 볼 지도 모르는 내 슈퍼 머리굴리기.

    처음에는 단순 상속구조로 처리하려고 했는데...

    그, 스킬 카드에서도 딜 넣고 하는 카드도 있고 하니... 카드 효과를 SO로 독립시킨다.

    스킬 효과가 가지각색인걸 생각하면 그냥 이 쪽이 낫다.

    슬더스 1편이 왜 스파게티 코드라는줄은 알겠네...

    카드 효과에서 따로 처리한다고 쳐도, 디버프 쪽은 범용 함수에서 처리하고 디버프도 따로 SO로 만드는게 나을지도.

    어렵네 ㅅㅂ

    일단 광역기는 카드 효과에서 처리시키고, 휘발성이나 소멸 카드는 이 스크립트에서 처리하는 게 나아보인다.


     */

    [Header("기본 정보")]
    [SerializeField] public Sprite cardImage;
    [SerializeField] public Sprite CardBackground;
    [SerializeField] private CardType cardType;
    [SerializeField] private CardRarity cardRarity;
    [SerializeField] private CardOwner cardOwner;

    [Tooltip("현재 무기에 따라 카드 전체가 교체되는 사격 카드입니다. 일반 공격 카드에는 사용하지 않습니다.")]
    [SerializeField] private bool isWeaponShootingCard;
    public bool IsWeaponShootingCard => isWeaponShootingCard;

    // 무기 변형도 다음 무기 변경의 대상이며, 장별 강화 상태는 원래 카드를 따른다.
    public CardData CreateWeaponVariant(CardData template)
    {
        var copy = Instantiate(template);
        copy.isWeaponShootingCard = true;
        copy.isUpgraded = isUpgraded;
        return copy;
    }

    [Header("카드 설명 및 효과")]
    // {인덱스.필드명} 토큰 지원 — GetFullDescription() 참고. 기본/강화 공용 템플릿 하나만 작성.
    // EffectTokenHint가 인스펙터에 사용 가능 토큰 목록과 치환 미리보기를 띄워준다.
    [SerializeField, EffectTokenHint("normalState.effects")] public string cardDescription;
    // 강화로 문구 자체가 바뀌는 카드용 (예: "다음 카드" → "다음 두 카드", "무작위로" → "선택해서").
    // 비워 두면 강화 상태도 cardDescription을 쓴다. 토큰은 강화 상태의 이펙트 기준으로 치환된다.
    [Tooltip("비우면 기본 설명을 강화에도 사용합니다. 강화로 문구가 바뀌는 카드만 작성하세요.")]
    [SerializeField, EffectTokenHint("upgradedState.effects", nameof(cardDescription))] public string upgradedDescription;
    public bool HasUpgradedDescription => !string.IsNullOrEmpty(upgradedDescription);
    [SerializeField] private CardUseMode useMode = CardUseMode.DropToPlayArea;
    [Tooltip("사용 조건. 비우면 언제나 쓸 수 있다 (예: 회수 — 특정 카드가 소멸 더미에 있을 때만)")]
    [SerializeReference, SubclassPicker] private CardCondition playCondition;

    [Header("기본/강화 상태")]
    // 이름·코스트·키워드·이펙트는 상태별로 CardUpgradeState에 담는다. 이펙트는 SO 참조가 아니라
    // 인라인 직렬화 — 상태 폴드아웃 안에서 타입 선택 후 수치를 직접 입력한다.
    // isUpgraded는 런타임 강화 스위치. 덱에는 Instantiate 복사본이 들어가므로 장별 강화가 가능하지만,
    // 원본 에셋에 체크한 채 커밋하면 해당 카드는 획득 시점부터 강화 상태가 되니 주의.
    [SerializeField] public bool isUpgraded;
    [SerializeField] private CardUpgradeState normalState;
    [SerializeField] private CardUpgradeState upgradedState;

    [Header("연출")]
    public AnimationClip useAnimation;
    public AudioClip useSFX;

    // --- Public Accessors ---
    private CardUpgradeState ActiveState => isUpgraded ? upgradedState : normalState;
    public string CardName        => ActiveState.cardName;
    public string BaseCardName => normalState.cardName;
    public Sprite CardImage       => cardImage;
    public int    EnergyCost      => ActiveState.energyCost;
    public int    AmmoCost        => ActiveState.ammoCost;
    public string CardDescription => GetFullDescription(); // 토큰 치환 결과. 원문 템플릿은 cardDescription
    public List<CardEffect> CardEffect  => normalState.effects;
    public List<CardEffect> UpgradedEffects => upgradedState.effects;
    public IReadOnlyList<CardEffect> ActiveEffects
    {
        get
        {
            if (ActiveState.effects != null)
                return ActiveState.effects;

            return normalState.effects != null
                ? normalState.effects
                : System.Array.Empty<CardEffect>();
        }
    }
    public CardUseMode UseMode    => useMode;
    // 저주 등 사용 불가 카드. 손패에는 남지만 플레이가 막힌다(드래그해서 놓아도 사용되지 않음).
    public bool IsUnplayable      => useMode == CardUseMode.Unplayable;
    // target: 적 지정 카드면 그 적 기준으로 조건을 본다 (예: 대상에게 취약·약화·중독이 모두 있을 때만)
    public bool IsPlayConditionMet(BattleState state, EnemyInstance target = null) => playCondition == null || playCondition.IsMet(state, this, target);
    public bool HasPlayCondition => playCondition != null;
    public CardType   Type        => cardType;
    public CardRarity Rarity      => cardRarity;
    public CardOwner  Owner       => cardOwner;
    public bool IsExhaust         => ActiveState.isExhaust;
    public bool IsEthereal        => ActiveState.isEthereal;
    public bool IsInnate          => ActiveState.isInnate;
    public bool IsRetain          => ActiveState.isRetain;

    // 강화 가능 여부. upgradedState가 비어 있으면(미작성 카드) 강화 시 이름·효과가 사라지므로 제외한다.
    public bool CanUpgrade => !isUpgraded
                              && upgradedState.HasEffects
                              && !string.IsNullOrEmpty(upgradedState.cardName);

    // cardDescription의 {인덱스.필드명} 토큰을 ActiveEffects[인덱스]의 public 필드값으로 치환한다.
    // 예) "적에게 {0.amount}의 피해를 {0.hitCount}회 줍니다." → "적에게 5의 피해를 3회 줍니다."
    // 안쪽 리스트는 숫자 조각으로 연다: "초탄이면 방어도를 {1.OnMet.0.amount} 얻습니다." (OnMet = effectsWhenMet 별칭)
    // ActiveEffects가 강화 여부에 따라 다른 리스트를 반환하므로 강화 수치는 자동 반영됨.
    // 템플릿은 기본/강화 공용 하나만 작성하면 된다. 잘못된 토큰(인덱스 초과, 없는 필드)은 원문 그대로 노출.
    //
    // state를 넘기면(전투 중 손패 표시) int 수치를 이펙트의 GetDisplayValue로 한 번 더 통과시켜
    // 힘·민첩 등 패시브 보정이 반영된 값으로 표시한다. state가 null이면(보상·덱 화면) 원시값 그대로.
    // target까지 넘기면(타겟팅 드래그 중) 취약·버퍼 등 대상 측 보정도 반영된다.
    public string GetFullDescription(BattleState state = null, EnemyInstance target = null)
        => GetDescription(isUpgraded, state, target);

    // ---- 강화 전/후 비교용 (강화 표시: CardUpgradeHighlight) ----
    // 현재 isUpgraded와 무관하게 지정한 상태 기준으로 계산한다. isUpgraded를 잠깐 뒤집지 않아도 되게.
    private CardUpgradeState StateOf(bool upgraded) => upgraded ? upgradedState : normalState;
    private IReadOnlyList<CardEffect> EffectsOf(bool upgraded)
    {
        var effects = StateOf(upgraded).effects;
        if (effects != null) return effects;
        return normalState.effects != null ? normalState.effects : System.Array.Empty<CardEffect>();
    }
    public int GetEnergyCost(bool upgraded) => StateOf(upgraded).energyCost;
    public int GetAmmoCost(bool upgraded)   => StateOf(upgraded).ammoCost;
    public bool IsEnergyCostX(bool upgraded) => StateOf(upgraded).energyCostIsX;
    public bool IsAmmoCostX(bool upgraded)   => StateOf(upgraded).ammoCostIsX;
    // 비용 칸 표시용 문자열 (X 비용이면 "X")
    public string EnergyCostLabel => ActiveState.energyCostIsX ? "X" : EnergyCost.ToString();
    public string AmmoCostLabel   => ActiveState.ammoCostIsX ? "X" : AmmoCost.ToString();

    // 상태 플래그로 정해지는 키워드 (표시 이름). 설명문 조립은 GetDescription이 하고, 이 목록은 조회용.
    public List<string> GetKeywords(bool upgraded)
    {
        var st = StateOf(upgraded);
        var list = new List<string>();
        if (st.isInnate)   list.Add("선천성");
        if (st.isRetain)   list.Add("보존");
        if (st.isEthereal) list.Add("휘발성");
        if (st.isExhaust && Type != CardType.Power) list.Add("소멸"); // 파워는 플래그와 무관하게 항상 소멸하므로 표시하지 않는다
        var traits = EffectiveTraitsOf(upgraded);
        if (traits != EnemyTrait.None) list.Add(EnemyTraits.Label(traits) + " 특효");
        return list;
    }

    private const string BuffedValueColor = "#6FE38A";   // 보정으로 오른 수치
    private const string DebuffedValueColor = "#FF6B5E"; // 보정으로 내린 수치

    // 설명문 최종 조립: [선천성, 휘발성.] [○○ 특효.] 본문 [소멸, 보존.]
    // 특수 키워드(선천성·휘발성·소멸·보존)는 카드 자체의 상태 플래그로 자동 표기한다 — 설명문에 직접 적지 않는다.
    // (효과 문장 속의 "카드 1장을 소멸" 같은 표현은 키워드가 아니므로 설명문에 그대로 쓴다)
    public string GetDescription(bool upgraded, BattleState state = null, EnemyInstance target = null)
    {
        string text = ResolveDescription(upgraded, state, target);
        text = PrependTraitKeyword(text, upgraded, target);

        var st = StateOf(upgraded);
        string front = JoinKeywords(st.isInnate ? "선천성" : null, st.isEthereal ? "휘발성" : null);
        // 파워는 플래그와 무관하게 항상 소멸하므로 소멸을 표시하지 않는다
        string back = JoinKeywords(st.isExhaust && Type != CardType.Power ? "소멸" : null, st.isRetain ? "보존" : null);

        var sb = new System.Text.StringBuilder();
        if (front != null) sb.Append(front);
        if (!string.IsNullOrEmpty(text)) { if (sb.Length > 0) sb.Append(' '); sb.Append(text); }
        if (back != null) { if (sb.Length > 0) sb.Append(' '); sb.Append(back); }
        return sb.ToString();
    }

    // 특효 키워드("생체·신비 특효")는 설명문 템플릿에 적혀 있지 않으면 앞에 자동으로 붙인다.
    // 겨냥 중인 적에게 특효가 걸리면 강조색(청록)으로 칠한다.
    private string PrependTraitKeyword(string text, bool upgraded, EnemyInstance target)
    {
        var traits = EffectiveTraitsOf(upgraded);
        if (traits == EnemyTrait.None) return text;
        string keyword = EnemyTraits.Label(traits) + " 특효";
        if (text.Contains(keyword)) return text;
        // 속성이 아직 공개되지 않은 적(기록 2편 전)에게는 강조하지 않는다 — 강조로 속성이 드러나므로
        if (target != null && EnemyTraits.HasAny(target, traits) && MaterialArchive.TraitsRevealed(target.Data)) keyword = $"<color={EffectiveKeywordColor}>{keyword}</color>";
        return string.IsNullOrEmpty(text) ? keyword + "." : keyword + ". " + text;
    }

    // "소멸." / "소멸, 보존." — 키워드가 없으면 null
    private static string JoinKeywords(string first, string second)
    {
        if (first == null && second == null) return null;
        if (first == null || second == null) return (first ?? second) + ".";
        return first + ", " + second + ".";
    }

    private const string EffectiveKeywordColor = "#0DB8F2";

    public EnemyTrait EffectiveTraitsOf(bool upgraded)
    {
        var traits = EnemyTrait.None;
        foreach (var effect in EffectsOf(upgraded))
            if (effect is DamageEffect damage) traits |= damage.effectiveAgainst;
        return traits;
    }

    // 설명문 토큰에서 긴 필드명 대신 쓸 수 있는 짧은 이름. 필드명은 에셋에 저장되므로 바꾸지 않고 토큰에서만 바꿔 읽는다.
    // 예) {1.OnMet.0.amount} == {1.effectsWhenMet.0.amount}
    private static readonly Dictionary<string, string> TokenAliases = new()
    {
        { "OnMet", nameof(ConditionalEffect.effectsWhenMet) },
    };
    public static string TokenAliasToField(string name) => TokenAliases.TryGetValue(name, out var field) ? field : name;
    public static string FieldToTokenAlias(string field)
    {
        foreach (var pair in TokenAliases) if (pair.Value == field) return pair.Key;
        return field;
    }

    private string ResolveDescription(bool upgraded, BattleState state, EnemyInstance target)
    {
        string template = upgraded && HasUpgradedDescription ? upgradedDescription : cardDescription;
        if (string.IsNullOrEmpty(template)) return string.Empty;
        var effects = EffectsOf(upgraded);
        return Regex.Replace(template, @"\{(\d+)\.([\w.]+)\}", match =>
        {
            if (!int.TryParse(match.Groups[1].Value, out int idx)) return match.Value;
            if (idx >= effects.Count) return match.Value;

            var effect = effects[idx];
            if (effect == null) return match.Value; // 슬롯은 늘렸지만 아직 타입을 안 고른 빈 이펙트

            // 경로 조각이 숫자면 리스트 인덱스로 본다 — 조건부·처치 시 효과처럼 안쪽 리스트에 든 수치용.
            // 예) {1.effectsWhenMet.0.amount}, {0.conditionalBonuses.0.amount}
            // 보정 표시는 그 수치를 직접 가진 이펙트에 맡긴다 (조건부 안의 방어도면 그 BlockEffect의 민첩 보정).
            // 이펙트가 아닌 하위 객체의 수치(scaling.multiplier, conditionalBonuses.0.amount 등)는 원시값 그대로.
            object value = effect;
            CardEffect fieldOwner = null; // 마지막 필드를 직접 가진 이펙트
            string finalFieldName = null;
            foreach (string memberName in match.Groups[2].Value.Split('.'))
            {
                if (value == null) return match.Value;
                if (int.TryParse(memberName, out int listIndex))
                {
                    if (value is not System.Collections.IList list || listIndex < 0 || listIndex >= list.Count)
                        return match.Value;
                    value = list[listIndex];
                    fieldOwner = null;
                    finalFieldName = null;
                    continue;
                }

                var valueType = value.GetType();
                var field = valueType.GetField(TokenAliasToField(memberName));
                var property = field == null ? valueType.GetProperty(memberName) : null;
                if (field == null && property == null) return match.Value;
                fieldOwner = value as CardEffect;
                finalFieldName = memberName;
                value = field != null ? field.GetValue(value) : property.GetValue(value);
            }
            if (value == null) return match.Value;

            if (state != null && value is int intValue && fieldOwner != null)
            {
                // 힘·죄와 벌·민첩·약화 등 보정이 반영된 최종 수치. 보정으로 바뀐 값은 색으로 알려 준다(슬더스 방식)
                int shown = fieldOwner.GetDisplayValue(finalFieldName, intValue, state, this, target);
                if (shown > intValue) return $"<color={BuffedValueColor}>{shown}</color>";
                if (shown < intValue) return $"<color={DebuffedValueColor}>{shown}</color>";
                return shown.ToString();
            }

            return value.ToString();
        });
    }

    public enum CardType  { Attack, Skill, Power, Status }
    public enum CardRarity { Common, Rare, Unique }
    // Unplayable = 저주 등 사용 불가 카드. CardHandler.TryDrop의 default가 사용을 막아준다.
    // 주의: 값이 에셋에 정수로 저장되므로 새 항목은 반드시 끝에 추가할 것(기존 카드는 0/1).
    public enum CardUseMode { DropToPlayArea, SelectEnemy, Unplayable }
    public enum CardOwner
    {
        Player, Jogasaki, CanadaMarine, GermanDeserter,
        NamibiaPartisan, BrassKnight, ChinaSpecOps,
        Non_Color,
        Alchemist
    }
}
