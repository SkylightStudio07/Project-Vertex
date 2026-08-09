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

    [Header("카드 설명 및 효과")]
    // {인덱스.필드명} 토큰 지원 — GetFullDescription() 참고. 기본/강화 공용 템플릿 하나만 작성.
    // EffectTokenHint가 인스펙터에 사용 가능 토큰 목록과 치환 미리보기를 띄워준다.
    [SerializeField, EffectTokenHint("normalState.effects")] public string cardDescription;
    [SerializeField] private CardUseMode useMode = CardUseMode.DropToPlayArea;

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
    public CardType   Type        => cardType;
    public CardRarity Rarity      => cardRarity;
    public CardOwner  Owner       => cardOwner;
    public bool IsExhaust         => ActiveState.isExhaust;
    public bool IsEthereal        => ActiveState.isEthereal;
    public bool IsInnate          => ActiveState.isInnate;
    public bool IsRetain          => ActiveState.isRetain;

    // cardDescription의 {인덱스.필드명} 토큰을 ActiveEffects[인덱스]의 public 필드값으로 치환한다.
    // 예) "적에게 {0.amount}의 피해를 {0.hitCount}회 줍니다." → "적에게 5의 피해를 3회 줍니다."
    // ActiveEffects가 강화 여부에 따라 다른 리스트를 반환하므로 강화 수치는 자동 반영됨.
    // 템플릿은 기본/강화 공용 하나만 작성하면 된다. 잘못된 토큰(인덱스 초과, 없는 필드)은 원문 그대로 노출.
    //
    // state를 넘기면(전투 중 손패 표시) int 수치를 이펙트의 GetDisplayValue로 한 번 더 통과시켜
    // 힘·민첩 등 패시브 보정이 반영된 값으로 표시한다. state가 null이면(보상·덱 화면) 원시값 그대로.
    // target까지 넘기면(타겟팅 드래그 중) 취약·버퍼 등 대상 측 보정도 반영된다.
    public string GetFullDescription(BattleState state = null, EnemyInstance target = null)
    {
        if (string.IsNullOrEmpty(cardDescription)) return string.Empty;
        return Regex.Replace(cardDescription, @"\{(\d+)\.(\w+)\}", match =>
        {
            if (!int.TryParse(match.Groups[1].Value, out int idx)) return match.Value;
            var effects = ActiveEffects;
            if (idx >= effects.Count) return match.Value;

            var effect = effects[idx];
            if (effect == null) return match.Value; // 슬롯은 늘렸지만 아직 타입을 안 고른 빈 이펙트

            var field = effect.GetType().GetField(match.Groups[2].Value);
            object value = field?.GetValue(effect);
            if (value == null) return match.Value;

            if (state != null && value is int intValue)
                return effect.GetDisplayValue(field.Name, intValue, state, this, target).ToString();

            return value.ToString();
        });
    }

    public enum CardType  { Attack, Skill, Power, Status }
    public enum CardRarity { Common, Rare, Unique }
    public enum CardUseMode { DropToPlayArea, SelectEnemy }
    public enum CardOwner
    {
        Player, Jogasaki, CanadaMarine, GermanDeserter,
        NamibiaPartisan, BrassKnight, ChinaSpecOps,
        Non_Color
    }
}
