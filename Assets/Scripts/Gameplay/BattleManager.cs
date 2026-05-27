using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 전투 유형 일단 임시?
public enum BattleType
{
    Normal,
    Elite,
    Boss,
}

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
    private int handChangeBatchDepth;
    private bool hasPendingHandChange;

    // UI용으로 읽기 전용 손패 리스트 빼놓은 거.
    public IReadOnlyList<CardData> Hand => hand;

    // 손패가 바뀔 때마다 HandView가 구독해서 화면 갱신
    public event Action OnHandChanged;

    private System.Random rndSeed = new();

    // EnemyInstance는 plain C# 클래스라 Inspector엔 안 뜸 — 런타임 전용
    private readonly List<EnemyInstance> enemies = new();
    // 이건 필요할지 모르겠는데, 일단 해 둠.
    public IReadOnlyList<EnemyInstance> Enemies => enemies;

    // 전투 승리시 보상 띄우기 위한 이벤트.
    public event Action<Reward> OnBattleVictory;
    private BattleType currentBattleType;

    // 적 리스트가 바뀔 때 EnemyZoneView가 구독해서 화면 갱신
    public event Action OnEnemiesChanged;

    // 전투 초기화
    // 매 전투 진입 시 GameManager가 호출.

    public void StartBattle(List<EnemyData> enemyDataList, List<CardData> masterDeck, int seed)
    {
        rndSeed = new System.Random(seed);
        SetupEnemies(enemyDataList);
        SetupBattleDeck(masterDeck);
        ResetPlayerBattleState();
        OnEnemiesChanged?.Invoke();
    }

    private void SetupEnemies(List<EnemyData> enemyDataList)
    {
        enemies.Clear();
        foreach (var data in enemyDataList)
        {
            EnemyInstance enemy = new EnemyInstance(data);
            enemy.OnDied += CheckVictory;
            enemies.Add(enemy);
        }
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

    private void Shuffle(List<CardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int j = rndSeed.Next(i, deck.Count);
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

        bool drewAnyCard = false;
        for(int i = 0; i < DrawCount && hand.Count < 10; i++)
        {
            if (drawPile.Count == 0)
            {
                // 뽑을 카드 더미가 비었으면 버릴 카드 더미를 섞어서 뽑을 카드 더미로
                if (discardPile.Count == 0)
                {
                    Debug.Log("뽑을 카드 더미와 버릴 카드 더미가 모두 액션빔.");
                    break;
                }
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                Shuffle(drawPile);
            }
            // 뽑을 카드 더미에서 랜덤으로 카드 하나 뽑아서 손패로
            int index = rndSeed.Next(0, drawPile.Count);
            CardData drawnCard = drawPile[index];
            hand.Add(drawnCard);
            drawPile.RemoveAt(index);
            drewAnyCard = true;
        }

        if (drewAnyCard)
            NotifyHandChanged();
    }

    // 

    public void PlayerTurnStart()
    {
        BeginHandChangeBatch();
        try
        {
            Energy = MaxEnergy;
            TakeOutCardtoHand();
        }
        finally
        {
            EndHandChangeBatch();
        }
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
        AddCardsToHand(new[] { card });
    }

    public void AddCardsToHand(IEnumerable<CardData> cards)
    {
        if (cards == null) return;

        bool addedAnyCard = false;
        foreach (CardData card in cards)
        {
            if (hand.Count >= 10) break;
            if (card == null) continue;

            hand.Add(Instantiate(card));
            addedAnyCard = true;
        }

        if (addedAnyCard)
            NotifyHandChanged();

        //핸드가 꽉 차는 등 카드가 추가될 수 없ㄴ으면 추가 로직이 있어야함
    }

    public bool IsCardPlayable(CardData card)
    {
        return EvaluateCardPlayability(card);
    }

    private bool EvaluateCardPlayability(CardData card)
    {
        if (card == null || !hand.Contains(card)) return false;
        if (Energy < card.EnergyCost || Ammo < card.AmmoCost) return false;

        if (card.UseMode == CardData.CardUseMode.SelectEnemy)
        {
            foreach (EnemyInstance enemy in enemies)
            {
                if (!enemy.IsDead) return true;
            }

            return false;
        }

        return true;
    }

    private void NotifyHandChanged()
    {
        if (handChangeBatchDepth > 0)
        {
            hasPendingHandChange = true;
            return;
        }

        OnHandChanged?.Invoke();
    }

    private void BeginHandChangeBatch()
    {
        handChangeBatchDepth++;
    }

    private void EndHandChangeBatch()
    {
        handChangeBatchDepth--;
        if (handChangeBatchDepth > 0) return;

        bool handChanged = hasPendingHandChange;
        hasPendingHandChange = false;

        if (handChanged)
            OnHandChanged?.Invoke();
    }

    //카드 플레이 시 카드 효과 실행. 카드가 손패에 없거나, 에너지/탄약 부족, 적 선택 필요 카드에 적 선택 안 했거나 유효하지 않은 적 선택한 경우 플레이 실패.
    public bool TryPlayCard(CardData card, EnemyInstance target)
    {
        if (!EvaluateCardPlayability(card)) return false;
        if (card.UseMode == CardData.CardUseMode.SelectEnemy &&
            (target == null || target.IsDead || !enemies.Contains(target)))
            return false;

        BeginHandChangeBatch(); //배치 시작 - 카드 플레이 과정에서 손패 여러 번 바뀔 수 있으니, 배치로 묶어서 한 번만 갱신하도록
        try
        {
            Energy -= card.EnergyCost;
            Ammo -= card.AmmoCost;

            hand.Remove(card);
            if (card.IsExhaust) exhaustPile.Add(card);
            else discardPile.Add(card);
            NotifyHandChanged();

            var ctx = new CardContext
            {
                Battle     = this,
                Card       = card,
                Target     = target,
                AllEnemies = enemies
            };

            foreach (CardEffect effect in card.ActiveEffects)
            {
                if (effect != null)
                    effect.Execute(ctx);
            }
        }
        finally
        {
            EndHandChangeBatch(); //배치 끝
        }

        return true;
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

    private void CheckVictory()
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead) return;
        }

        Victory();
    }

    // 테스트용 - 빌드 전 제거
    public void TestVictory(BattleType battleType)
    {
        currentBattleType = battleType;
        Victory();
    }
    private void Update()
    {
        if (Keyboard.current[Key.V].wasPressedThisFrame)
        {
            TestVictory(BattleType.Normal);
        }
    }

    private void Victory()
    {
        RewardProbabilityData rewardData = GameManager.Instance.GetRewardProbability(currentBattleType);
        Reward reward = new Reward(GameManager.Instance.cardPools, rewardData, currentBattleType);

        OnBattleVictory?.Invoke(reward);
        foreach (var enemy in enemies)
        {
            enemy.OnDied -= CheckVictory;
        }
    }
}
