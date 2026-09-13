using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

// Asset paths and reward-pool operations for the authoring UI; no runtime dependency.
internal static class CardAuthoringCatalog
{
    internal const string PlayerRoot = "Assets/Data/Cards/Player";
    internal const string DefaultPoolPath = "Assets/Data/Reward/PlayerRewardPool.asset";

    internal static string RarityFolder(CardData.CardRarity rarity) => rarity switch
    {
        CardData.CardRarity.Common => "일반", CardData.CardRarity.Rare => "희귀",
        CardData.CardRarity.Unique => "고유", _ => null
    };

    internal static string TypeFolder(CardData.CardType type) => type switch
    {
        CardData.CardType.Attack => "공격", CardData.CardType.Skill => "스킬",
        CardData.CardType.Power => "파워", _ => null
    };

    internal static bool IsUnderPlayer(string path) => !string.IsNullOrEmpty(path) &&
        (path == PlayerRoot || path.StartsWith(PlayerRoot + "/", StringComparison.Ordinal));

    internal static bool IsDefaultFolder(string path)
    {
        if (!IsUnderPlayer(path) || path == PlayerRoot) return false;
        string first = path.Substring(PlayerRoot.Length + 1).Split('/')[0];
        return first == "일반" || first == "희귀" || first == "고유";
    }

    internal static bool IsConventionalCardFolder(string path)
    {
        if (!IsDefaultFolder(path)) return false;
        string[] parts = path.Substring(PlayerRoot.Length + 1).Split('/');
        return parts.Length == 2 && (parts[1] == "공격" || parts[1] == "스킬" || parts[1] == "파워");
    }

    internal static string LocationLabel(string path)
    {
        if (!IsUnderPlayer(path)) return "Player 외부 · " + path;
        string relative = path == PlayerRoot ? "(Player 루트)" : path.Substring(PlayerRoot.Length + 1);
        return IsDefaultFolder(path) ? relative : "다른 폴더 · " + relative;
    }

    internal static List<string> PlayerFolders()
    {
        var result = new List<string>();
        if (!AssetDatabase.IsValidFolder(PlayerRoot)) return result;
        CollectFolders(PlayerRoot, result);
        return result;
    }

    private static void CollectFolders(string root, List<string> result)
    {
        result.Add(root);
        foreach (string child in AssetDatabase.GetSubFolders(root).OrderBy(path => path, StringComparer.Ordinal))
            CollectFolders(child, result);
    }

    internal static bool TryAutomaticFolder(CardData card, string chosenFolder, out string folder)
    {
        folder = chosenFolder;
        if (string.IsNullOrEmpty(folder))
        {
            string rarity = RarityFolder(card.Rarity);
            string type = TypeFolder(card.Type);
            if (rarity == null || type == null) return false;
            folder = PlayerRoot + "/" + rarity + "/" + type;
        }
        return IsUnderPlayer(folder) && AssetDatabase.IsValidFolder(folder);
    }

    internal static string NormalName(CardData card)
    {
        using var so = new SerializedObject(card);
        return so.FindProperty("normalState.cardName").stringValue?.Trim() ?? "";
    }

