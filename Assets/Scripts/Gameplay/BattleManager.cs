using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public enum BattleType { Normal, Elite, Boss }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // 싱글톤 인스턴스와 반복 사용할 대기 객체를 초기화한다.
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        RefreshWaitCache();
    }

    // Inspector 기본값 — StartBattle 시 BattleState 초기화에 사용
    [Header("전투 기본 설정")]
    [FormerlySerializedAs("defaultMaxEnergy")]
    [SerializeField] private int _defaultMaxEnergy = 3; // 전투 시작 시 적용할 기본 최대 에너지.
    [FormerlySerializedAs("defaultAmmo")]
    [SerializeField] private int _defaultAmmo = 3; // 전투 시작 시 적용할 기본 탄약 수.
    [FormerlySerializedAs("defaultDrawCount")]
    [SerializeField] private int _defaultDrawCount = 5; // 플레이어 턴 시작 시 기본으로 뽑을 카드 수.
    [FormerlySerializedAs("defaultWeapon")]
    [SerializeField] private WeaponData _defaultWeapon; // 전투 시작 시 장착할 기본 무기.

    // 런타임 전투 상태 — 모든 읽기/쓰기는 여기를 통함
    private BattleState _state;
    public BattleState State => _state;

    private bool _isInBattle;
    public bool IsInBattle => _isInBattle;
    // StartBattle 끝: _isInBattle = true;
    // Victory()/Defeat() 시작: _isInBattle = false;

    // 하위 호환 래퍼 (UI/외부 코드용)
    public int Energy      => _state?.Energy      ?? 0;
    public int MaxEnergy   => _state?.MaxEnergy   ?? _defaultMaxEnergy;
    public int Ammo        => _state?.Ammo        ?? 0;
    public int PlayerBlock => _state?.Player?.Block ?? 0;
    public WeaponData CurrentWeapon => _state?.CurrentWeapon;

#if UNITY_EDITOR
    [Header("디버그")]
    [SerializeField] private List<string> _passiveDebugView = new();
