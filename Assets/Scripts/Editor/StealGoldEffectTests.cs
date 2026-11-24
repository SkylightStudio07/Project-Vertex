using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// 크레딧 강탈(StealGoldEffect) → 강탈한 크레딧 상태 → 처치 시 반환(ReturnStolenGoldStatusBehavior)
public class StealGoldEffectTests
{
    private const string StolenGoldPath = "Assets/Data/Status/StolenGold.asset";
    private const string PickpocketPath = "Assets/Data/Enemy/EnemyDatas/친절한 강도/Actions/주머니 털기.asset";

    private GameObject _gameObject;
    private GameManager _previousGame, _game;
    private EnemyData _data;
    private EnemyInstance _thief;
    private BattleState _state;
    private StatusDefinition _stolenGold;

    private static void Singleton(Type type, object value) => type.GetProperty("Instance").SetValue(null, value);

    [SetUp]
    public void SetUp()
    {
        _previousGame = GameManager.Instance;
        _gameObject = new GameObject("StealGold test game");
        _game = _gameObject.AddComponent<GameManager>();
        Singleton(typeof(GameManager), _game);

        _data = ScriptableObject.CreateInstance<EnemyData>();
        _data.health = 50;
        _thief = new EnemyInstance(_data);
        _state = new BattleState { Player = new PlayerCombatant(), Enemies = { _thief } };
        _stolenGold = AssetDatabase.LoadAssetAtPath<StatusDefinition>(StolenGoldPath);
        Assert.IsNotNull(_stolenGold, "강탈한 크레딧 상태 에셋이 있어야 한다");
    }

    [TearDown]
    public void TearDown()
    {
        if (_gameObject != null) UnityEngine.Object.DestroyImmediate(_gameObject);
        Singleton(typeof(GameManager), _previousGame);
        if (_data != null) UnityEngine.Object.DestroyImmediate(_data);
    }

    private void Steal(int amount) => new StealGoldEffect { amount = amount, stolenStatus = _stolenGold }
        .Execute(new CardContext { State = _state, ActingEnemy = _thief, AllEnemies = _state.Enemies });

    [Test]
    public void Steal_TakesCreditsAndRecordsThemOnThief()
    {
        _game.PlayerGold = 100;
        Steal(15);
        Steal(15);

        Assert.AreEqual(70, _game.PlayerGold);
        Assert.AreEqual(30, _thief.Statuses.GetMagnitude(_stolenGold));
    }

    [Test]
    public void Steal_TakesOnlyWhatPlayerHas()
    {
        _game.PlayerGold = 10;
        Steal(15);

        Assert.AreEqual(0, _game.PlayerGold);
        Assert.AreEqual(10, _thief.Statuses.GetMagnitude(_stolenGold), "실제로 빼앗은 양만 쌓인다");

        Steal(15);
        Assert.AreEqual(10, _thief.Statuses.GetMagnitude(_stolenGold), "빈털터리면 더 쌓이지 않는다");
    }

    [Test]
    public void ThiefDeath_ReturnsStolenCredits()
    {
        _game.PlayerGold = 40;
        Steal(15);
        Steal(15);
        Assert.AreEqual(10, _game.PlayerGold);

        _thief.TakeDamage(new DamageInfo(999, _state.Player, true));
        _thief.Statuses.NotifyDeath(_state, _thief);

        Assert.AreEqual(40, _game.PlayerGold);
    }

    [Test]
    public void PoliteRobberPickpocket_UsesStealGoldWithReturnStatus()
    {
        var action = AssetDatabase.LoadAssetAtPath<EnemyAction>(PickpocketPath);
        Assert.IsNotNull(action);
        var steal = action.effects.Find(e => e is StealGoldEffect) as StealGoldEffect;
        Assert.IsNotNull(steal, "주머니 털기는 StealGoldEffect로 크레딧을 빼앗는다");
        Assert.AreEqual(15, steal.amount);
        Assert.AreEqual(_stolenGold, steal.stolenStatus);
        Assert.IsFalse(action.effects.Exists(e => e is GainGoldEffect), "반환되지 않는 GainGold 차감은 남아 있지 않아야 한다");
        Assert.IsFalse(_stolenGold.Dispellable);
    }
}
