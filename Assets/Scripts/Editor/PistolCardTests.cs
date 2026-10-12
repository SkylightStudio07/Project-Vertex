using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class PistolCardTests
{
    private GameObject _gameObject, _battleObject;
    private GameManager _previousGame;
    private BattleManager _previousBattle, _battle;
    private BattleState _state;
    private static void Field(object o, string name, object value) => o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(o, value);
    private static void Singleton(Type type, object value) => type.GetProperty("Instance").SetValue(null, value);
    private static WeaponData Weapon(string name) => AssetDatabase.LoadAssetAtPath<WeaponData>($"Assets/Data/Weapons/{name}.asset");
    private CardData Card(int index, bool upgraded = false)
    {
        var card = _state.CreateCard(AssetDatabase.LoadAssetAtPath<CardData>(PistolCardBatch.PathFor(index)));
        Assert.That(card, Is.Not.Null, PistolCardBatch.Names[index]);
        card.isUpgraded = upgraded;
        return card;
    }
    [SetUp]
    public void SetUp()
    {
        _previousGame = GameManager.Instance; _previousBattle = BattleManager.Instance;
        Singleton(typeof(GameManager), null); Singleton(typeof(BattleManager), null);
        _gameObject = new GameObject("Pistol test game");
        var game = _gameObject.AddComponent<GameManager>();
        Singleton(typeof(GameManager), game);
        typeof(GameManager).GetProperty("PlayerHP").SetValue(game, 80);
        _battleObject = new GameObject("Pistol test battle");
        _battle = _battleObject.AddComponent<BattleManager>();
        Singleton(typeof(BattleManager), _battle);
        _state = new BattleState { Player = new PlayerCombatant(), Energy = 99 };
        _state.ChangePlayerWeapon(Weapon("권총"));
        Field(_battle, "_state", _state); Field(_battle, "_rnd", new System.Random(1));
        for (int i = 0; i < 6; i++) _state.DrawPile.Add(Card(0));
    }
    [TearDown]
    public void TearDown()
    {
        _state?.ReleaseRuntimeCards();
        if (_battleObject != null) Object.DestroyImmediate(_battleObject);
        if (_gameObject != null) Object.DestroyImmediate(_gameObject);
        Singleton(typeof(GameManager), _previousGame); Singleton(typeof(BattleManager), _previousBattle);
    }
    private CardContext Play(int index, Target target = null, bool upgraded = false)
    {
        var card = Card(index, upgraded);
        var ctx = new CardContext { State = _state, Battle = _battle, Card = card, PrimaryTargetOverride = target };
        var cost = _battle.GetCardPlayCost(card);
        Assert.That(_state.Ammo, Is.GreaterThanOrEqualTo(cost.Ammo));
        _state.Energy -= cost.Energy;
        _state.ConsumeAmmo(cost.Ammo, ctx);
        Run(EffectRunner.ExecuteSequence(card.ActiveEffects, ctx));
        return ctx;
    }
    private static void Run(IEnumerator enumerator)
    {
        while (enumerator.MoveNext()) if (enumerator.Current is IEnumerator nested) Run(nested);
    }
    public static IEnumerable<TestCaseData> Cards()
    {
        for (int i = 0; i < 20; i++)
            foreach (bool upgraded in new[] { false, true })
                yield return new TestCaseData(i, upgraded).SetName($"PistolCard_{i + 1}_{(upgraded ? "Upgraded" : "Normal")}");
    }
    [TestCaseSource(nameof(Cards))]
    public void AllTwentyAssetsHavePlayableEffectsAndRewardRegistration(int index, bool upgraded)
    {
        var template = AssetDatabase.LoadAssetAtPath<CardData>(PistolCardBatch.PathFor(index));
        var card = Card(index, upgraded);
        Assert.That(card.CardImage, Is.Not.Null);
        Assert.That(card.EnergyCost, Is.EqualTo(1));
        Assert.That(card.IsWeaponShootingCard, Is.False);
        Assert.That(card.ActiveEffects, Is.Not.Empty);
        Assert.That(card.GetFullDescription(), Does.Not.Contain("{"));
        Assert.That(card.GetFullDescription(), Does.Not.Contain("사격 카드"));
        var pool = AssetDatabase.LoadAssetAtPath<PlayerRewardPoolSO>(CardAuthoringCatalog.DefaultPoolPath);
        Assert.That(pool.commonCards.Count(c => c == template), Is.EqualTo(1));
        var normal = (CardUpgradeState)typeof(CardData).GetField("normalState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(card);
        var upgrade = (CardUpgradeState)typeof(CardData).GetField("upgradedState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(card);
        upgrade.cardName = normal.cardName;
        string Normalize(string json) => System.Text.RegularExpressions.Regex.Replace(json, "\"rid\":-?[0-9]+", "\"rid\":0");
        Assert.That(Normalize(JsonUtility.ToJson(upgrade)), Is.EqualTo(Normalize(JsonUtility.ToJson(normal))));
        Play(index, new Target(), upgraded);
    }
    [Test]
    public void FeedbackDamageAndBlockValuesAreApplied()
    {
        var target = new Target(); Play(0, target); Assert.That(target.Hits, Is.EqualTo(new[] { 11 }));
        _state.ReloadAmmo(); target.Hits.Clear(); Play(1, target); Assert.That(target.Hits, Is.EqualTo(new[] { 6 })); Assert.That(_state.Player.Block, Is.EqualTo(7));
        _state.Ammo = 1; target.Hits.Clear(); Play(2, target); Assert.That(target.Hits, Is.EqualTo(new[] { 18 }));
        _state.ReloadAmmo(); _state.Ammo = 1; Play(3, target); Assert.That(_state.Player.Block, Is.EqualTo(15));
        _state.ReloadAmmo(); _state.Ammo = 2; target.Hits.Clear(); Play(4, target); Assert.That(target.Hits, Is.EqualTo(new[] { 5, 5, 5 }));
    }
    [Test]
    public void MasteryAddsEightWhenBothAndPreparedDamageAppliesToEntireNextCard()
    {
        Play(5); _state.ChangePlayerWeapon(Weapon("스나이퍼"));
        var target = new Target(); Play(0, target); Assert.That(target.Hits, Is.EqualTo(new[] { 19 }));
        _state.Player.Passives.Clear(); _state.ChangePlayerWeapon(Weapon("권총"));
        Play(14); _state.Ammo = 2; target.Hits.Clear(); Play(4, target);
        Assert.That(target.Hits, Is.EqualTo(new[] { 11, 11, 11 }));
        _state.ReloadAmmo(); target.Hits.Clear(); Play(0, target); Assert.That(target.Hits, Is.EqualTo(new[] { 11 }));
    }
    [Test]
    public void MovementTracksSpentAmmoAndExpiresAtEndOfTurn()
    {
        Play(8); Play(4, new Target()); Assert.That(_state.Player.Block, Is.EqualTo(10));
        Assert.That(_state.Player.GainedBlockThisTurn, Is.True);
        var target = new Target(); Play(6, target); Assert.That(target.Hits, Is.EqualTo(new[] { 13 }));
        Assert.That(_state.Player.Block, Is.EqualTo(15));
        _state.Player.EndTurnPassives(_state); _state.ReloadAmmo(); Play(0, new Target());
        Assert.That(_state.Player.Block, Is.EqualTo(15));
        _state.Player.ResetBlockGainHistory(); Assert.That(new PlayerGainedBlockCondition().IsMet(new CardContext { State = _state }), Is.False);
    }
    [Test]
    public void ReloadAndFirstAmmoPowersTriggerOnSameWeaponAndPreviewIsReadOnly()
    {
        Play(11); Play(17);
        _state.ChangePlayerWeapon(Weapon("권총")); Assert.That(_state.Player.Block, Is.EqualTo(4));
        var card = Card(0); var target = new Target();
        EffectRunner.Preview(card.ActiveEffects, new CardContext { State = _state, Card = card, PrimaryTargetOverride = target });
        Assert.That(_state.Player.Block, Is.EqualTo(4));
        Play(0, target); Assert.That(_state.Player.Block, Is.EqualTo(9));
        Play(0, target); Assert.That(_state.Player.Block, Is.EqualTo(9));
        _state.ChangePlayerWeapon(Weapon("스나이퍼")); Play(0, target); Assert.That(_state.Player.Block, Is.EqualTo(18));
    }
    [Test]
    public void DrawReloadAndMagazineExpansionUseRuntimeWeaponState()
    {
        _state.Ammo = 0; Play(13); Assert.That(_state.Hand.Count, Is.EqualTo(2)); Assert.That(_state.Ammo, Is.EqualTo(3));
        Play(13); Assert.That(_state.Hand.Count, Is.EqualTo(2));
        Play(12); Assert.That(_state.Hand.Count, Is.EqualTo(3));
        Play(18); Assert.That(_state.Ammo, Is.EqualTo(4)); Assert.That(Weapon("권총").MaxAmmo, Is.EqualTo(3));
        _state.ChangePlayerWeapon(Weapon("스나이퍼")); Play(18); Assert.That(_state.Ammo, Is.EqualTo(1));
        _state.ChangePlayerWeapon(Weapon("권총")); Assert.That(_state.Ammo, Is.EqualTo(5));
        var fresh = new BattleState { Player = new PlayerCombatant() }; fresh.ChangePlayerWeapon(Weapon("권총")); Assert.That(fresh.Ammo, Is.EqualTo(3));
    }
    [Test]
    public void DiscountIsConsumedByNextAmmoCardAndExpires()
    {
        Play(15); var card = Card(0);
        Assert.That(_battle.GetCardPlayCost(card).Energy, Is.Zero);
        Assert.That(_battle.GetCardPlayCost(card).Energy, Is.Zero);
        Play(12); Assert.That(_battle.GetCardPlayCost(card).Energy, Is.Zero);
        Play(0, new Target()); Assert.That(_battle.GetCardPlayCost(card).Energy, Is.EqualTo(1));
        Play(15); _state.Player.EndTurnPassives(_state); Assert.That(_battle.GetCardPlayCost(card).Energy, Is.EqualTo(1));
    }
    [Test]
    public void FinisherOnlyReloadsOnItsOwnLethalHit()
    {
        var alive = new Target(); Play(19, alive); Assert.That(_state.Ammo, Is.EqualTo(2)); Assert.That(alive.Hits, Is.EqualTo(new[] { 9 }));
        var dying = new Target { HP = 9 }; Play(19, dying); Assert.That(_state.Ammo, Is.EqualTo(3));
    }
    private sealed class Target : ICombatant
    {
        public int HP { get; set; } = 500;
        public int MaxHP => 500; public int Block => 0; public bool IsDead => HP <= 0;
        public List<int> Hits = new();
        public List<IPassiveLogic> Passives { get; } = new();
        public StatusContainer Statuses { get; }
        public Target() => Statuses = new StatusContainer(Passives);
        public void TakeDamage(DamageInfo info) { Hits.Add(info.Amount); HP = Math.Max(0, HP - info.Amount); }
        public void AddBlock(int amount) { } public void Heal(int amount) { }
        public void AddPassive(IPassiveLogic p) => Statuses.Add(p);
        public void ResetBlock() { } public void RemoveExpiredPassives() => Statuses.RemoveExpired();
    }
}
