using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

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

    [Header("플레이어 HP")]
    [SerializeField] private int maxPlayerHP = 80; // 기획서 10.1 기준
    public int MaxPlayerHP => maxPlayerHP;
    public int PlayerHP    { get; private set; }

    [Header("막, 층")]
    [SerializeField] private int chapter = 1;
    [SerializeField] private int floor   = 1;

    [Header("기본 덱")]
    [SerializeField] private CardData strikeCard;
    [SerializeField] private CardData blockCard;




    void Start()
    {
        Instance = this;
        InitializeRun();
    }

    void Update()
    {

    }

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

        MapManager.Instance.InitializeMap();

        InitializeBattle();
    }

    // 매 전투 시작 시 호출.
    // 전투 전용 덱 복사/셔플은 BattleManager가 담당한다.
    public void InitializeBattle()
    {
        BattleManager.Instance.StartBattle(currentEnemies, playerDeck);
        BattleManager.Instance.TakeOutCardtoHand();
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
}
