using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// 끝없는 굴레(ReloadDraw): 재장전할 때마다 스택 수만큼 카드를 뽑는다. DrawEffect.scaling 기본값은 기존 카드에 영향이 없어야 한다.
public class ReloadDrawTests
{
    private const string ReloadDrawPath = "Assets/Data/Status/ReloadDraw.asset";
    private const string SniperPath = "Assets/Data/Weapons/스나이퍼.asset";

    private readonly List<UnityEngine.Object> _assets = new();
    private GameObject _battleObject;
    private BattleManager _previousBattle, _battle;
    private BattleState _state;
    private StatusDefinition _reloadDraw;

    private static void Singleton(Type type, object value) => type.GetProperty("Instance").SetValue(null, value);

    [SetUp]
    public void SetUp()
    {
        _previousBattle = BattleManager.Instance;
        _battleObject = new GameObject("ReloadDraw test battle");
        _battle = _battleObject.AddComponent<BattleManager>();
        Singleton(typeof(BattleManager), _battle);

        _state = new BattleState { Player = new PlayerCombatant() };
        typeof(BattleManager).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_battle, _state);
        for (int i = 0; i < 6; i++)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            _assets.Add(card);
            _state.DrawPile.Add(card);
        }

        var sniper = AssetDatabase.LoadAssetAtPath<WeaponData>(SniperPath);
        Assert.IsNotNull(sniper);
        _state.ChangePlayerWeapon(sniper); // 재장전 이벤트가 돌도록 총기를 든다 (상태를 걸기 전이라 아무것도 뽑지 않음)

        _reloadDraw = AssetDatabase.LoadAssetAtPath<StatusDefinition>(ReloadDrawPath);
        Assert.IsNotNull(_reloadDraw);
    }

    [TearDown]
    public void TearDown()
    {
        _state?.ReleaseRuntimeCards();
        if (_battleObject != null) UnityEngine.Object.DestroyImmediate(_battleObject);
        Singleton(typeof(BattleManager), _previousBattle);
        foreach (var asset in _assets)
            if (asset != null) UnityEngine.Object.DestroyImmediate(asset);
        _assets.Clear();
    }

    private void ApplyReloadDraw() => _state.Player.AddPassive(_reloadDraw.CreateInstance(1));

    [Test]
    public void OneStack_DrawsOneCardPerReload()
    {
        ApplyReloadDraw();
        _state.ReloadAmmo();
        Assert.AreEqual(1, _state.Hand.Count);
    }

    [Test]
    public void TwoStacks_DrawTwoCardsPerReload()
    {
        ApplyReloadDraw();
        ApplyReloadDraw();
        Assert.AreEqual(2, _state.Player.Statuses.GetMagnitude(_reloadDraw), "같은 상태는 합쳐지고 스택이 더해진다");

        _state.ReloadAmmo();
        Assert.AreEqual(2, _state.Hand.Count);
    }

    [Test]
    public void DrawEffectWithoutScaling_StillDrawsFixedCount()
    {
        EffectRunner.ExecuteImmediate(new List<CardEffect> { new DrawEffect { count = 2 } },
                                      new CardContext { State = _state, Battle = _battle });
        Assert.AreEqual(2, _state.Hand.Count);
    }
}
