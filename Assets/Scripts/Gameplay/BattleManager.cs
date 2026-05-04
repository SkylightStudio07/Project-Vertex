using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("플레이어 전투  스테이터스")]
    [SerializeField] private int Energy      = 3;
    [SerializeField] private int MaxEnergy   = 3;
    [SerializeField] private int Ammo        = 3;
    [SerializeField] private int PlayerBlock = 0;
    [SerializeField] private int DrawCount = 5;

    [Header("플레이어 카드 더미")]
    [SerializeField] private List<CardData> drawPile    = new();
    [SerializeField] private List<CardData> hand        = new();
    [SerializeField] private List<CardData> discardPile = new();
    [SerializeField] private List<CardData> exhaustPile = new();

    // UI용으로 읽기 전용 손패 리스트 빼놓은 거.
    public IReadOnlyList<CardData> Hand => hand;

    // 손패가 바뀔 때마다 HandView가 구독해서 화면 갱신
    public event Action OnHandChanged;

    // EnemyInstance는 plain C# 클래스라 Inspector엔 안 뜸 — 런타임 전용
    private readonly List<EnemyInstance> enemies = new();
    // 이건 필요할지 모르겠는데, 일단 해 둠.
    public IReadOnlyList<EnemyInstance> Enemies => enemies;

    // ─────────────────────────────────────────────────────────
    // 전투 초기화
    // 매 전투 진입 시 GameManager가 호출.
    // ─────────────────────────────────────────────────────────

    public void StartBattle(List<EnemyData> enemyDataList, List<CardData> masterDeck)
    {
        SetupEnemies(enemyDataList);
        SetupBattleDeck(masterDeck);
        ResetPlayerBattleState();
    }

    private void SetupEnemies(List<EnemyData> enemyDataList)
    {
        enemies.Clear();
        foreach (var data in enemyDataList)
            enemies.Add(new EnemyInstance(data));
    }

    // 카드 더미 초기화
    private void SetupBattleDeck(List<CardData> masterDeck)
    {
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();
        exhaustPile.Clear();

        foreach (var card in masterDeck)
            drawPile.Add(Instantiate(card));
        Shuffle(drawPile);
    }

    private void ResetPlayerBattleState()
    {
        Energy       = MaxEnergy;
        PlayerBlock  = 0;
        Ammo         = 3; // 기본 무기(권총) 탄창
    }

    private static void Shuffle(List<CardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, deck.Count);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    // 플레이어 턴에 뽑을 카드 더미에서 손패로 카드 가져오기(드로우)
    public void TakeOutCardtoHand()
    {
        if(hand.Count >= 10)
        {
            // 손패 최대치 10장. 10장 넘었으면 여분 뽑을 카드 더미로
            Debug.Log("손패가 가득 참.");
            return;
        }
        // 드로우할 카드가 손패에 들어감
        // 뽑을 카드 더미(drawpile)에서 랜덤으로 카드 6개 뽑아서 손패로

        for(int i = 0; i < DrawCount; i++)
        {
            if (drawPile.Count == 0)
            {
                // 뽑을 카드 더미가 비었으면 버릴 카드 더미를 섞어서 뽑을 카드 더미로
                if (discardPile.Count == 0)
                {
                    Debug.Log("뽑을 카드 더미와 버릴 카드 더미가 모두 액션빔.");
                    return;
                }
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                Shuffle(drawPile);
            }
            // 뽑을 카드 더미에서 랜덤으로 카드 하나 뽑아서 손패로
            int index = UnityEngine.Random.Range(0, drawPile.Count);
            CardData drawnCard = drawPile[index];
            hand.Add(drawnCard);
            drawPile.RemoveAt(index);
        }
        OnHandChanged?.Invoke();
    }

    // 

    public void PlayerTurnStart()
    {
        Energy = MaxEnergy;
        TakeOutCardtoHand();
    }

    public void PlayerTurnEnd()
    {
        
    }

    public void AddCardToDrawPile(CardData card)
    {
        drawPile.Add(Instantiate(card));
        Shuffle(drawPile);
    }

    public void AddCardToDiscardPile(CardData card)
    {
        discardPile.Add(Instantiate(card));
    }

    public void AddCardToHand(CardData card)
    {
        if (hand.Count < 10)
        {
            hand.Add(Instantiate(card));
            OnHandChanged?.Invoke();
        }
    }

    public void EnemyTurnStart()
    {
        foreach (var enemy in enemies)
        {
            var action = enemy.GetCurrentAction();
            if (action != null)
            {
                var ctx = new CardContext
                {
                    Battle      = this,
                    ActingEnemy = enemy,
                    AllEnemies  = enemies
                };
                foreach (var effect in action.effects)
                    effect.Execute(ctx);
            }
            enemy.AdvancePattern();
        }
    }

    
}

