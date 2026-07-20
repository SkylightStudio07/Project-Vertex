using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BattleType { Normal, Elite, Boss }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Inspector 기본값 — StartBattle 시 BattleState 초기화에 사용
    [Header("전투 기본 설정")]
    [SerializeField] private int defaultMaxEnergy = 3;
    [SerializeField] private int defaultAmmo      = 3;
    [SerializeField] private int defaultDrawCount = 5;

    // 런타임 전투 상태 — 모든 읽기/쓰기는 여기를 통함
    private BattleState _state;
    public BattleState State => _state;

    private bool _isInBattle;
    public bool IsInBattle => _isInBattle;
    // StartBattle 끝: _isInBattle = true;
    // Victory()/Defeat() 시작: _isInBattle = false;

    // 하위 호환 래퍼 (UI/외부 코드용)
    public int Energy      => _state?.Energy      ?? 0;
    public int MaxEnergy   => _state?.MaxEnergy   ?? defaultMaxEnergy;
    public int Ammo        => _state?.Ammo        ?? 0;
    public int PlayerBlock => _state?.Player?.Block ?? 0;

#if UNITY_EDITOR
    [Header("디버그")]
    [SerializeField] private List<string> _passiveDebugView = new();
#endif

    // 손패 변경 배치 처리
    private int  _handChangeBatchDepth;
    private bool _hasPendingHandChange;

    public IReadOnlyList<CardData>      Hand    => _state?.Hand    ?? new List<CardData>();
    public IReadOnlyList<EnemyInstance> Enemies => _state?.Enemies ?? new List<EnemyInstance>();

    public event Action         OnHandChanged;
    public event Action         OnEnemiesChanged;
    public event Action<BattleReward> OnBattleVictory;
    public event Action         OnBattleDefeat;

    private BattleType   _currentBattleType;
    private System.Random _rnd = new();
    // 카드 이펙트(DamageEffect 등)가 RandomEnemy 타겟을 결정할 때 이 인스턴스를 사용해야
    // 전투 시드 기반의 결정론적 동작이 보장된다. UnityEngine.Random.Range는 시드와 무관하다.
    public System.Random Rnd => _rnd;

    [Header("적 턴 연출")]
    [SerializeField] private EnemyTurnBannerView turnBanner;
    // 전진(lunge out)이 끝나는 시점 = 공격이 닿는 타이밍이라 여기서 데미지를 적용한다.
    // EnemyView.lungeOutDuration과 값을 맞춰야 모션과 타격감이 어긋나지 않는다.
    [SerializeField] private float lungeOutWaitDuration  = 0.2f;
    [SerializeField] private float lungeBackWaitDuration = 0.2f; // 후퇴 모션 + 피격 리액션 대기

    [Header("플레이어 턴 연출")]
    [SerializeField] private EnemyTurnBannerView playerTurnBanner; // 전투 첫 진입 시에는 PlayerTurnStart(false)로 건너뜀
    [SerializeField] private float postActionDelay       = 0.3f; // 추가 후처리 대기

    // ─────────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────────

    public void StartBattle(List<EnemyData> enemyDataList, List<CardData> masterDeck, int seed, BattleType battleType = BattleType.Normal)
    {
        _currentBattleType = battleType;
        _rnd = new System.Random(seed);

        _state = new BattleState
        {
            Player    = new PlayerCombatant(),
            Energy    = defaultMaxEnergy,
            MaxEnergy = defaultMaxEnergy,
            Ammo      = defaultAmmo,
            DrawCount = defaultDrawCount,
            Phase     = BattlePhase.PlayerTurn,
        };

        SetupEnemies(enemyDataList);
        SetupBattleDeck(masterDeck);
        _state.Player.OnDied += Defeat;
        OnEnemiesChanged?.Invoke();

        _isInBattle = true;
    }

    private void SetupEnemies(List<EnemyData> enemyDataList)
    {
        _state.Enemies.Clear();
        if (enemyDataList == null) return;

        foreach (var data in enemyDataList)
        {
            if (data == null)
            {
                Debug.LogWarning("[BattleManager] enemyDataList에 null 항목이 있어 건너뜀. Inspector 확인 필요.");
                continue;
            }
            var enemy = new EnemyInstance(data, _rnd);
            enemy.OnDied += CheckVictory;
            _state.Enemies.Add(enemy);
        }
    }

    private void SetupBattleDeck(List<CardData> masterDeck)
    {
        _state.Hand.Clear();
        _state.DrawPile.Clear();
        _state.DiscardPile.Clear();
        _state.ExhaustPile.Clear();

        foreach (var card in masterDeck)
            _state.DrawPile.Add(Instantiate(card));
        Shuffle(_state.DrawPile);
    }

    // ─────────────────────────────────────────────
    // 턴 흐름
    // ─────────────────────────────────────────────

    // 적 턴에서 넘어올 때(기본값)는 배너를 보여주고, 전투 최초 진입 시에는 PlayerTurnStart(false)로
    // 호출해 배너를 건너뛴다 (GameManager.InitializeBattle / DebugBattleStarter에서 사용).
    public void PlayerTurnStart() => PlayerTurnStart(true);

    public void PlayerTurnStart(bool showBanner)
    {
        StartCoroutine(PlayerTurnStartSequence(showBanner));
    }

    private IEnumerator PlayerTurnStartSequence(bool showBanner)
    {
        if (showBanner && playerTurnBanner != null)
            yield return playerTurnBanner.ShowAndWait();

        // 블록은 적 턴의 공격을 막아주는 용도라 적 턴이 끝난 뒤(=내 턴 시작 시점)에 초기화해야 한다.
        // PlayerTurnEnd에서 초기화하면 적이 공격하기 전에 블록이 사라져 무의미해진다.
        _state.Player.ResetBlock();

        BeginHandChangeBatch();
        try
        {
            _state.Energy = _state.MaxEnergy;
            TakeOutCardtoHand();
        }
        finally
        {
            EndHandChangeBatch();
        }
    }

    public void PlayerTurnEnd()
    {
        if (_state.Phase != BattlePhase.PlayerTurn) return;

        // 손패 → 버린 카드 더미
        BeginHandChangeBatch();
        try
        {
            foreach (var card in _state.Hand)
                _state.DiscardPile.Add(card);
            _state.Hand.Clear();
            NotifyHandChanged();
        }
        finally
        {
            EndHandChangeBatch();
        }

        _state.Player.TickPassives(_state);

        EnemyTurnStart();
    }

    public void EnemyTurnStart()
    {
        StartCoroutine(EnemyTurnSequence());
    }

    // 배너 표시 → 적마다 (전진 대기 → 데미지 적용(전진 피크 시점) → 후퇴+후처리 대기) → 다음 플레이어 턴.
    // 데미지를 전진이 끝나는 시점에 적용해서, 적이 아직 앞에 있는 동안 타격감이 나도록 한다
    // (후퇴까지 다 끝난 뒤 적용하면 적이 이미 물러난 다음에 맞는 것처럼 보여 타이밍이 늦게 느껴짐).
    private IEnumerator EnemyTurnSequence()
    {
        _state.Phase = BattlePhase.EnemyTurn;

        if (turnBanner != null)
            yield return turnBanner.ShowAndWait();

        // _state.Enemies를 직접 foreach하면 yield return 대기 중 OnDied 등이 리스트를 수정했을 때
        // InvalidOperationException(Collection was modified)이 발생할 수 있다.
        // 지금은 OnDied → CheckVictory()만 연결돼 있어 당장 터지진 않지만,
        // 나중에 OnDied에 리스트 제거 로직이 붙는 순간 조용히 터지는 문제라 복사본으로 순회한다.
        var enemiesCopy = new List<EnemyInstance>(_state.Enemies);
        foreach (var enemy in enemiesCopy)
        {
            if (enemy == null || enemy.IsDead) continue;

            enemy.TickPassives(_state);
            if (enemy.IsDead) continue; // 패시브(독 등)로 죽었으면 행동하지 않음

            enemy.NotifyActionStarted();
            yield return new WaitForSeconds(lungeOutWaitDuration);

            enemy.ExecuteCurrentAction(_state, this); // 전진 피크 시점에 데미지 적용

            yield return new WaitForSeconds(lungeBackWaitDuration + postActionDelay);

            if (_state.Player.IsDead) yield break; // OnDied → Defeat()는 이미 구독되어 있음
        }

        if (IsAllEnemiesDead()) yield break;

        _state.Phase = BattlePhase.PlayerTurn;
        PlayerTurnStart();
    }

    private bool IsAllEnemiesDead()
    {
        foreach (var e in _state.Enemies)
            if (!e.IsDead) return false;
        return true;
    }

    // ─────────────────────────────────────────────
    // 카드 플레이
    // ─────────────────────────────────────────────

    public bool IsCardPlayable(CardData card) => EvaluateCardPlayability(card);

    private bool EvaluateCardPlayability(CardData card)
    {
        if (card == null || !_state.Hand.Contains(card)) return false;
        if (_state.Energy < card.EnergyCost || _state.Ammo < card.AmmoCost) return false;

        if (card.UseMode == CardData.CardUseMode.SelectEnemy)
        {
            foreach (var e in _state.Enemies)
                if (!e.IsDead) return true;
            return false;
        }

        return true;
    }

    public bool TryPlayCard(CardData card, EnemyInstance target)
    {
        if (!EvaluateCardPlayability(card)) return false;
        if (card.UseMode == CardData.CardUseMode.SelectEnemy &&
            (target == null || target.IsDead || !_state.Enemies.Contains(target)))
            return false;

        // 비용 차감·패 제거는 즉시 처리해 UI가 바로 반영되도록 한다.
        // 이펙트 실행만 코루틴으로 분리해, 연타처럼 히트 사이 딜레이가 필요한 경우를 지원한다.
        BeginHandChangeBatch();
        try
        {
            _state.Energy -= card.EnergyCost;
            _state.Ammo   -= card.AmmoCost;

            _state.Hand.Remove(card);
            // 파워 카드는 isExhaust 설정과 무관하게 항상 소멸 — 패시브가 영구 등록되므로
            // 덱 순환으로 다시 뽑혀 중복 사용되는 것을 시스템 차원에서 차단한다.
            if (card.IsExhaust || card.Type == CardData.CardType.Power)
                _state.ExhaustPile.Add(card);
            else
                _state.DiscardPile.Add(card);
            NotifyHandChanged();
        }
        finally
        {
            EndHandChangeBatch();
        }

        var ctx = new CardContext
        {
            State      = _state,
            Battle     = this,
            Card       = card,
            Target     = target,
            AllEnemies = _state.Enemies,
        };
        StartCoroutine(ExecuteEffectsSequence(card.ActiveEffects, ctx));

        return true;
    }

    // 지금 아이템을 사용할 수 있는 상태인지 (전투 중 + 플레이어 턴). UI 버튼 활성 판정용.
    public bool CanUseItemNow => _isInBattle && _state != null && _state.Phase == BattlePhase.PlayerTurn;

    // 아이템 사용. 카드 사용(TryPlayCard)과 동일 구조, 차이는 비용 없음 / 인벤토리에서 소비 / ctx.Item 세팅.
    public bool TryUseItem(ItemData item, EnemyInstance target)
    {
        if (_state == null || item == null) return false;
        if (!CanUseItemNow) return false;   // 적 턴 중 사용 금지

        // SelectTarget 아이템은 유효한 적 타겟 필요 (타겟팅 UI 미구현)
        if (item.UseMode == ItemData.ItemUseMode.SelectTarget &&
            (target == null || target.IsDead || !_state.Enemies.Contains(target)))
            return false;

        BeginHandChangeBatch();   // 아이템 효과가 손패를 건드릴 수 있어 카드와 동일하게 배치로 묶음
        try
        {
            var ctx = new CardContext
            {
                State      = _state,
                Battle     = this,
                Card       = null,     // 카드 아님
                Item       = item,     // 아이템
                Target     = target,
                AllEnemies = _state.Enemies,
            };

            foreach (var effect in item.ItemEffects)
                effect?.Execute(ctx);
        }
        finally
        {
            EndHandChangeBatch();
        }

        if (ItemInventoryManager.Instance != null)
        {
            ItemInventoryManager.Instance.RemoveItem(item);   // 소비 → OnInventoryChanged로 바 자동 갱신
        }
        return true;
    }
    
    private IEnumerator ExecuteEffectsSequence(System.Collections.Generic.IReadOnlyList<CardEffect> effects, CardContext ctx)
    {
        foreach (var effect in effects)
        {
            if (effect != null)
                yield return StartCoroutine(effect.ExecuteCoroutine(ctx));
        }
    }

    // ─────────────────────────────────────────────
    // 카드 더미 조작 (Effect에서 호출 가능)
    // ─────────────────────────────────────────────

    public void TakeOutCardtoHand()
    {
        if (_state.Hand.Count >= 10)
        {
            Debug.Log("손패가 가득 참.");
            return;
        }

        bool drew = false;
        for (int i = 0; i < _state.DrawCount && _state.Hand.Count < 10; i++)
        {
            if (_state.DrawPile.Count == 0)
            {
                if (_state.DiscardPile.Count == 0)
                {
                    Debug.Log("뽑을 카드 더미와 버릴 카드 더미가 모두 비어있음.");
                    break;
                }
                _state.DrawPile.AddRange(_state.DiscardPile);
                _state.DiscardPile.Clear();
                Shuffle(_state.DrawPile);
            }

            int idx = _rnd.Next(0, _state.DrawPile.Count);
            _state.Hand.Add(_state.DrawPile[idx]);
            _state.DrawPile.RemoveAt(idx);
            drew = true;
        }

        if (drew) NotifyHandChanged();
    }

    // DrawEffect에서 특정 매수만큼 드로우
    public void DrawCards(int count)
    {
        if (_state == null || count <= 0) return;

        bool drew = false;
        for (int i = 0; i < count && _state.Hand.Count < 10; i++)
        {
            if (_state.DrawPile.Count == 0)
            {
                if (_state.DiscardPile.Count == 0) break;
                _state.DrawPile.AddRange(_state.DiscardPile);
                _state.DiscardPile.Clear();
                Shuffle(_state.DrawPile);
            }

            int idx = _rnd.Next(0, _state.DrawPile.Count);
            _state.Hand.Add(_state.DrawPile[idx]);
            _state.DrawPile.RemoveAt(idx);
            drew = true;
        }

        if (drew) NotifyHandChanged();
    }

    public void AddCardToDrawPile(CardData card)
    {
        _state.DrawPile.Add(Instantiate(card));
        Shuffle(_state.DrawPile);
    }

    public void AddCardToDiscardPile(CardData card)
    {
        _state.DiscardPile.Add(Instantiate(card));
    }

    public void AddCardToHand(CardData card) => AddCardsToHand(new[] { card });

    public void AddCardsToHand(IEnumerable<CardData> cards)
    {
        if (cards == null) return;
        bool added = false;
        foreach (var card in cards)
        {
            if (_state.Hand.Count >= 10) break;
            if (card == null) continue;
            _state.Hand.Add(Instantiate(card));
            added = true;
        }
        if (added) NotifyHandChanged();
    }

    // ─────────────────────────────────────────────
    // 승리 판정
    // ─────────────────────────────────────────────

    private void CheckVictory()
    {
        foreach (var e in _state.Enemies)
            if (!e.IsDead) return;
        Victory();
    }

    private void Victory()
    {
        // 승리 후 손패를 그대로 두면 맵으로 돌아갔을 때 손패가 가로채져서 맵 스크롤과 노드 클릭이 동작하지 않는 문제가 있었다.
        // 손패를 버리는 배치 처리로 해결. 승리 시 손패는 어차피 초기화되므로, 승리 후에 OnHandChanged가 한 번만 호출되도록...
        BeginHandChangeBatch();
        _isInBattle = false;
        try
        {
            foreach (var card in _state.Hand)
                _state.DiscardPile.Add(card);
            _state.Hand.Clear();
            NotifyHandChanged();
        }
        finally
        {
            EndHandChangeBatch();
        }

        var rewardData = GameManager.Instance.GetRewardProbability(_currentBattleType);
        // 보상 RNG — 노드 좌표로 재시드된 스트림이라 같은 노드면 항상 같은 보상.
        // 시드 규칙/스트림 구분은 RunRng에 통합돼 있다.
        var rewardRng = RunRng.For(RngStream.Reward,
                                   RunData.Instance.currentFloor,
                                   RunData.Instance.currentNodeIndex);
        var reward    = new BattleReward(GameManager.Instance.cardPools, rewardData, rewardRng);
        OnBattleVictory?.Invoke(reward);
        foreach (var e in _state.Enemies)
            e.OnDied -= CheckVictory;
    }

    private void Defeat()
    {
        _isInBattle = false;
        OnBattleDefeat?.Invoke();
    }

    // ─────────────────────────────────────────────
    // 내부 유틸
    // ─────────────────────────────────────────────

    private void NotifyHandChanged()
    {
        if (_handChangeBatchDepth > 0) { _hasPendingHandChange = true; return; }
        OnHandChanged?.Invoke();
    }

    private void BeginHandChangeBatch() => _handChangeBatchDepth++;

    private void EndHandChangeBatch()
    {
        _handChangeBatchDepth--;
        if (_handChangeBatchDepth > 0) return;
        bool changed = _hasPendingHandChange;
        _hasPendingHandChange = false;
        if (changed) OnHandChanged?.Invoke();
    }

    private void Shuffle(List<CardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int j = _rnd.Next(i, deck.Count);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    // ─────────────────────────────────────────────
    // 테스트용 (빌드 전 제거)
    // ─────────────────────────────────────────────

    public void TestVictory(BattleType battleType)
    {
        _currentBattleType = battleType;
        Victory();
    }

    private void Update()
    {
        if (Keyboard.current[Key.V].wasPressedThisFrame)
            TestVictory(BattleType.Normal);

#if UNITY_EDITOR
        if (_state?.Player != null)
            _passiveDebugView = _state.Player.DebugPassiveInfo;
#endif
    }
}
