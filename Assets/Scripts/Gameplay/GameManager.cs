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

    [Header("덱 관련")]
    public List<CardData> playerDeck; // 플레이어가 얻은 전체 카드덱 (런 내내 지속)

    [Header("카드 풀")]
    // 플레이어가 보상으로 획득 가능한 카드들.
    // 지금은 SerializeField로 개별 리스트 만들어서 채우는 방식인데, 나중에 카드 데이터 관리 시스템 구축하면 그쪽에서 관리하도록 바꿔야 함.
    public Dictionary<CardData.CardRarity, List<CardData>> cardPools = new Dictionary<CardData.CardRarity, List<CardData>>();
    [SerializeField] private List<CardData> commonPool;
    [SerializeField] private List<CardData> rarePool;
    [SerializeField] private List<CardData> uniquePool;
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

    [Header("기본 덱")]
    [SerializeField] private CardData strikeCard;
    [SerializeField] private CardData blockCard;
    [SerializeField] private CardData reloadCard;

    [Header("디버그")]
    [SerializeField] private CardData strikeCard_Debug;
    [SerializeField] private CardData blockCard_Debug;




    void Start()
    {
        Instance = this;
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

        // SO 원본이 오염되지 않도록 Instantiate로 복사
        playerDeck = new List<CardData>();
        for (int i = 0; i < 5; i++)
        {
            playerDeck.Add(Instantiate(strikeCard));
            playerDeck.Add(Instantiate(blockCard));
        }
        playerDeck.Add(Instantiate(reloadCard));

        // 디버그용 시작 덱
        // 실제 릴리즈 시에는 이 부분 제거할 것
        playerDeck.Add(Instantiate(strikeCard_Debug));
        playerDeck.Add(Instantiate(blockCard_Debug));
        
        // 카드 풀 초기화
        cardPools[CardData.CardRarity.Common] = commonPool;
        cardPools[CardData.CardRarity.Rare] = rarePool;
        cardPools[CardData.CardRarity.Unique] = uniquePool;

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

        BattleManager.Instance.StartBattle(enemies, playerDeck, RunData.Instance.mapData.seed, battleType);
        BattleManager.Instance.PlayerTurnStart();
    }


    // 플레이어 덱에 획득 카드를 추가하는 메소드
    public void AddCardToPlayerDeck(CardData card)
    {
        if (card == null) return;

        playerDeck.Add(Instantiate(card));
        Debug.Log("플레이어 덱에 카드 추가완료.");
    }

    // 카드 등급에 맞는 전투 보상 카드 풀에 카드를 추가하는 메소드
    public void AddCardToRewardPool(CardData card)
    {
        if (card == null) return;

        if (!cardPools.TryGetValue(card.Rarity, out var pool))
        {
            pool = new List<CardData>();
            cardPools[card.Rarity] = pool;
        }

        if (!pool.Contains(card))
        {
            pool.Add(card);
            Debug.Log("카드 풀에 카드 추가 완료.");
        }
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
