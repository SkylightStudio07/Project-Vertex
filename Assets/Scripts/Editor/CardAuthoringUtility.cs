using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Editor-only operations. Saved cards keep the existing two-state runtime format.
internal static class CardAuthoringUtility
{
    private static readonly FieldInfo Normal = typeof(CardData).GetField("normalState", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo Upgraded = typeof(CardData).GetField("upgradedState", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static IEnumerable<Type> TypesFor(Type baseType) => TypeCache.GetTypesDerivedFrom(baseType)
        .Where(t => !t.IsAbstract && !t.IsGenericType && t.IsSerializable
                    && !typeof(UnityEngine.Object).IsAssignableFrom(t)
                    && !Attribute.IsDefined(t, typeof(ObsoleteAttribute))
                    && t.GetConstructor(Type.EmptyTypes) != null)
        .OrderBy(t => t.Name);

    internal static void Initialize(CardData card)
    {
        Normal.SetValue(card, new CardUpgradeState { cardName = "새 카드", energyCost = 1, effects = new List<CardEffect>() });
        Upgraded.SetValue(card, new CardUpgradeState { cardName = "새 카드+", energyCost = 1, effects = new List<CardEffect>() });
        card.isUpgraded = false;
        // Both PNGs use Multiple Sprite import mode with one sliced Sprite each.
        card.cardImage = LoadDefaultSprite("Assets/Art/Cards/Default Artwork _ Player Card V2.png");
        card.CardBackground = LoadDefaultSprite("Assets/Art/Cards/Player Card V2.png");
    }

    private static Sprite LoadDefaultSprite(string path)
    {
        var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (sprite == null) Debug.LogWarning($"[Card Authoring] 기본 Sprite를 찾을 수 없습니다: {path}");
        return sprite;
    }

    internal static void CopyNormalToUpgrade(CardData card)
    {
        // Unity clones the entire managed-reference graph, preserving external asset references.
        // A struct assignment alone would alias both states' lists and nested effects.
        var clone = UnityEngine.Object.Instantiate(card);
        try
        {
            var state = (CardUpgradeState)Normal.GetValue(clone);
            state.cardName = (state.cardName ?? string.Empty) + "+";
            Undo.RegisterCompleteObjectUndo(card, "기본 상태를 강화에 복사");
            Upgraded.SetValue(card, state);
            EditorUtility.SetDirty(card);
        }
        finally { UnityEngine.Object.DestroyImmediate(clone); }
    }

    internal static string Preview(CardData card, bool upgraded)
    {
        var clone = UnityEngine.Object.Instantiate(card);
        try
        {
            clone.isUpgraded = upgraded;
            return clone.GetFullDescription(); // No battle context: no RNG or gameplay execution.
        }
        finally { UnityEngine.Object.DestroyImmediate(clone); }
    }

    internal static bool SameShape(SerializedProperty normal, SerializedProperty upgraded, int depth = 0)
    {
        if (depth > 32) return false;
        if (normal.propertyType != upgraded.propertyType) return false;
        if (normal.propertyType == SerializedPropertyType.ManagedReference &&
            normal.managedReferenceFullTypename != upgraded.managedReferenceFullTypename) return false;
        if (normal.isArray && normal.propertyType != SerializedPropertyType.String)
        {
            if (normal.arraySize != upgraded.arraySize) return false;
            for (int i = 0; i < normal.arraySize; i++)
                if (!SameShape(normal.GetArrayElementAtIndex(i), upgraded.GetArrayElementAtIndex(i), depth + 1)) return false;
            return true;
        }
        foreach (var child in Children(normal))
        {
            var other = upgraded.FindPropertyRelative(child.name);
            if (other == null || !SameShape(child, other, depth + 1)) return false;
        }
        return true;
    }

    internal static IEnumerable<SerializedProperty> Children(SerializedProperty parent)
    {
        var child = parent.Copy();
        var end = child.GetEndProperty();
        if (!child.NextVisible(true)) yield break;
        do
        {
            if (SerializedProperty.EqualContents(child, end) || child.depth <= parent.depth) yield break;
            yield return child.Copy();
        } while (child.NextVisible(false));
    }

    internal static string TypeLabel(Type type)
    {
        string korean = type.Name switch
        {
            "DamageEffect" => "피해", "BlockEffect" => "방어도", "HealEffect" => "회복",
            "DrawEffect" => "카드 뽑기", "DiscardEffect" => "손패 선택 버리기",
            "AddAmmoEffect" => "탄약 획득", "AmmoConsumeAllDamageEffect" => "탄약 전량 소비 공격",
            "ApplyStatusEffect" => "상태 적용·변경", "ApplyStatusAtEndTurnInHandEffect" => "손패 턴 종료 상태",
            "ConditionalEffect" => "조건부 효과", "RepeatEffect" => "효과 반복",
            "ChangeWeaponEffect" => "무기 변경", "AddCardToPileEffect" => "카드 생성",
            "LoseHPEffect" => "HP 손실", "GainGoldEffect" => "골드 획득",
            "IncreaseCardDrawEffect" => "드로우 수 증가", "AddCoopPointEffect" => "동료 포인트",
            "JoinCompanionEffect" => "동료 합류", _ => null
        };
        return korean == null ? ObjectNames.NicifyVariableName(type.Name) : $"{korean} ({type.Name})";
    }
}