    internal static bool ValidFileName(string name, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(name)) error = "기본 카드 이름을 입력하세요. 이 이름이 SO 파일명이 됩니다.";
        else if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.EndsWith(".", StringComparison.Ordinal))
            error = "카드 이름에 파일명으로 사용할 수 없는 문자 또는 끝의 마침표가 있습니다.";
        else
        {
            string stem = name.Split('.')[0].ToUpperInvariant();
            if (stem == "CON" || stem == "PRN" || stem == "AUX" || stem == "NUL" ||
                System.Text.RegularExpressions.Regex.IsMatch(stem, @"^(COM|LPT)[1-9]$"))
                error = "이 이름은 Windows 예약 파일명입니다. 다른 카드 이름을 입력하세요.";
        }
        return error == null;
    }

    internal static string AssetFolder(string assetPath) => Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? "";

    internal static string ProjectFolder(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath)) return null;
        string assets = Path.GetFullPath(UnityEngine.Application.dataPath).Replace('\\', '/').TrimEnd('/');
        string chosen = Path.GetFullPath(absolutePath).Replace('\\', '/').TrimEnd('/');
        if (string.Equals(chosen, assets, StringComparison.OrdinalIgnoreCase)) return "Assets";
        return chosen.StartsWith(assets + "/", StringComparison.OrdinalIgnoreCase)
            ? "Assets/" + chosen.Substring(assets.Length + 1) : null;
    }

    internal static List<CardData> AllPlayerCards()
    {
        if (!AssetDatabase.IsValidFolder(PlayerRoot)) return new List<CardData>();
        return AssetDatabase.FindAssets("t:CardData", new[] { PlayerRoot })
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(path => path, StringComparer.Ordinal)
            .Select(AssetDatabase.LoadAssetAtPath<CardData>).Where(card => card != null).ToList();
    }

    internal static string PoolField(CardData.CardRarity rarity) => rarity switch
    {
        CardData.CardRarity.Common => "commonCards", CardData.CardRarity.Rare => "rareCards",
        CardData.CardRarity.Unique => "uniqueCards", _ => null
    };

    internal readonly struct Membership
    {
        internal readonly int Total, Matching;
        internal readonly string Buckets;
        internal bool NeedsAttention => Total > 0 && (Total != 1 || Matching != 1);
        internal string Label => Total == 0 ? "미등록" : NeedsAttention ? "등록 확인 필요 · " + Buckets : "등록됨 · " + Buckets;
        internal Membership(int total, int matching, string buckets) { Total = total; Matching = matching; Buckets = buckets; }
    }

    internal static Membership GetMembership(PlayerRewardPoolSO pool, CardData card)
    {
        if (pool == null || card == null) return default;
        int common = Count(pool.commonCards, card), rare = Count(pool.rareCards, card), unique = Count(pool.uniqueCards, card);
        int matching = card.Rarity switch
        {
            CardData.CardRarity.Common => common, CardData.CardRarity.Rare => rare,
            CardData.CardRarity.Unique => unique, _ => 0
        };
        var labels = new List<string>();
        if (common > 0) labels.Add($"일반 {common}회");
        if (rare > 0) labels.Add($"희귀 {rare}회");
        if (unique > 0) labels.Add($"고유 {unique}회");
        return new Membership(common + rare + unique, matching, string.Join(" / ", labels));
    }

    private static int Count(List<CardData> list, CardData card) => list?.Count(entry => entry == card) ?? 0;

    internal static bool RemoveFromPool(PlayerRewardPoolSO pool, CardData card)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || pool == null || card == null ||
            !EditorUtility.IsPersistent(pool) || !EditorUtility.IsPersistent(card) ||
            !AssetDatabase.GetAssetPath(pool).StartsWith("Assets/", StringComparison.Ordinal) ||
            GetMembership(pool, card).Total == 0) return false;
        using var so = new SerializedObject(pool);
        foreach (string field in new[] { "commonCards", "rareCards", "uniqueCards" })
        {
            var list = so.FindProperty(field);
            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                var element = list.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue != card) continue;
                // Clear the reference first so deletion removes the slot as well.
                element.objectReferenceValue = null;
                list.DeleteArrayElementAtIndex(i);
            }
        }
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssetIfDirty(pool);
        return true;
    }

    internal static bool AddToPool(PlayerRewardPoolSO pool, CardData card)
    {
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || pool == null || card == null ||
            !UnityEditor.EditorUtility.IsPersistent(pool) || !UnityEditor.EditorUtility.IsPersistent(card) ||
            GetMembership(pool, card).Total != 0) return false;
        string field = PoolField(card.Rarity);
        if (field == null) return false;
        using var so = new SerializedObject(pool);
        var list = so.FindProperty(field);
        list.arraySize++;
        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = card;
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssetIfDirty(pool);
        return true;
    }
}
