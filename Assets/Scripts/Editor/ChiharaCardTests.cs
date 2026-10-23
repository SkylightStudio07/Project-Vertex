using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// 치하라(Cp_01) 카드: 노션 「주인공 카드 풀 - 실장」 피의 무장~점혈 11장 기준
public class ChiharaCardTests
{
    private const string Cards = "Assets/Data/Cards/CH_01/";
    private readonly List<UnityEngine.Object> _assets = new();
    private BattleState _state;
    private EnemyInstance _enemy, _enemy2;
    private GameObject _gameObject, _battleObject;
    private GameManager _previousGame;
    private BattleManager _previousBattle, _battle;
    private GameManager _game;

    private static void Singleton(Type type, object value) => type.GetProperty("Instance").SetValue(null, value);
    private void SetHP(int hp) => typeof(GameManager).GetProperty("PlayerHP").SetValue(_game, hp);
    private static CardData Card(string id) => AssetDatabase.LoadAssetAtPath<CardData>($"{Cards}CH_01_{id}.asset");
    private static StatusDefinition Def(string path) => AssetDatabase.LoadAssetAtPath<StatusDefinition>($"Assets/Data/Status/{path}.asset");

    [SetUp]
    public void SetUp()
    {
        _previousGame = GameManager.Instance; _previousBattle = BattleManager.Instance;
        _gameObject = new GameObject("Chihara test game");
        _game = _gameObject.AddComponent<GameManager>();
        Singleton(typeof(GameManager), _game);
        SetHP(60);
        _battleObject = new GameObject("Chihara test battle");
        _battle = _battleObject.AddComponent<BattleManager>();
        Singleton(typeof(BattleManager), _battle);

        _enemy = Enemy(); _enemy2 = Enemy();
        _state = new BattleState { Player = new PlayerCombatant(), Enemies = { _enemy, _enemy2 } };
        typeof(BattleManager).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_battle, _state);
    }

    private EnemyInstance Enemy()
    {
        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.health = 100;
        _assets.Add(data);
        return new EnemyInstance(data);
    }

    [TearDown]
    public void TearDown()
    {
        _state?.ReleaseRuntimeCards();
        if (_battleObject != null) UnityEngine.Object.DestroyImmediate(_battleObject);
        if (_gameObject != null) UnityEngine.Object.DestroyImmediate(_gameObject);
        Singleton(typeof(GameManager), _previousGame); Singleton(typeof(BattleManager), _previousBattle);
        foreach (var a in _assets) if (a != null) UnityEngine.Object.DestroyImmediate(a);
        _assets.Clear();
    }

    private CardContext Ctx(CardData card) => new() { State = _state, Battle = _battle, Card = card, Target = _enemy, AllEnemies = _state.Enemies };
    private void Play(CardData card) => EffectRunner.ExecuteImmediate(card.ActiveEffects, Ctx(card));
    private CardData Up(string id) { var c = _state.CreateCard(Card(id)); c.isUpgraded = true; return c; }
    private void Vuln(EnemyInstance e, int n) => e.AddPassive(Def("Vulnerable").CreateInstance(n));

    [Test]
    public void BloodBurst_HitsOncePerVulnerableStack()
    {
        Vuln(_enemy, 3);
        Play(Card("C15"));
        // 취약 3회 × 5 피해 (취약 50% → 7) = 21 안팎. 타수가 3이면 0보다 크고, 취약 없을 때는 0이어야 한다
        Assert.Less(_enemy.HP, 100);
        Play(_state.CreateCard(Card("C15")));
        int before = _enemy2.HP;
        EffectRunner.ExecuteImmediate(Card("C15").ActiveEffects, new CardContext { State = _state, Battle = _battle, Card = Card("C15"), Target = _enemy2, AllEnemies = _state.Enemies });
        Assert.AreEqual(before, _enemy2.HP, "취약이 없으면 타격 0번");
    }

    [Test]
    public void BloodBurst_HitCountMatchesStacks()
    {
        Vuln(_enemy, 2);
        var effect = (DamageEffect)Card("C15").ActiveEffects[0];
        Assert.AreEqual(2, effect.HitsFor(Ctx(Card("C15")), _enemy));
    }

    [Test]
    public void OpenWound_UpgradedDoublesTargetVulnerable_NotSelf()
    {
        Vuln(_enemy, 2);
        _state.Player.AddPassive(Def("Vulnerable").CreateInstance(1));
        Play(Up("C03"));
        Assert.AreEqual(4, _enemy.Statuses.GetMagnitude(Def("Vulnerable")), "대상 취약 2배");
        Assert.AreEqual(1, _state.Player.Statuses.GetMagnitude(Def("Vulnerable")), "자기 취약은 그대로");
    }

    [Test]
    public void BloodClaw_HitsAllEnemiesTwice_AndCostsOneHp()
    {
        Play(Card("C13"));
        Assert.AreEqual(90, _enemy.HP); Assert.AreEqual(90, _enemy2.HP);
        Assert.AreEqual(59, _game.PlayerHP);
    }

    [Test]
    public void BloodLeech_HealsAttackerPerHit()
    {
        SetHP(50);
        Play(Card("C14"));
        new DamageEffect { amount = 4, hitCount = 2 }.Execute(Ctx(Card("C14")));
        Assert.AreEqual(56, _game.PlayerHP, "타수마다 3 회복 × 2");
    }

    [Test]
    public void BloodRegen_HealsMoreIfHpWasLostThisTurn()
    {
        SetHP(40);
        Play(Card("C12"));
        Assert.AreEqual(45, _game.PlayerHP);
        _state.PlayerLostHpThisTurn = true;
        Play(Card("C12"));
        Assert.AreEqual(55, _game.PlayerHP);
    }

    [Test]
    public void BloodArmor_UpgradedGivesTwelveBlock()
    {
        Play(Up("C07"));
        Assert.AreEqual(12, _state.Player.Block);
        Assert.AreEqual(57, _game.PlayerHP);
        Assert.AreEqual("피의 무장+", Up("C07").CardName);
    }

    [Test]
    public void Companion_PoolsMatchNotion()
    {
        var cp = AssetDatabase.LoadAssetAtPath<CoopCharData>("Assets/Data/Cooperator/Cp_01.asset");
        var names = new HashSet<string>();
        foreach (var c in cp.OwnedCards()) names.Add(c.BaseCardName);
        foreach (var n in new[] { "피의 무장", "피를 대가로", "이혈제혈", "혈액재생", "비인혈조", "착혈", "조혈", "할퀴기", "상처 벌리기", "혈흔 폭발", "점혈" })
            Assert.IsTrue(names.Contains(n), n);
        Assert.AreEqual(CardData.CardOwner.Jogasaki, cp.cardOwner);
    }
}
