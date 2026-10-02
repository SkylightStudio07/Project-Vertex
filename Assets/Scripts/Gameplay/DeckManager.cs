using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CardData;

// 플레이어 덱 관리 전담 (시작 덱 구성, 런 중 카드 획득).
// 로비 등 GameManager(현재 런 상태: HP/골드/적/맵 진행)와 무관한 씬에서도
// 덱만 따로 참조할 수 있도록 분리했다.
//
// 씬 전환: 덱 목록은 static이라 씬을 넘어 유지되고, 컴포넌트는 씬마다 하나씩 있는 창구다(가장 최근 씬 것이 Instance).
// DontDestroyOnLoad를 쓰지 않는다 — 전투 씬에서는 GameManager·BattleManager 등과 같은 오브젝트(GameplayManager)에
// 붙어 있어서, 오브젝트를 살려 두거나 중복이라 지우면 그 씬의 매니저까지 같이 살아남거나 사라진다.
// 덱은 로비에 들어올 때(LobbyManager.Start) 시작 덱으로 다시 만든다.
public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    private static List<CardData> s_playerDeck;

    // 에디터에서 도메인 리로드를 끄고 플레이해도 이전 플레이의 덱이 남지 않게
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { s_playerDeck = null; Instance = null; }

    private void Awake()
    {
        Instance = this;

        // 첫 실행(전투 씬을 에디터에서 바로 켠 경우 포함)에만 시작 덱을 만든다.
        // Awake에 두는 이유: GameManager.Start() → InitializeBattle()이 PlayerDeck을 참조하기 전에 덱이 준비되어야 한다.
        if (s_playerDeck == null) InitializeStartingDeck();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    [Header("기본 덱")]
    [SerializeField] private CardData strikeCard;
    [SerializeField] private CardData blockCard;
    [SerializeField] private CardData reloadCard;

    [Header("디버그")]
    [SerializeField] private CardData strikeCard_Debug;
    [SerializeField] private CardData blockCard_Debug;

    public List<CardData> PlayerDeck
    {
        get => s_playerDeck ??= new List<CardData>();
        private set => s_playerDeck = value;
    }

    // 기본 시작 덱을 구성한다. 로비에 들어올 때마다(LobbyManager.Start) 새 런용으로 다시 만든다.
    public void InitializeStartingDeck()
    {
        // SO 원본이 오염되지 않도록 Instantiate로 복사
        PlayerDeck = new List<CardData>();
        for (int i = 0; i < 5; i++)
        {
            PlayerDeck.Add(Instantiate(strikeCard));
            PlayerDeck.Add(Instantiate(blockCard));
        }
        PlayerDeck.Add(Instantiate(reloadCard));

        // 디버그용 시작 덱. 실제 릴리즈 시에는 이 부분 제거할 것.
        PlayerDeck.Add(Instantiate(strikeCard_Debug));
        PlayerDeck.Add(Instantiate(blockCard_Debug));
    }

    // 플레이어 덱에 획득 카드를 추가하는 메소드. 덱에 들어간 복사본을 돌려준다 (동료 합류 카드 추적용).
    public CardData AddCardToPlayerDeck(CardData card)
    {
        if (card == null) return null;

        var instance = Instantiate(card);
        PlayerDeck.Add(instance);
        if (!TrainingSession.IsActive) PlayerRecord.AddObtainedCard(card); // 한 번이라도 얻은 카드 (훈련장 덱 구성)
        RunStats.AddCard(); // 결과 화면 '획득 카드'
        Debug.Log("플레이어 덱에 카드 추가완료.");
        return instance;
    }

    // 시작 덱 구성 카드인지 (동료 귀환 때 플레이어 기본 카드는 빼지 않기 위해)
    public bool IsStartingCard(CardData card)
    {
        string key = PlayerRecord.KeyOf(card);
        foreach (var c in new[] { strikeCard, blockCard, reloadCard, strikeCard_Debug, blockCard_Debug })
            if (c != null && PlayerRecord.KeyOf(c) == key) return true;
        return false;
    }

    // 조건에 맞는 카드를 덱에서 모두 뺀다 (동료 귀환 — 그 동료 몫의 카드). 뺀 장수를 돌려준다.
    public int RemoveCardsWhere(System.Predicate<CardData> match)
    {
        var removed = PlayerDeck.FindAll(c => c != null && match(c));
        foreach (var card in removed)
        {
            PlayerDeck.Remove(card);
            Destroy(card);
        }
        return removed.Count;
    }

    // 훈련장: 고른 카드 목록으로 덱을 통째로 바꾼다. 획득 기록은 남기지 않는다.
    public void SetPlayerDeck(IEnumerable<CardData> cards)
    {
        PlayerDeck = new List<CardData>();
        if (cards == null) return;
        foreach (var card in cards)
            if (card != null) PlayerDeck.Add(Instantiate(card));
    }
    // 플레이어 덱에서 카드를 제거하는 메소드
    public bool RemoveCardFromPlayerDeck(CardData card)
    {
        if (card == null) return false;

        if (!PlayerDeck.Remove(card))
        {
            Debug.LogWarning($"[DeckManager] 덱에 없는 카드를 제거하려 함: {card.CardName}");
            return false;
        }
        Destroy(card);
        return true;
    }

    // 강화 가능한 카드만 추린다. 목록 표시와 강화 버튼 활성화 판정에 공용으로 쓴다.
    public List<CardData> GetUpgradableCards() => PlayerDeck.FindAll(c => c != null && c.CanUpgrade);

    // 덱의 카드를 강화한다. 덱에는 Instantiate 복사본이 들어있으므로 원본 SO는 영향받지 않는다.
    public bool UpgradeCard(CardData card)
    {
        if (card == null || !card.CanUpgrade) return false;

        if (!PlayerDeck.Contains(card))
        {
            Debug.LogWarning($"[DeckManager] 덱에 없는 카드를 강화하려 함: {card.CardName}");
            return false;
        }

        card.isUpgraded = true;
        return true;
    }

    // 카드 변화 메소드
    public CardData TransmogrifyCard(CardData card, System.Random transRng)
    {
        if (card == null || !PlayerDeck.Contains(card))
        {
            return null;
        }

        List<CardData> allCards = GetTransmogrifiableCards(card);
        if(allCards.Count == 0)
        {
            return null;
        }
        CardData newCard = allCards[transRng.Next(allCards.Count)];

        if(newCard == null)
        {
            return null;
        }
        CardData copy = Instantiate(newCard);

        // 기존 카드의 위치를 유지하기 위해 인덱스를 저장한다.
        int index = PlayerDeck.IndexOf(card);
        RemoveCardFromPlayerDeck(card);
        PlayerDeck.Insert(index, copy);
        // 변화로 들어온 카드도 '얻은 카드'다 (훈련장 덱 구성 · 결과 화면 획득 카드)
        if (!TrainingSession.IsActive) PlayerRecord.AddObtainedCard(newCard);
        RunStats.AddCard();

        return newCard;
    }

    // 변화 후보: 획득 가능한 카드(보상 풀 전 등급) 중 Owner가 같은 카드. 자기 자신은 변화 대상에서 제외한다.
    public List<CardData> GetTransmogrifiableCards(CardData card)
    {
        var result = new List<CardData>();
        if (card == null || GameManager.Instance == null) 
        {
            return result;
        }

        foreach (var rarity in new[] { CardRarity.Common, CardRarity.Rare, CardRarity.Unique })
        {
            if (!GameManager.Instance.cardPools.TryGetValue(rarity, out var pool)) continue;
            foreach (var c in pool)
            {
                if (c != null && c.Owner == card.Owner && c.BaseCardName != card.BaseCardName && !c.IsUnplayable && !c.IsWeaponShootingCard)
                    result.Add(c);
            }
        }
        return result;
    }

    // DECK 버튼이 씬의 이 컴포넌트를 직접 부른다 (덱 목록은 static이라 어느 씬 컴포넌트든 같다)
    public void ViewDeck()
    {
        CardListView.Instance?.OpenAsViewer("플레이어 덱", PlayerDeck);
    }
}
