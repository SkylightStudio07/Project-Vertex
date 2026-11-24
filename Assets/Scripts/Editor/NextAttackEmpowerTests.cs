using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// 다음 공격 카드 강화 (정신 집중): 활력(덧셈) + 배율 상태(×1.5, 저격총일 때 카드가 추가로 건다).
// 합 → 곱 순서, 플레이어 공격 카드가 끝나면 둘 다 소모.
public class NextAttackEmpowerTests
{
    private const string VigorPath = "Assets/Data/Status/Vigor.asset";
    private const string DoublePath = "Assets/Data/Status/NextAttackDouble.asset";
    private const string StrengthPath = "Assets/Data/Status/Strength.asset";
    private const string SniperPath = "Assets/Data/Weapons/스나이퍼.asset";
    private const string PistolPath = "Assets/Data/Weapons/권총.asset";

    private readonly List<UnityEngine.Object> _assets = new();
    private GameObject _battleObject, _gameObject;
    private GameManager _previousGame;
    private BattleManager _previousBattle, _battle;
    private BattleState _state;
    private EnemyInstance _enemy;
    private StatusDefinition _vigor, _double;

    private static void Singleton(Type type, object value) => type.GetProperty("Instance").SetValue(null, value);
    private static void Field(object target, string name, object value)
        => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(target, value);

    [SetUp]
    public void SetUp()
    {
        // 플레이어 HP(생존 판정)는 GameManager가 들고 있어 자신 대상 효과에 필요하다
        _previousGame = GameManager.Instance;
        _gameObject = new GameObject("NextAttackEmpower test game");
        var game = _gameObject.AddComponent<GameManager>();
        Singleton(typeof(GameManager), game);
        typeof(GameManager).GetProperty("PlayerHP").SetValue(game, 80);

        _previousBattle = BattleManager.Instance;
        _battleObject = new GameObject("NextAttackEmpower test battle");
        _battle = _battleObject.AddComponent<BattleManager>();
        Singleton(typeof(BattleManager), _battle);

        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.health = 500;
        _assets.Add(data);
        _enemy = new EnemyInstance(data);
        _state = new BattleState { Player = new PlayerCombatant(), Enemies = { _enemy } };
        typeof(BattleManager).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_battle, _state);

        _vigor = AssetDatabase.LoadAssetAtPath<StatusDefinition>(VigorPath);
        _double = AssetDatabase.LoadAssetAtPath<StatusDefinition>(DoublePath);
        Assert.IsNotNull(_vigor);
        Assert.IsNotNull(_double);
    }

    [TearDown]
    public void TearDown()
    {
        _state?.ReleaseRuntimeCards();
        if (_battleObject != null) UnityEngine.Object.DestroyImmediate(_battleObject);
        if (_gameObject != null) UnityEngine.Object.DestroyImmediate(_gameObject);
        Singleton(typeof(BattleManager), _previousBattle);
        Singleton(typeof(GameManager), _previousGame);
        foreach (var asset in _assets)
            if (asset != null) UnityEngine.Object.DestroyImmediate(asset);
        _assets.Clear();
    }

    private CardData MakeCard(CardData.CardType type, params CardEffect[] effects)
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        _assets.Add(card);
        Field(card, "cardType", type);
        Field(card, "normalState", new CardUpgradeState { cardName = "test", energyCost = 0, effects = new List<CardEffect>(effects) });
        return card;
    }

    private CardData Attack(int damage, int hits = 1)
        => MakeCard(CardData.CardType.Attack, new DamageEffect { amount = damage, hitCount = hits, hitInterval = 0f });

    // 실전과 같이: 효과 실행 → 카드 해결 알림. 적에게 들어간 피해를 돌려준다
    private int Play(CardData card)
    {
        int before = _enemy.HP;
        var ctx = new CardContext { State = _state, Battle = _battle, Card = card, Target = _enemy, AllEnemies = _state.Enemies };
        EffectRunner.ExecuteImmediate(card.ActiveEffects, ctx);
        AmmoStatusEvents.NotifyCardResolved(_state, ctx);
        return before - _enemy.HP;
    }

    // 사용자가 만들 카드와 같은 구성: 활력 N + (저격총이면) 정조준
    private CardData EmpowerCard(int vigor) => MakeCard(CardData.CardType.Skill,
        new ApplyStatusEffect { status = _vigor, amount = vigor, targets = EffectTargetSelector.Source },
        new ConditionalEffect
        {
            condition = new PlayerWeaponCondition { weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(SniperPath) },
            effectsWhenMet = new List<CardEffect> { new ApplyStatusEffect { status = _double, amount = 1, targets = EffectTargetSelector.Source } },
        });

    [Test]
    public void Vigor_AddsToEveryHitOfNextAttackCard_ThenConsumed()
    {
        _state.Player.AddPassive(_vigor.CreateInstance(4));
        Assert.AreEqual(3 * (5 + 4), Play(Attack(5, 3)), "3타 모두 +4");
        Assert.IsFalse(_state.Player.Statuses.Has(_vigor), "플레이어 공격 카드가 끝나면 활력 소모");
        Assert.AreEqual(5, Play(Attack(5)));
    }

    [Test]
    public void Multiplier_AppliesAfterAllAdditions_ThenConsumed()
    {
        _state.Player.AddPassive(AssetDatabase.LoadAssetAtPath<StatusDefinition>(StrengthPath).CreateInstance(3));
        _state.Player.AddPassive(_vigor.CreateInstance(4));
        _state.Player.AddPassive(_double.CreateInstance(1));

        Assert.AreEqual(24, Play(Attack(9)), "(기본 9 + 활력 4 + 힘 3) × 1.5");
        Assert.IsFalse(_state.Player.Statuses.Has(_double));
        Assert.AreEqual(9 + 3, Play(Attack(9)), "소모 후엔 힘만");
    }

    [Test]
    public void SkillCards_DoNotConsumeEmpower()
    {
        _state.Player.AddPassive(_vigor.CreateInstance(4));
        _state.Player.AddPassive(_double.CreateInstance(1));
        Play(MakeCard(CardData.CardType.Skill, new BlockEffect { amount = 1 }));
        Assert.IsTrue(_state.Player.Statuses.Has(_vigor));
        Assert.IsTrue(_state.Player.Statuses.Has(_double));
    }

    [Test]
    public void EmpowerCard_WithSniper_GivesVigorAndDouble()
    {
        _state.ChangePlayerWeapon(AssetDatabase.LoadAssetAtPath<WeaponData>(SniperPath));
        Play(EmpowerCard(4));
        Assert.AreEqual(21, Play(Attack(10)), "(10 + 활력 4) × 1.5");
    }

    [Test]
    public void EmpowerCard_WithoutSniper_GivesOnlyVigor()
    {
        _state.ChangePlayerWeapon(AssetDatabase.LoadAssetAtPath<WeaponData>(PistolPath));
        Play(EmpowerCard(4));
        Assert.IsFalse(_state.Player.Statuses.Has(_double));
        Assert.AreEqual(10 + 4, Play(Attack(10)));
    }
}
