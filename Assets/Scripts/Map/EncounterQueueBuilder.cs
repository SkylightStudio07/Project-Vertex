using System.Collections.Generic;
using System.Linq;

// 런 시작 시 전투 조우 "순서"를 미리 뽑아 큐로 만들어 두는 생성기.
// 노드에 적을 박아두던 방식(옛 MapGenerator.AssignEncounter)을 대체한다 —
// "어느 노드냐"가 아니라 "몇 번째 전투냐"로 조우가 정해지는 슬더스식 소비형 큐.
//
// 큐는 런 시작 때 시드 하나로 한 번에 생성되고(RunData가 보관), 전투 진입마다 앞에서
// 하나씩 소비된다. 소비 인덱스(RunData.combatsFought 등)는 메모리 전용 — 런이 앱 재시작을
// 넘겨 이어지지 않으므로(A안) 직렬화하지 않는다.
public static class EncounterQueueBuilder
{
    // 일반 전투 큐: 앞의 weakEncounterCount개는 약한 풀에서, 이후는 일반 풀에서 가중치 추첨한다.
    // 직전 조우와 겹치면 재추첨해 같은 적 구성이 연속으로 나오지 않게 한다(anti-repeat).
    public static List<EnemyEncounter> BuildNormalQueue(MapConfig config, int chapter, int length, System.Random rng)
    {
        var weakPool = FilterByChapter(config.weakEncounterPool, chapter);
        var normalPool = FilterByChapter(config.normalEncounterPool, chapter);
        var queue = new List<EnemyEncounter>();
        EnemyEncounter prev = null;
        for (int i = 0; i < length; i++)
        {
            bool useWeak = i < config.weakEncounterCount
                        && weakPool.Count > 0;
            var pool = useWeak ? weakPool : normalPool;

            var pick = DrawWithAntiRepeat(pool, prev, rng);
            if (pick == null) break; // 풀이 비어 있으면 거기서 큐 생성을 멈춘다 (소비 시 currentEnemies로 폴백)
            queue.Add(pick);
            prev = pick;
        }
        return queue;
    }

    // 엘리트 전투 큐: 약적 우선 개념 없이 엘리트 풀에서 가중치 추첨하고 anti-repeat를 적용한다.
    public static List<EnemyEncounter> BuildEliteQueue(MapConfig config, int chapter, int length, System.Random rng)
    {
        var elitePool = FilterByChapter(config.eliteEncounterPool, chapter);
        var queue = new List<EnemyEncounter>();
        EnemyEncounter prev = null;
        for (int i = 0; i < length; i++)
        {
            var pick = DrawWithAntiRepeat(elitePool, prev, rng);
            if (pick == null) break;
            queue.Add(pick);
            prev = pick;
        }
        return queue;
    }

    public static EnemyEncounter PickBossEncounter(MapConfig config, int chapter, System.Random rng)
    {
        var bossPool = FilterByChapter(config.bossEncounterPool, chapter);
        return DrawWeighted(bossPool, rng);
    }

    private static List<EnemyEncounter> FilterByChapter(List<EnemyEncounter> pool, int chapter)
    {
        return pool?
            .Where(encounter => encounter != null && encounter.chapter == chapter)
            .ToList()
            ?? new List<EnemyEncounter>();
    }

    // 직전 조우(previous)와 같은 조우가 뽑히면 다시 뽑는다.
    // guard는 가중치가 양수인 선택지가 사실상 하나뿐일 때 무한 루프에 빠지지 않게 하는 안전장치다.
    private static EnemyEncounter DrawWithAntiRepeat(List<EnemyEncounter> pool, EnemyEncounter previous, System.Random rng)
    {
        EnemyEncounter pick;
        int guard = 0;
        do
        {
            pick = DrawWeighted(pool, rng);
            if (pick == null) return null;
        }
        while (pick == previous && ++guard < 10);
        return pick;
    }

    private static EnemyEncounter DrawWeighted(List<EnemyEncounter> pool, System.Random rng)
    {
        if (pool == null || pool.Count == 0) return null;

        double totalWeight = 0d;
        EnemyEncounter lastEligible = null;
        foreach (var encounter in pool)
        {
            if (encounter == null || encounter.weight <= 0f) continue;
            totalWeight += encounter.weight;
            lastEligible = encounter;
        }

        if (lastEligible == null || totalWeight <= 0d) return null;

        double roll = rng.NextDouble() * totalWeight;
        double cumulativeWeight = 0d;
        foreach (var encounter in pool)
        {
            if (encounter == null || encounter.weight <= 0f) continue;
            cumulativeWeight += encounter.weight;
            if (roll < cumulativeWeight)
                return encounter;
        }

        return lastEligible;
    }
}
