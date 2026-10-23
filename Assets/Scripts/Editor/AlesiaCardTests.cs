using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// 알레시아(Cp_02) 카드: 투척 창술 → 회수 순환, 창 속성, 위압, 대형 유지(한 턴 장수 제한), 방패 후려치기(민첩 비례), 파워 발동
public class AlesiaCardTests
{
    private const string Cards = "Assets/Data/Cards/CH_02/";
    private const string Status = "Assets/Data/Status/";
    private readonly List<UnityEngine.Object> _assets = new();
    private BattleState _state;
    private EnemyInstance _enemy;
    private GameObject _gameObject, _battleObject;
    private GameManager _previousGame;
    private BattleManager _previousBattle, _battle;

    private static void Singleton(Type type, object value) => type.GetProperty("Instance").SetValue(null, value);

    private static CardData Card(string id) => AssetDatabase.LoadAssetAtPath<CardData>($"{Cards}CH_02_{id}.asset");
    private static StatusDefinition Def(string path) => AssetDatabase.LoadAssetAtPath<StatusDefinition>($"{Status}{path}.asset");

    [SetUp]
    public void SetUp()
    {
        _previousGame = GameManager.Instance; _previousBattle = BattleManager.Instance;
        _gameObject = new GameObject("Alesia test game");
        var game = _gameObject.AddComponent<GameManager>();
        Singleton(typeof(GameManager), game);
        typeof(GameManager).GetProperty("PlayerHP").SetValue(game, 80);
        _battleObject = new GameObject("Alesia test battle");
        _battle = _battleObject.AddComponent<BattleManager>();
        Singleton(typeof(BattleManager), _battle);

        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.health = 100;
        _assets.Add(data);
        _enemy = new EnemyInstance(data);
        _state = new BattleState { Player = new PlayerCombatant(), Enemies = { _enemy } };
        typeof(BattleManager).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_battle, _state);
    }

    [TearDown]
    public void TearDown()
    {
        _state?.ReleaseRuntimeCards();
        if (_battleObject != null) UnityEngine.Object.DestroyImmediate(_battleObject);
        if (_gameObject != null) UnityEngine.Object.DestroyImmediate(_gameObject);
        Singleton(typeof(GameManager), _previousGame); Singleton(typeof(BattleManager), _previousBattle);
        foreach (var asset in _assets)
            if (asset != null) UnityEngine.Object.DestroyImmediate(asset);
        _assets.Clear();
    }

    private CardContext Ctx(CardData card) => new() { State = _state, Battle = _battle, Card = card, Target = _enemy, AllEnemies = _state.Enemies };

    private void PlayEffects(CardData card) => EffectRunner.ExecuteImmediate(card.ActiveEffects, Ctx(card));

    private void Give(StatusDefinition def, int stacks) => _state.Player.AddPassive(def.CreateInstance(stacks));

    [Test]
    public void Recall_OnlyPlayableWhileSpearIsExhausted_AndBringsItBack()
    {
        var recall = _state.CreateCard(Card("C04"));
        Assert.IsFalse(recall.IsPlayConditionMet(_state), "창이 소멸 더미에 없으면 못 쓴다");

        var spear = _state.CreateCard(Card("C02"));
        _state.ExhaustPile.Add(spear);
        Assert.IsTrue(recall.IsPlayConditionMet(_state));

        PlayEffects(recall);
        Assert.AreEqual(95, _enemy.HP, "회수 피해 5");
        Assert.Contains(spear, _state.Hand, "던진 창이 손으로 돌아온다");
        Assert.IsFalse(_state.ExhaustPile.Contains(spear));
    }

    [Test]
    public void Spear_AppliesOnlyTheCurrentElement()
    {
        Give(Def("CH_02/SpearElement_Weak"), 1);
        PlayEffects(Card("C02"));

        Assert.AreEqual(90, _enemy.HP, "창 피해 10");
        Assert.IsTrue(_state.DiscardPile.Exists(c => c.BaseCardName == "회수"), "버린 더미에 회수");
        Assert.IsTrue(_enemy.Statuses.Has(Def("Weak")));
        Assert.IsFalse(_enemy.Statuses.Has(Def("Vulnerable")));
        Assert.IsFalse(_enemy.Statuses.Has(Def("Poison")));
    }

    [Test]
    public void ElementChange_ReplacesThePreviousElement()
    {
        PlayEffects(Card("C10")); // 취약
        PlayEffects(Card("C12")); // 독성
        Assert.IsFalse(_state.Player.Statuses.Has(Def("CH_02/SpearElement_Vulnerable")));
        Assert.IsTrue(_state.Player.Statuses.Has(Def("CH_02/SpearElement_Poison")));
    }

    [Test]
    public void Intimidate_HalvesDamageFromEnemiesHitBySpear_PlusQuarters()
    {
        Give(Def("CH_02/Intimidate"), 1);
        PlayEffects(Card("C02"));
        Assert.AreEqual(10, Outgoing(20), "위압: 50% 감소");

        TearDown(); SetUp();
        Give(Def("CH_02/Intimidate"), 1);
        Give(Def("CH_02/IntimidatePlus"), 1);
        PlayEffects(Card("C02"));
        Assert.AreEqual(5, Outgoing(20), "위압+: 75% 감소 (둘 다 있어도 한 번)");
    }

    private int Outgoing(int amount)
    {
        var info = new DamageInfo(amount, _enemy);
        foreach (var p in _enemy.Passives) info = p.ModifyOutgoingDamage(info, _state);
        return info.Amount;
    }

    [Test]
    public void Formation_LimitsCardsAndBuffsThisTurnOnly()
    {
        var formation = Card("C07");
        PlayEffects(formation);
        Assert.AreEqual(3, _state.Player.Statuses.GetMagnitude(Def("Strength")));
        Assert.AreEqual(3, _state.Player.Statuses.GetMagnitude(Def("Dexterity")));

        _state.CardsPlayedThisTurn = 2;
        Assert.IsTrue(_state.Player.Statuses.AllowsCardPlay(Ctx(formation)));
        _state.CardsPlayedThisTurn = 3;
        Assert.IsFalse(_state.Player.Statuses.AllowsCardPlay(Ctx(formation)), "3장째 이후 막힘");

        _state.Player.Statuses.NotifyTurnEnd(_state, _state.Player);
        Assert.AreEqual(0, _state.Player.Statuses.GetMagnitude(Def("Strength")), "힘은 이번 턴만");
        Assert.AreEqual(0, _state.Player.Statuses.GetMagnitude(Def("Dexterity")), "민첩은 이번 턴만");
        Assert.IsTrue(_state.Player.Statuses.AllowsCardPlay(Ctx(formation)), "장수 제한도 이번 턴만");
    }

    [Test]
    public void Formation_KeepsDexterityGainedElsewhere()
    {
        Give(Def("Dexterity"), 2);
        PlayEffects(Card("C07"));
        _state.Player.Statuses.NotifyTurnEnd(_state, _state.Player);
        Assert.AreEqual(2, _state.Player.Statuses.GetMagnitude(Def("Dexterity")), "원래 있던 민첩 2는 남는다");
    }

    [Test]
    public void ShieldBash_ScalesWithDexterity()
    {
        Give(Def("Dexterity"), 2);
        PlayEffects(Card("C03"));
        Assert.AreEqual(90, _enemy.HP, "민첩 2 × 5");
    }

    [Test]
    public void Rotation_GainsDexterityOnlyWhenRecallIsPlayed()
    {
        Give(Def("CH_02/Rotation"), 1);
        var dex = Def("Dexterity");

        _state.Player.Statuses.NotifyCardPlayed(Ctx(_state.CreateCard(Card("C01"))), _state.Player);
        Assert.AreEqual(0, _state.Player.Statuses.GetMagnitude(dex), "다른 카드는 무시");

        _state.Player.Statuses.NotifyCardPlayed(Ctx(_state.CreateCard(Card("C04"))), _state.Player);
        Assert.AreEqual(1, _state.Player.Statuses.GetMagnitude(dex));
    }

    [Test]
    public void DefenseLine_GainsDexterityWhenHpDamageIsTaken()
    {
        Give(Def("CH_02/DefenseLine"), 1);
        _state.Player.Statuses.NotifyAfterDamageTaken(_state, _state.Player, _enemy, 4, null);
        Assert.AreEqual(1, _state.Player.Statuses.GetMagnitude(Def("Dexterity")));
    }

    [Test]
    public void Companion_DataIsWired()
    {
        var cp = AssetDatabase.LoadAssetAtPath<CoopCharData>("Assets/Data/Cooperator/Cp_02.asset");
        Assert.AreEqual("알레시아", cp.charName);
        Assert.AreEqual(Card("C02"), cp.joinRewardCard);
        Assert.IsNotNull(cp.skill);
        Assert.IsNotNull(cp.standingSprite);
        foreach (var c in cp.OwnedCards()) Assert.AreEqual(CardData.CardOwner.BrassKnight, c.Owner, c.name);
    }
}