#endif

    // 손패 변경 배치 처리
    private int  _handChangeBatchDepth;
    private bool _hasPendingHandChange;
    private bool _isEndingPlayerTurn; // 플레이어 턴 종료 처리가 진행 중인지 나타낸다.

    public IReadOnlyList<CardData>      Hand    => _state != null ? _state.Hand : Array.Empty<CardData>();
    public IReadOnlyList<EnemyInstance> Enemies => _state != null ? _state.Enemies : Array.Empty<EnemyInstance>();
    public bool IsPlayerTurnEnding => _isEndingPlayerTurn; // 턴 종료 연출/정리 중 카드 입력을 막기 위한 읽기 전용 상태.

    public event Action         OnBattleStarted; // StartBattle() 끝에서 1회 발화 — 씬을 갈아끼우지 않고 화면을 SetActive로만 전환하는 구조라, 전투 UI는 Start/OnEnable 대신 이 이벤트로 매 전투 진입을 감지해야 한다 (PartyView 참고)
    public event Action         OnHandChanged;
    public event Func<float> OnHandExitAnimationRequested; // 손패 카드가 사라지기 전 퇴장 애니메이션을 요청하고 예상 시간을 돌려받는다.
    public event Action         OnEnemiesChanged;
    public event Action<WeaponData> OnWeaponChanged;
    // 플레이어가 카드를 실제로 사용한 시점(비용 차감 직후, 이펙트 실행 직전)에 발화.
    // 데미지 적용을 기다리지 않는 "구경용" 연출(PoseSequencePlayer 등)을 병행 재생하는 용도 —
    // 구독 쪽에서 결과를 기다리지 않고 그냥 재생만 하면 된다.
    public event Action<CardData> OnCardPlayed;
    public event Action<BattleReward> OnBattleVictory;
    public event Action         OnBattleDefeat;

    private BattleType   _currentBattleType;
    private System.Random _rnd = new();
    private WaitForSeconds _lungeOutWait; // 적 전진 타이밍 대기에 재사용할 객체.
    private WaitForSeconds _lungeBackAndPostActionWait; // 적 후퇴와 후처리 대기에 재사용할 객체.
    // 카드 이펙트(DamageEffect 등)가 RandomEnemy 타겟을 결정할 때 이 인스턴스를 사용해야
    // 전투 시드 기반의 결정론적 동작이 보장된다. UnityEngine.Random.Range는 시드와 무관하다.
    public System.Random Rnd => _rnd;

    [Header("적 턴 연출")]
    [FormerlySerializedAs("turnBanner")]
    [SerializeField] private EnemyTurnBannerView _turnBanner; // 적 턴 시작 배너를 표시하는 뷰.
    // 전진(lunge out)이 끝나는 시점 = 공격이 닿는 타이밍이라 여기서 데미지를 적용한다.
    // EnemyView.lungeOutDuration과 값을 맞춰야 모션과 타격감이 어긋나지 않는다.
    [FormerlySerializedAs("lungeOutWaitDuration")]
    [SerializeField] private float _lungeOutWaitDuration = 0.2f; // 적 전진 모션이 공격 지점에 도달할 때까지 기다릴 시간.
    [FormerlySerializedAs("lungeBackWaitDuration")]
    [SerializeField] private float _lungeBackWaitDuration = 0.2f; // 후퇴 모션과 피격 리액션을 기다릴 시간.

    [Header("플레이어 턴 연출")]
    [FormerlySerializedAs("playerTurnBanner")]
    [SerializeField] private EnemyTurnBannerView _playerTurnBanner; // 플레이어 턴 시작 배너를 표시하는 뷰.
    [FormerlySerializedAs("postActionDelay")]
    [SerializeField] private float _postActionDelay = 0.3f; // 적 행동 후 추가 후처리를 기다릴 시간.

    // ─────────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────────

    public void StartBattle(List<EnemyData> enemyDataList, List<CardData> masterDeck, int seed, BattleType battleType = BattleType.Normal)
    {
        StopAllCoroutines();
        _state?.ReleaseRuntimeCards();
        _currentBattleType = battleType;
        _rnd = new System.Random(seed);
        _isEndingPlayerTurn = false;

        _state = new BattleState
        {
            Player    = new PlayerCombatant(),
            Energy    = _defaultMaxEnergy,
            MaxEnergy = _defaultMaxEnergy,
            Ammo      = _defaultAmmo,
            DrawCount = _defaultDrawCount,
            Phase     = BattlePhase.PlayerTurn,
        };

        SetupEnemies(enemyDataList);
        _state.ChangePlayerWeapon(_defaultWeapon);
        SetupBattleDeck(masterDeck);
        _state.Player.Statuses.NotifyBattleStart(_state, _state.Player);
        foreach (var enemy in _state.Enemies)
            enemy.Statuses.NotifyBattleStart(_state, enemy);
        _state.Player.OnDied += Defeat;
        _state.Player.Statuses.OnChanged += RefreshAllEnemyIntents;
        OnBattleStarted?.Invoke();
        _state.Player.OnDamaged += HandlePlayerDamaged;
        OnEnemiesChanged?.Invoke();

        string enemyRoster = enemyDataList != null && enemyDataList.Count > 0
            ? string.Join(", ", enemyDataList.Where(e => e != null).Select(e => $"'{e.enemyName}'(HP:{e.health})"))
            : "없음";
        Debug.Log($"<color=#4ADE80>==================================================\n[BattleManager] 전투 개시! (타입: {battleType}, 시드: {seed})\n▶ 현재 대전 중인 적 목록 [{enemyDataList?.Count ?? 0}명]: {enemyRoster}\n==================================================</color>");

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

        if (masterDeck == null) return;
        foreach (var card in masterDeck)
        {
            if (card == null) continue;
            _state.DrawPile.Add(_state.CreateCard(card));
        }
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
        if (showBanner && _playerTurnBanner != null)
            yield return _playerTurnBanner.ShowAndWait();

        // 블록은 적 턴의 공격을 막아주는 용도라 적 턴이 끝난 뒤(=내 턴 시작 시점)에 초기화해야 한다.
        // PlayerTurnEnd에서 초기화하면 적이 공격하기 전에 블록이 사라져 무의미해진다.
        _state.PlayerLostHpThisTurn = false;
        _state.Player.ResetBlock();

        BeginHandChangeBatch();
        try
        {
            _state.Energy = _state.MaxEnergy;
            _state.Player.StartTurnPassives(_state);
            if (!_isInBattle || _state.Player.IsDead) yield break;
            TakeOutCardtoHand();
        }
        finally
        {
            EndHandChangeBatch();
        }
    }

    public void PlayerTurnEnd()
    {
        if (_state.Phase != BattlePhase.PlayerTurn || _isEndingPlayerTurn) return;
        // 카드 선택 중 턴이 끝나면 손패가 사라져 진행 중인 효과가 깨진다
        if (HandCardSelector.IsSelecting) return;

        StartCoroutine(PlayerTurnEndSequence());
    }

    // 플레이어 턴 종료 연출을 기다린 뒤 손패를 정리하고 적 턴을 시작한다.
    private IEnumerator PlayerTurnEndSequence()
    {
        _isEndingPlayerTurn = true;
        bool shouldStartEnemyTurn = false; // 턴 종료 정리가 정상 완료되면 적 턴을 시작하기 위한 플래그.

        try
        {
            EndPlayerTurnCards();

            float handExitDuration = OnHandExitAnimationRequested?.Invoke() ?? 0f; // 손패 퇴장 애니메이션의 총 대기 시간.
            if (handExitDuration > 0f)
                yield return WaitSecondsByFrame(handExitDuration);

            NotifyHandChanged();

            _state.Player.EndTurnPassives(_state);
            if (!_isInBattle || _state.Player.IsDead) yield break;

            shouldStartEnemyTurn = true;
        }
        finally
        {
            _isEndingPlayerTurn = false;
        }

        if (shouldStartEnemyTurn)
            EnemyTurnStart();
    }

    // 손패 카드를 기존 더미로 이동시키고 Retain 카드는 Hand에 남긴다.
    private void EndPlayerTurnCards()
    {
        bool shouldShuffleDrawPile = false; // 드로우 더미로 돌아가는 카드가 있을 때 true가 된다.
        for (int i = 0; i < _state.Hand.Count;)
        {
            CardData card = _state.Hand[i]; // 현재 턴 종료 처리를 검사할 손패 카드.
            bool returnToDrawPile = ShouldReturnToDrawPileAtTurnEnd(card); // 카드 효과가 드로우 더미 복귀를 요구하는지 저장한다.

            if (returnToDrawPile)
            {
                _state.Hand.RemoveAt(i);
                _state.DrawPile.Add(card);
                shouldShuffleDrawPile = true;
            }
            else if (card.IsRetain)
            {
                i++;
            }
            else if (card.IsEthereal)
            {
                _state.Hand.RemoveAt(i);
                _state.ExhaustPile.Add(card);
            }
            else
            {
                _state.Hand.RemoveAt(i);
                _state.DiscardPile.Add(card);
            }
        }

        if (shouldShuffleDrawPile)
            Shuffle(_state.DrawPile);
    }

    // 카드의 턴 종료 손패 효과를 실행하고 DrawPile 복귀 여부를 반환한다.
    private bool ShouldReturnToDrawPileAtTurnEnd(CardData card)
    {
        var ctx = new CardContext // 턴 종료 손패 효과 계산에 사용할 카드 실행 맥락.
        {
            State      = _state,
            Battle     = this,
            Card       = card,
            AllEnemies = _state.Enemies,
        };

        bool returnToDrawPile = false; // 효과가 DrawPile 복귀를 요구하는지 저장한다.
        foreach (var effect in card.ActiveEffects)
        {
            if (effect is ICardEndTurnInHandEffect endTurnEffect)
                returnToDrawPile |= endTurnEffect.OnTurnEndInHand(ctx);
        }

        return returnToDrawPile;
    }

    // WaitForSeconds 할당 없이 지정 시간만큼 프레임 단위로 대기한다.
    private IEnumerator WaitSecondsByFrame(float duration)
    {
        float elapsed = 0f; // 지금까지 대기한 누적 시간.
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
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

        if (_turnBanner != null)
            yield return _turnBanner.ShowAndWait();

        // _state.Enemies를 직접 foreach하면 yield return 대기 중 OnDied 등이 리스트를 수정했을 때
        // InvalidOperationException(Collection was modified)이 발생할 수 있다.
        // 지금은 OnDied → CheckVictory()만 연결돼 있어 당장 터지진 않지만,
        // 나중에 OnDied에 리스트 제거 로직이 붙는 순간 조용히 터지는 문제라 복사본으로 순회한다.
        var enemiesCopy = new List<EnemyInstance>(_state.Enemies);
        foreach (var enemy in enemiesCopy)
        {
            if (enemy == null || enemy.IsDead) continue;

            enemy.StartTurnPassives(_state);
            if (!_isInBattle || _state.Player.IsDead) yield break;
            if (enemy.IsDead) continue; // 패시브(독 등)로 죽었으면 행동하지 않음

            var action = enemy.GetCurrentAction();
            string actionName = action != null ? action.name : "없음";
            string intentDesc = action != null ? action.intentType.ToString() : "없음";
            Debug.Log($"[BattleManager] 적 턴 진행: '{enemy.Data?.enemyName}' -> 행동: '{actionName}' (인텐트: {intentDesc}, 현재 HP: {enemy.HP}/{enemy.MaxHP})");

            enemy.NotifyActionStarted();
            yield return _lungeOutWait;

            yield return enemy.ExecuteCurrentActionCoroutine(_state, this); // 전진 피크 시점에 효과 적용

            if (!_isInBattle || _state.Player.IsDead) yield break;
            if (!enemy.IsDead) enemy.EndTurnPassives(_state);
            if (!_isInBattle || _state.Player.IsDead) yield break;

            yield return _lungeBackAndPostActionWait;

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

    public CardPlayCost GetCardPlayCost(CardData card, EnemyInstance target = null)
    {
        if (card == null) return new CardPlayCost();

        var cost = new CardPlayCost(card.EnergyCost, card.AmmoCost);
        if (_state?.Player == null)
        {
            cost.ClampToNonNegative();
            return cost;
        }

        return _state.Player.Statuses.ModifyCardPlayCost(cost, CreateCardPlayContext(card, target));
    }

    private bool EvaluateCardPlayability(CardData card)
    {
        if (card == null || _state == null || !_state.Hand.Contains(card)) return false;
        if (!CanPayCardPlayCost(GetCardPlayCost(card))) return false;

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

        var cost = GetCardPlayCost(card, target);
        if (!CanPayCardPlayCost(cost)) return false;
        var ctx = CreateCardPlayContext(card, target);

        // 비용 차감·패 제거는 즉시 처리해 UI가 바로 반영되도록 한다.
        // 이펙트 실행만 코루틴으로 분리해, 연타처럼 히트 사이 딜레이가 필요한 경우를 지원한다.
        BeginHandChangeBatch();
        try
        {
            PayCardPlayCost(cost);

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

        if (_state.Player.IsDead) return true;

        OnCardPlayed?.Invoke(card);
        _state.Player.Statuses.NotifyCardPlayed(ctx, _state.Player);
        StartCoroutine(ExecuteEffectsSequence(card.ActiveEffects, ctx));

        return true;
    }

    private bool CanPayCardPlayCost(CardPlayCost cost)
    {
        if (_state?.Player == null) return false;
        if (_state.Energy < cost.Energy) return false;
        if (_state.Ammo < cost.Ammo) return false;
        if (_state.Player.HP < cost.Hp) return false;
        return true;
    }

    private void PayCardPlayCost(CardPlayCost cost)
    {
        _state.Energy -= cost.Energy;
        _state.Ammo -= cost.Ammo;

        if (cost.Hp > 0)
            _state.Player.TakeDamage(new DamageInfo(cost.Hp, null, true));
    }

    private CardContext CreateCardPlayContext(CardData card, EnemyInstance target)
    {
        return new CardContext
        {
            State      = _state,
            Battle     = this,
            Card       = card,
            Target     = target,
            AllEnemies = _state?.Enemies,
        };
    }

    // 지금 아이템을 사용할 수 있는 상태인지 (전투 중 + 플레이어 턴). UI 버튼 활성 판정용.
    public bool CanUseItemNow => _isInBattle && _state != null
                                 && _state.Phase == BattlePhase.PlayerTurn
                                 && !_isEndingPlayerTurn
                                 && !HandCardSelector.IsSelecting; // 손패 선택 중에는 아이템 사용 불가

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

            EffectRunner.ExecuteImmediate(item.ItemEffects, ctx);
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
        yield return EffectRunner.ExecuteSequence(effects, ctx);
        RefreshAllEnemyIntents();
    }

    public void RefreshAllEnemyIntents()
    {
        if (_state?.Enemies == null) return;
        foreach (var enemy in _state.Enemies)
        {
            if (enemy != null && !enemy.IsDead)
                enemy.NotifyIntentChanged();
        }
    }

    private void HandlePlayerDamaged(int actualDamage)
    {
        if (actualDamage > 0 && _state != null)
            _state.PlayerLostHpThisTurn = true;
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
        if (_state == null || card == null) return;
        _state.DrawPile.Add(_state.CreateCard(card));
        Shuffle(_state.DrawPile);
    }

    public void AddCardToDiscardPile(CardData card)
    {
        if (_state == null || card == null) return;
        _state.DiscardPile.Add(_state.CreateCard(card));
    }

    // 손패의 특정 카드를 버린 카드 더미로 보낸다 (DiscardEffect 등에서 호출).
    // 이미 손패에 없으면 아무 일도 하지 않는다 — 선택 대기 중 손패가 비는 경우를 조용히 흘려보낸다.
    public bool DiscardCardFromHand(CardData card)
    {
        if (_state == null || card == null) return false;
        if (!_state.Hand.Remove(card)) return false;

        _state.DiscardPile.Add(card);
        NotifyHandChanged();
        return true;
    }

    // 카드 한 장을 런타임 복사본으로 만들어 손패에 추가한다.
    public void AddCardToHand(CardData card)
    {
        if (_state == null || card == null || _state.Hand.Count >= 10) return;

        _state.Hand.Add(_state.CreateCard(card));
        NotifyHandChanged();
    }

    public void AddCardsToHand(IEnumerable<CardData> cards)
    {
        if (_state == null || cards == null) return;
        bool added = false;
        foreach (var card in cards)
        {
            if (_state.Hand.Count >= 10) break;
            if (card == null) continue;
            _state.Hand.Add(_state.CreateCard(card));
            added = true;
        }
        if (added) NotifyHandChanged();
    }

    public List<CardData> GetRandomCardsFromPlayerDeck(int count, bool allowDuplicates = false)
    {
        var selectedCards = new List<CardData>();
        var playerDeck = DeckManager.Instance?.PlayerDeck;
        if (count <= 0 || playerDeck == null || playerDeck.Count == 0) return selectedCards;

        var candidates = new List<CardData>();
        foreach (var card in playerDeck)
        {
            if (card != null) candidates.Add(card);
        }

        if (candidates.Count == 0) return selectedCards;

        for (int i = 0; i < count; i++)
        {
            if (candidates.Count == 0) break;

            int index = _rnd.Next(0, candidates.Count);
            selectedCards.Add(candidates[index]);

            if (!allowDuplicates)
                candidates.RemoveAt(index);
        }

        return selectedCards;
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
        Debug.Log($"<color=#4ADE80>[BattleManager] 전투 승리! ({_currentBattleType}) 모든 적을 무찔렀습니다.</color>");
        OnBattleVictory?.Invoke(reward);
        foreach (var e in _state.Enemies)
            e.OnDied -= CheckVictory;
    }

    private void Defeat()
    {
        _isInBattle = false;
        Debug.Log($"<color=#F87171>[BattleManager] 전투 패배... 플레이어가 쓰러졌습니다.</color>");
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

    public bool ChangePlayerWeapon(WeaponData weapon)
    {
        if (_state == null || !_state.ChangePlayerWeapon(weapon)) return false;
        NotifyHandChanged();
        OnWeaponChanged?.Invoke(weapon);
        return true;
    }

    private void OnDestroy()
    {
        _state?.ReleaseRuntimeCards();
        if (Instance == this) Instance = null;
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

    // 반복 사용되는 WaitForSeconds 객체를 현재 설정값 기준으로 다시 만든다.
    private void RefreshWaitCache()
    {
        _lungeOutWait = new WaitForSeconds(_lungeOutWaitDuration);
        _lungeBackAndPostActionWait = new WaitForSeconds(_lungeBackWaitDuration + _postActionDelay);
    }

#if UNITY_EDITOR
    // Inspector 값 변경 시 에디터에서 대기 객체 캐시를 최신 값으로 갱신한다.
    private void OnValidate()
    {
        RefreshWaitCache();
    }
#endif

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
