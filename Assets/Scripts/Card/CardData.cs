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
// ============================================================


using System.Collections.Generic;
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
    [SerializeField] public CardEffect cardEffect; // 카드 효과 - 별도의 ScriptableObject로 처리

    [Header("키워드")]
    [SerializeField] private bool isExhaust;    // 소멸
    [SerializeField] private bool isEthereal;   // 휘발 (손패에 있을때 안 쓰면 소멸)
    [SerializeField] private bool isInnate;     // 선천성. 항상 초기 패에 포함
    [SerializeField] private bool isRetain;     // 턴 넘겨도 유지

    [Header("강화")]
    [SerializeField] private string upgradedName;
    [SerializeField] private int upgradedCost;
    [SerializeField] public bool isUpgraded;
    [SerializeField] private List<CardEffect> upgradedEffects;

    [Header("연출")]
    public AnimationClip useAnimation;
    public AudioClip useSFX;

    // --- Public Accessors ---
    public string CardName        => isUpgraded ? upgradedName : cardName;
    public Sprite CardImage       => cardImage;
    public int    EnergyCost      => isUpgraded ? upgradedCost : energyCost;
    public int    AmmoCost        => ammoCost;
    public string CardDescription => cardDescription;
    public CardEffect CardEffect  => cardEffect;
    public List<CardEffect> UpgradedEffects => upgradedEffects;
    public CardType   Type        => cardType;
    public CardRarity Rarity      => cardRarity;
    public CardOwner  Owner       => cardOwner;
    public bool IsExhaust         => isExhaust;
    public bool IsEthereal        => isEthereal;
    public bool IsInnate          => isInnate;
    public bool IsRetain          => isRetain;

    public enum CardType  { Attack, Skill, Power }
    public enum CardRarity { Common, Rare, Unique }
    public enum CardOwner
    {
        Player, Jogasaki, CanadaMarine, GermanDeserter,
        NamibiaPartisan, BrassKnight, ChinaSpecOps,
        Non_Color
    }
}
