using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class PressuredStatusTests
{
    private readonly List<Object> _objects = new();
    private GameManager _previousGame;
    private BattleManager _previousBattle, _battle;
    private BattleState _state;
    private StatusDefinition _pressured;
    private const string ActionPath = "Assets/Data/Enemy/EnemyDatas/오큘리엘/Actions/";

    [SetUp]
    public void SetUp()
    {
        _previousGame = GameManager.Instance;
        _previousBattle = BattleManager.Instance;
        var go = new GameObject("Pressure test");
        _objects.Add(go);
        var game = go.AddComponent<GameManager>();
        typeof(GameManager).GetProperty("Instance").SetValue(null, game);
        typeof(GameManager).GetProperty("PlayerHP").SetValue(game, 80);
        _state = new BattleState { Player = new PlayerCombatant() };
        typeof(BattleManager).GetProperty("Instance").SetValue(null, null);
        var battleObject = new GameObject("Pressure test battle");
        _objects.Add(battleObject);
        _battle = battleObject.AddComponent<BattleManager>();
        typeof(BattleManager).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_battle, _state);
        typeof(BattleManager).GetField("_rnd", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_battle, new System.Random(1));
        _pressured = AssetDatabase.LoadAssetAtPath<StatusDefinition>("Assets/Data/Status/Pressured.asset");
    }

    [TearDown]
    public void TearDown()
    {
        _state?.ReleaseRuntimeCards();
        foreach (var obj in _objects) Object.DestroyImmediate(obj);
        _objects.Clear();
        typeof(GameManager).GetProperty("Instance").SetValue(null, _previousGame);
        typeof(BattleManager).GetProperty("Instance").SetValue(null, _previousBattle);
    }

    private EnemyInstance Enemy(EnemyAction action)
    {
        var data = ScriptableObject.CreateInstance<EnemyData>();
        _objects.Add(data);
        data.health = 100;
        data.activityPatterns = new List<EnemyAction> { action };
        var enemy = new EnemyInstance(data);
        _state.Enemies.Add(enemy);
        return enemy;
    }

    private void Pressure(int stacks) => _state.Player.AddPassive(_pressured.CreateInstance(stacks));
    private static void Run(IEnumerator routine)
    {
        while (routine.MoveNext())
            if (routine.Current is IEnumerator nested) Run(nested);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void EndTurnCardAppliesToImmediateAttackAndIsConsumed(bool coroutine)
    {
        var card = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Cards/Status Cards/압박감.asset");
        var context = new CardContext { State = _state, Card = card };
        foreach (var effect in card.ActiveEffects)
            if (effect is ICardEndTurnInHandEffect endTurn) endTurn.OnTurnEndInHand(context);
        _state.Player.EndTurnPassives(_state);
        Assert.That(_state.Player.Statuses.GetMagnitude(_pressured), Is.EqualTo(1));

        var enemy = Enemy(AssetDatabase.LoadAssetAtPath<EnemyAction>(ActionPath + "선고.asset"));
        if (coroutine) Run(enemy.ExecuteCurrentActionCoroutine(_state, _battle));
        else enemy.ExecuteCurrentAction(_state, _battle);
        Assert.That(_state.Player.HP, Is.EqualTo(72), "7 + 압박감 1");
        Assert.That(_state.Player.Statuses.Find(_pressured), Is.Null);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void AllHitsOfJudgmentGainStacksThenNextAttackIsNormal(bool coroutine)
    {
        Pressure(3);
        var enemy = Enemy(AssetDatabase.LoadAssetAtPath<EnemyAction>(ActionPath + "심판.asset"));
        if (coroutine) Run(enemy.ExecuteCurrentActionCoroutine(_state, _battle));
        else enemy.ExecuteCurrentAction(_state, _battle);
        Assert.That(_state.Player.HP, Is.EqualTo(56), "(5 + 3) × 3");
        Assert.That(_state.Player.Statuses.Find(_pressured), Is.Null);
        enemy.ExecuteCurrentAction(_state, _battle);
        Assert.That(_state.Player.HP, Is.EqualTo(41), "두 번째 공격은 5 × 3");
    }

    [Test]
    public void PreviewAndNonAttackDoNotConsumeAndAnyEnemyCanTrigger()
    {
        Pressure(2);
        var idle = ScriptableObject.CreateInstance<EnemyAction>();
        _objects.Add(idle);
        var enemy = Enemy(idle);
        var status = _state.Player.Statuses.Find(_pressured);
        Assert.That(status.PreviewIncomingDamage(new DamageInfo(7, enemy), _state).Amount, Is.EqualTo(9));
        enemy.ExecuteCurrentAction(_state, _battle);
        _state.Player.EndTurnPassives(_state);
        _state.Player.StartTurnPassives(_state);
        Assert.That(_state.Player.Statuses.GetMagnitude(_pressured), Is.EqualTo(2));
        Enemy(AssetDatabase.LoadAssetAtPath<EnemyAction>(ActionPath + "선고.asset")).ExecuteCurrentAction(_state, _battle);
        Assert.That(_state.Player.HP, Is.EqualTo(71));
        Assert.That(_state.Player.Statuses.Find(_pressured), Is.Null);
    }

    [Test]
    public void EnvironmentalDamageDoesNotIncreaseOrConsumePressure()
    {
        Pressure(2);
        DamageCalculator.Resolve(new DamageInfo(5), _state.Player, _state);
        Assert.That(_state.Player.HP, Is.EqualTo(75));
        Assert.That(_state.Player.Statuses.GetMagnitude(_pressured), Is.EqualTo(2));
    }

    [Test]
    public void FullyBlockedAttackStillConsumesAfterAllHits()
    {
        Pressure(2);
        _state.Player.AddBlock(30);
        Enemy(AssetDatabase.LoadAssetAtPath<EnemyAction>(ActionPath + "심판.asset")).ExecuteCurrentAction(_state, _battle);
        Assert.That(_state.Player.HP, Is.EqualTo(80));
        Assert.That(_state.Player.Block, Is.EqualTo(9));
        Assert.That(_state.Player.Statuses.Find(_pressured), Is.Null);
    }
}
