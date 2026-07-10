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
    [SerializeField] private string cardName;
    [SerializeField] public Sprite cardImage;
    [SerializeField] public Sprite CardBackground;
    [SerializeField] private int energyCost;
    [SerializeField] private int ammoCost;
    [SerializeField] private CardType cardType;
    [SerializeField] private CardRarity cardRarity;
    [SerializeField] private CardOwner cardOwner;

    [Header("카드 설명 및 효과")]
    [SerializeField] public string cardDescription;
    [SerializeField] public List<CardEffect> cardEffects = new(); // 카드 효과 - 별도의 ScriptableObject로 처리
    [SerializeField] private CardUseMode useMode = CardUseMode.DropToPlayArea;

    [Header("키워드")]
    [SerializeField] private bool isExhaust;    // 소멸
    [SerializeField] private bool isEthereal;   // 휘발 (손패에 있을때 안 쓰면 소멸)
    [SerializeField] private bool isInnate;     // 선천성. 항상 초기 패에 포함
    [SerializeField] private bool isRetain;     // 턴 넘겨도 유지

    [Header("강화")]
    [SerializeField] private string upgradedName;
    [SerializeField] private int upgradedCost;
    [SerializeField] private bool upgradedIsExhaust; // 강화 후 소멸 여부. 체크 안 하면 강화 시 소멸 해제 (꼼짝마! 등)
    [SerializeField] public bool isUpgraded;
    [SerializeField] private List<CardEffect> upgradedEffects = new();

    [Header("연출")]
    public AnimationClip useAnimation;
    public AudioClip useSFX;

    // --- Public Accessors ---
    public string CardName        => isUpgraded ? upgradedName : cardName;
    public Sprite CardImage       => cardImage;
    public int    EnergyCost      => isUpgraded ? upgradedCost : energyCost;
    public int    AmmoCost        => ammoCost;
    public string CardDescription => GetFullDescription();
    public List<CardEffect> CardEffect  => cardEffects;
    public List<CardEffect> UpgradedEffects => upgradedEffects;
    public IReadOnlyList<CardEffect> ActiveEffects
    {
        get
        {
            if (isUpgraded && upgradedEffects != null && upgradedEffects != null)
                return upgradedEffects;

            return cardEffects != null
                ? cardEffects
                : System.Array.Empty<CardEffect>();
        }
    }
    public CardUseMode UseMode    => useMode;
    public CardType   Type        => cardType;
    public CardRarity Rarity      => cardRarity;
    public CardOwner  Owner       => cardOwner;
    public bool IsExhaust         => isUpgraded ? upgradedIsExhaust : isExhaust;
    public bool IsEthereal        => isEthereal;
    public bool IsInnate          => isInnate;
    public bool IsRetain          => isRetain;

    // cardDescription의 {인덱스.필드명} 토큰을 ActiveEffects[인덱스]의 public 필드값으로 치환한다.
    // 예) "적에게 {0.amount}의 피해를 {0.hitCount}회 줍니다." → "적에게 5의 피해를 3회 줍니다."
    // ActiveEffects가 강화 여부에 따라 다른 리스트를 반환하므로 강화 수치는 자동 반영됨.
    // 템플릿은 기본/강화 공용 하나만 작성하면 된다. 잘못된 토큰(인덱스 초과, 없는 필드)은 원문 그대로 노출.
    public string GetFullDescription()
    {
        if (string.IsNullOrEmpty(cardDescription)) return string.Empty;
        return Regex.Replace(cardDescription, @"\{(\d+)\.(\w+)\}", match =>
        {
            if (!int.TryParse(match.Groups[1].Value, out int idx)) return match.Value;
            var effects = ActiveEffects;
            if (idx >= effects.Count) return match.Value;
            var field = effects[idx].GetType().GetField(match.Groups[2].Value);
            return field?.GetValue(effects[idx])?.ToString() ?? match.Value;
        });
    }

    public enum CardType  { Attack, Skill, Power }
    public enum CardRarity { Common, Rare, Unique }
    public enum CardUseMode { DropToPlayArea, SelectEnemy }
    public enum CardOwner
    {
        Player, Jogasaki, CanadaMarine, GermanDeserter,
        NamibiaPartisan, BrassKnight, ChinaSpecOps,
        Non_Color
    }
}
