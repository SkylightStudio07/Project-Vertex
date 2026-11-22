using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// 방어도 파괴 상태: 피격 즉시 행동 교체, 다음 행동 전 만료, 패턴 진행을 검증한다.
public class EnemyActionInterruptTests
{
    private readonly List<Object> _assets = new();
    private EnemyAction _prepare, _attack, _conflict, _recovery;
    private StatusDefinition _synchronizing;
    private EnemyData _data;
    private EnemyInstance _enemy;
    private BattleState _state;

    private T Asset<T>() where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        _assets.Add(asset);
        return asset;
    }

    [SetUp]
    public void SetUp()
    {
        _prepare = Asset<EnemyAction>();
        _attack = Asset<EnemyAction>();
        _attack.intentType = IntentType.Attack;
        _conflict = Asset<EnemyAction>();
        _conflict.intentType = IntentType.Stun;
        _recovery = Asset<EnemyAction>();
        _synchronizing = Asset<StatusDefinition>();
        typeof(StatusDefinition).GetField("behaviors", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(_synchronizing, new List<StatusBehavior>
            {
                new BlockBreakInterruptStatusBehavior
                {
                    expectedAction = _attack,
                    replacementAction = _conflict
                }
            });
        _prepare.effects = new List<CardEffect>
        {
            new BlockEffect { amount = 15 },
            new ApplyStatusEffect
            {
                status = _synchronizing,
                amount = 1,
                targets = EffectTargetSelector.Source
            }
        };
        _data = Asset<EnemyData>();
        _data.health = 100;
        _data.activityPatterns = new List<EnemyAction> { _prepare, _attack, _recovery };
        _enemy = new EnemyInstance(_data);
        _state = new BattleState { Enemies = { _enemy } };
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var asset in _assets)
            if (asset != null) Object.DestroyImmediate(asset);
        _assets.Clear();
    }

    private void Prepare()
    {
        _enemy.StartTurnPassives(_state);
        _enemy.ExecuteCurrentAction(_state, null);
        _enemy.EndTurnPassives(_state);
        Assert.AreSame(_attack, _enemy.GetCurrentAction());
        Assert.AreEqual(1, _enemy.Statuses.GetMagnitude(_synchronizing));
    }

    [Test]
    public void ExactBlockDamage_ReplacesImmediatelyWithoutHpLoss_AndRefreshesIntent()
    {
        Prepare();
        bool conflictShown = false;
        _enemy.OnIntentChanged += () => conflictShown |= _enemy.GetCurrentAction() == _conflict;

        DamageCalculator.Resolve(new DamageInfo(15), _enemy, _state);

        Assert.AreEqual(100, _enemy.HP);
        Assert.AreEqual(0, _enemy.Block);
        Assert.AreSame(_conflict, _enemy.GetCurrentAction());
        Assert.IsTrue(conflictShown);
        Assert.IsNull(_enemy.Statuses.Find(_synchronizing));
    }

    [Test]
    public void PartialDamage_KeepsAttackUntilTheHitThatBreaksBlock()
    {
        Prepare();
        _enemy.TakeDamage(new DamageInfo(7));
        Assert.AreEqual(8, _enemy.Block);
        Assert.AreSame(_attack, _enemy.GetCurrentAction());

        _enemy.TakeDamage(new DamageInfo(8));
        _enemy.TakeDamage(new DamageInfo(3));

        Assert.AreSame(_conflict, _enemy.GetCurrentAction());
        Assert.AreSame(_recovery, _enemy.GetUpcomingActions(1)[0]);
        _enemy.ExecuteCurrentAction(_state, null);
        Assert.AreSame(_recovery, _enemy.GetCurrentAction(), "대체 행동 후 한 칸만 진행해야 합니다.");
    }

    [Test]
    public void OverflowDamage_AppliesRemainingHpDamageAndReplacesAction()
    {
        Prepare();
        _enemy.TakeDamage(new DamageInfo(19));
        Assert.AreEqual(96, _enemy.HP);
        Assert.AreSame(_conflict, _enemy.GetCurrentAction());
    }

    [Test]
    public void NoStatus_BlockBreakDoesNotReplaceAction()
    {
        _enemy.AdvancePattern();
        _enemy.AddBlock(15);
        _enemy.TakeDamage(new DamageInfo(15));
        Assert.AreSame(_attack, _enemy.GetCurrentAction());
    }

    [Test]
    public void ResetAndHpOnlyDamage_DoNotTriggerBlockBreak()
    {
        Prepare();
        _enemy.ResetBlock();
        _enemy.TakeDamage(new DamageInfo(3));
        Assert.AreEqual(97, _enemy.HP);
        Assert.AreSame(_attack, _enemy.GetCurrentAction());
        Assert.AreEqual(1, _enemy.Statuses.GetMagnitude(_synchronizing));
    }

    [Test]
    public void UnbrokenPreparation_ExpiresOnNextOwnTurnStart()
    {
        Prepare();
        _enemy.ResetBlock();
        _enemy.StartTurnPassives(_state);
        Assert.IsNull(_enemy.Statuses.Find(_synchronizing));
        Assert.AreSame(_attack, _enemy.GetCurrentAction());

        _enemy.AddBlock(15);
        _enemy.TakeDamage(new DamageInfo(15));
        Assert.AreSame(_attack, _enemy.GetCurrentAction());
    }

    [Test]
    public void PiercingDamage_DoesNotInterruptWhileBlockRemains()
    {
        Prepare();
        _enemy.TakeDamage(new DamageInfo(3, isPiercing: true));
        Assert.AreEqual(97, _enemy.HP);
        Assert.AreEqual(15, _enemy.Block);
        Assert.AreSame(_attack, _enemy.GetCurrentAction());
    }

    [Test]
    public void DifferentScheduledAction_IsNotOverwritten()
    {
        Prepare();
        _enemy.AdvancePattern();
        _enemy.TakeDamage(new DamageInfo(15));
        Assert.AreSame(_recovery, _enemy.GetCurrentAction());
    }

    [Test]
    public void LethalBlockBreak_PrioritizesDeath()
    {
        Prepare();
        int deaths = 0;
        _enemy.OnDied += () => deaths++;
        _enemy.TakeDamage(new DamageInfo(115));
        Assert.IsTrue(_enemy.IsDead);
        Assert.AreEqual(1, deaths);
        Assert.AreSame(_attack, _enemy.GetCurrentAction());
    }

    [Test]
    public void SameHitPhaseTransition_PrioritizesNewPhase()
    {
        // 다음 페이즈가 같은 공격을 참조해도 이전 페이즈의 중단 조건을 적용하지 않는다.
        _data.phases.Add(new EnemyPhase
        {
            hpRatioThreshold = 0.5f,
            activityPatterns = new List<EnemyAction> { _attack, _recovery }
        });
        Prepare();
        _enemy.TakeDamage(new DamageInfo(65));
        Assert.AreEqual(0, _enemy.PhaseIndex);
        Assert.AreSame(_attack, _enemy.GetCurrentAction());
    }

    [Test]
    public void SharedData_InterruptsOnlyTheDamagedEnemy()
    {
        Prepare();
        var other = new EnemyInstance(_data);
        _state.Enemies.Add(other);
        other.ExecuteCurrentAction(_state, null);

        _enemy.TakeDamage(new DamageInfo(15));

        Assert.AreSame(_conflict, _enemy.GetCurrentAction());
        Assert.AreSame(_attack, other.GetCurrentAction());
        Assert.AreEqual(15, other.Block);
        Assert.AreEqual(1, other.Statuses.GetMagnitude(_synchronizing));
    }

    [Test]
    public void SelfAppliedTurnEndStatus_CanSurviveThroughTheNextPlayerTurn()
    {
        typeof(StatusDefinition).GetField("durationPolicy", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(_synchronizing, StatusDurationPolicy.DecreaseOnTurnEnd);
        _state.Phase = BattlePhase.EnemyTurn;
        new ApplyStatusEffect
        {
            status = _synchronizing, amount = 1, targets = EffectTargetSelector.Source
        }.Execute(new CardContext { State = _state, ActingEnemy = _enemy });

        _enemy.EndTurnPassives(_state);
        Assert.AreEqual(1, _enemy.Statuses.GetMagnitude(_synchronizing));
        _state.Phase = BattlePhase.PlayerTurn;
        Assert.AreEqual(1, _enemy.Statuses.GetMagnitude(_synchronizing));
        _state.Phase = BattlePhase.EnemyTurn;
        _enemy.StartTurnPassives(_state);
        _enemy.EndTurnPassives(_state);
        Assert.IsNull(_enemy.Statuses.Find(_synchronizing));
    }

    [Test]
    public void SelfAppliedStartStatus_ExpiresAtNextOwnTurnStart()
    {
        _state.Phase = BattlePhase.EnemyTurn;
        new ApplyStatusEffect
        {
            status = _synchronizing, amount = 1, targets = EffectTargetSelector.Source
        }.Execute(new CardContext { State = _state, ActingEnemy = _enemy });
        _enemy.EndTurnPassives(_state);
        Assert.AreEqual(1, _enemy.Statuses.GetMagnitude(_synchronizing));
        _enemy.StartTurnPassives(_state);
        Assert.IsNull(_enemy.Statuses.Find(_synchronizing));
    }

    [Test]
    public void StatusFromAnotherEnemy_DoesNotSkipOwnTurnEnd()
    {
        typeof(StatusDefinition).GetField("durationPolicy", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(_synchronizing, StatusDurationPolicy.DecreaseOnTurnEnd);
        var other = new EnemyInstance(_data);
        _enemy.AddPassive(_synchronizing.CreateInstance(1, source: other));
        _enemy.EndTurnPassives(_state);
        Assert.IsNull(_enemy.Statuses.Find(_synchronizing));
    }

    [Test]
    public void ReplacementUsingNextPatternAction_DoesNotRepeatThatAction()
    {
        _data.activityPatterns = new List<EnemyAction> { _prepare, _attack, _conflict, _recovery };
        Prepare();
        _enemy.TakeDamage(new DamageInfo(15));
        Assert.AreSame(_conflict, _enemy.GetCurrentAction());
        Assert.AreSame(_recovery, _enemy.GetUpcomingActions(1)[0]);
        _enemy.ExecuteCurrentAction(_state, null);
        Assert.AreSame(_recovery, _enemy.GetCurrentAction(), "대체 행동을 중복하지 않고 다음 순서로 넘어가야 합니다.");
    }

    [Test]
    public void AttackAndBlockPreview_ShowsBothValuesWithoutExecutingEffects()
    {
        var target = new EnemyInstance(_data);
        _attack.effects = new List<CardEffect>
        {
            new DamageEffect { amount = 6 }, new BlockEffect { amount = 6 }
        };
        var preview = EffectRunner.Preview(_attack.effects, new CardContext
        {
            State = _state, ActingEnemy = _enemy, PrimaryTargetOverride = target
        });
        Assert.AreEqual(6, preview.TotalDamage);
        Assert.AreEqual(6, preview.GetBlockGain(_enemy));
        Assert.AreEqual(0, preview.GetBlockGain(target));
        Assert.AreEqual(0, _enemy.Block, "의도 미리보기는 방어도를 실제로 부여하면 안 됩니다.");
        Assert.AreEqual(100, target.HP);
    }

    [Test]
    public void BlockPreview_UsesTheSameStatusModifierAsExecution()
    {
        var modifier = Asset<StatusDefinition>();
        typeof(StatusDefinition).GetField("behaviors", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(modifier, new List<StatusBehavior>
            {
                new BlockModifierStatusBehavior { magnitudeSource = StatusMagnitudeSource.Constant, constantAmount = 2 }
            });
        _enemy.AddPassive(modifier.CreateInstance(1));
        var effect = new BlockEffect { amount = 6 };
        var context = new CardContext { State = _state, ActingEnemy = _enemy };
        var preview = EffectRunner.Preview(new[] { effect }, context);
        Assert.AreEqual(8, preview.GetBlockGain(_enemy));
        Assert.AreEqual(0, _enemy.Block);
        effect.Execute(context);
        Assert.AreEqual(8, _enemy.Block);
    }

    [Test]
    public void BrokenPreparation_ReplacesAttackWithExactlyOneStunnedAction()
    {
        Prepare();
        _enemy.TakeDamage(new DamageInfo(15));
        Assert.AreEqual(IntentType.Stun, _enemy.GetCurrentAction().intentType);
        Assert.IsEmpty(_enemy.GetCurrentAction().effects);
        _enemy.StartTurnPassives(_state);
        _enemy.ExecuteCurrentAction(_state, null);
        _enemy.EndTurnPassives(_state);
        Assert.AreSame(_recovery, _enemy.GetCurrentAction(), "공격 집행을 다시 시도하지 않고 재정렬로 진행해야 합니다.");
        Assert.AreEqual(100, _enemy.HP);
    }
}
