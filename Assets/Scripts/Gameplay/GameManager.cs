using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;



public enum GamePhase { Battle, Map, Reward, Shop, Event }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GamePhase Phase { get; private set; } = GamePhase.Battle;

    public void SetPhase(GamePhase phase) => Phase = phase;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<EnemyData> currentEnemies; // 현재 전투에 참여하는 적들의 데이터 리스트

    [Header("보상 풀")]
    // 런 시작 시 기본 보상 풀 SO. 캐릭터 합류 시 CoopCharData 풀이 여기에 합산된다.
    [SerializeField] private PlayerRewardPoolSO playerRewardPool;
    // 런타임 전체 보상 풀 — 기본 풀 복사본으로 시작해 런 중 동적으로 카드가 추가된다.
    // SO 원본 리스트를 직접 참조하면 런 간 데이터가 누적되므로 InitializeRun에서 복사본을 만든다.
    public Dictionary<CardData.CardRarity, List<CardData>> cardPools = new Dictionary<CardData.CardRarity, List<CardData>>();
    // 아이템 보상 풀
    [SerializeField] private List<ItemData> itemPool;
    // 챕터, 전투 유형 별 카드 보상 확률 데이터
    [SerializeField] private List<RewardProbabilityData> rewardProbabilityTable;

    [Header("플레이어 HP")]
    [SerializeField] private int maxPlayerHP = 80; // 기획서 10.1 기준
    public int MaxPlayerHP => maxPlayerHP;
    public int PlayerHP    { get; private set; }

    [Header("플레이어 골드")]
    [SerializeField] private int playerGold = 0;
    public int PlayerGold
    {
        get => playerGold;
        set => playerGold = Mathf.Max(0, value);
    }

    [Header("막, 층")]
    [SerializeField] private int chapter = 1;
    [SerializeField] private int floor   = 1;


    void Start()
    {
        InitializeRun();
    }

    void Update()
    {

    }

    // 백날 게임매니저 시작 덱에 카드 넣어봤자 테스트 안 된다!
    // 플레이어 시작 덱은 여기서 Instantiate해서 따로 처리한다는 것.

    void InitializeRun()
    {
        chapter = 1;
        PlayerHP = maxPlayerHP;

        // 카드 풀 초기화 — SO 원본이 아닌 복사본으로 시작해야 런 간 데이터 누적을 막는다.
        cardPools[CardData.CardRarity.Common] = playerRewardPool != null ? new List<CardData>(playerRewardPool.commonCards) : new List<CardData>();
        cardPools[CardData.CardRarity.Rare]   = playerRewardPool != null ? new List<CardData>(playerRewardPool.rareCards)   : new List<CardData>();
        cardPools[CardData.CardRarity.Unique] = playerRewardPool != null ? new List<CardData>(playerRewardPool.uniqueCards) : new List<CardData>();

        // 아이템 풀 초기화
        BattleReward.SetItemRewardsPool(itemPool);

        // 새 런 시작 시 이전 판 아이템이 남지 않도록 인벤토리 초기화
        if (ItemInventoryManager.Instance != null)
            ItemInventoryManager.Instance.Clear();

        MapManager.Instance.InitializeMap();

        InitializeBattle();
    }

    // 매 전투 시작 시 호출.
    // 전투 전용 덱 복사/셔플은 BattleManager가 담당한다.
    public void InitializeBattle()
    {
        BattleType battleType = RunData.Instance.CurrentNodeType switch
        {
            NodeType.Elite => BattleType.Elite,
            NodeType.Boss  => BattleType.Boss,
            _              => BattleType.Normal,
        };

        // 노드에 조우가 할당돼 있으면 그걸 쓰고, 없으면 Inspector 기본값(currentEnemies) 사용
        var node = RunData.Instance.CurrentNode;
        var enemies = (node?.encounter != null && node.encounter.Count > 0)
            ? node.encounter
            : currentEnemies;

        BattleManager.Instance.StartBattle(enemies, DeckManager.Instance.PlayerDeck, RunData.Instance.mapData.seed, battleType);
        BattleManager.Instance.PlayerTurnStart(false); // 전투 첫 진입이라 턴 배너는 건너뜀
    }

    // 단일 카드를 카드 자체의 Rarity에 맞는 풀에 추가. unlockCardCoopLevel 등 단건 추가용.
    public void AddCardToRewardPool(CardData card)
    {
        if (card == null) return;
        if (!cardPools.TryGetValue(card.Rarity, out var pool))
        {
            pool = new List<CardData>();
            cardPools[card.Rarity] = pool;
        }
        if (!pool.Contains(card))
            pool.Add(card);
    }

    // 등급별 리스트 일괄 추가. CoopCharData의 rewardPoolCommon/Rare/Unique 합산용.
    public void AddCardsToRewardPool(List<CardData> cards, CardData.CardRarity rarity)
    {
        if (cards == null || cards.Count == 0) return;
        if (!cardPools.TryGetValue(rarity, out var pool))
        {
            pool = new List<CardData>();
            cardPools[rarity] = pool;
        }
        foreach (var card in cards)
            if (card != null && !pool.Contains(card))
                pool.Add(card);
    }

    public void TakeDamage(int amount)
    {
        PlayerHP = Mathf.Max(0, PlayerHP - amount);
    }

    public void HealPlayer(int amount)
    {
        PlayerHP = Mathf.Min(maxPlayerHP, PlayerHP + amount);
    }

    public bool IsPlayerDead() => PlayerHP <= 0;

    public RewardProbabilityData GetRewardProbability(BattleType battleType)
    {
        return rewardProbabilityTable.Find(t => t.Chapter == chapter && t.BattleType == battleType);
    }
}
