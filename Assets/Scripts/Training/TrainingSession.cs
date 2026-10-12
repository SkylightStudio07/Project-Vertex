using System.Collections.Generic;
using UnityEngine;

// 훈련장 출격 정보. 로비(TrainingView)가 채우고 전투 씬(GameManager)이 읽는다. 기지로 귀환할 때 비운다.
// static이라 씬을 넘어 유지된다. 기획: Docs/기획/훈련장.md
public static class TrainingSession
{
    public const int MaxCompanions = 3; // 플레이어 포함 4인

    private static readonly List<CardData> s_deck = new();
    private static readonly List<string> s_companions = new();

    public static bool IsActive { get; private set; }
    public static EnemyEncounter Encounter { get; private set; }
    public static IReadOnlyList<CardData> Deck => s_deck;
    public static IReadOnlyList<string> Companions => s_companions;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => End();

    public static void Begin(EnemyEncounter encounter, IEnumerable<CardData> deck, IEnumerable<string> companions)
    {
        Encounter = encounter;
        s_deck.Clear();
        if (deck != null) s_deck.AddRange(deck);
        s_companions.Clear();
        if (companions != null)
            foreach (string id in companions)
                if (s_companions.Count < MaxCompanions && !string.IsNullOrEmpty(id)) s_companions.Add(id);
        IsActive = encounter != null;
    }

    public static void End()
    {
        IsActive = false;
        Encounter = null;
        s_deck.Clear();
        s_companions.Clear();
    }
}
