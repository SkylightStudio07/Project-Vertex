using System.Collections.Generic;

// 런 시작 시 전투 조우 "순서"를 미리 뽑아 큐로 만들어 두는 생성기.
// 노드에 적을 박아두던 방식(옛 MapGenerator.AssignEncounter)을 대체한다 —
// "어느 노드냐"가 아니라 "몇 번째 전투냐"로 조우가 정해지는 슬더스식 소비형 큐.
//
// 큐는 런 시작 때 시드 하나로 한 번에 생성되고(RunData가 보관), 전투 진입마다 앞에서
// 하나씩 소비된다. 소비 인덱스(RunData.combatsFought 등)는 메모리 전용 — 런이 앱 재시작을
// 넘겨 이어지지 않으므로(A안) 직렬화하지 않는다.
public static class EncounterQueueBuilder
{
    // 일반 전투 큐: 앞의 weakEncounterCount개는 약한 풀에서, 이후는 일반 풀에서 뽑는다.
    // 직전 조우와 겹치면 재추첨해 같은 적이 연속으로 나오지 않게 한다(anti-repeat).
    public static List<EnemyData> BuildNormalQueue(MapConfig config, int length, System.Random rng)
    {
        var queue = new List<EnemyData>();
        EnemyData prev = null;
        for (int i = 0; i < length; i++)
        {
            bool useWeak = i < config.weakEncounterCount
                        && config.weakEncounterPool != null
                        && config.weakEncounterPool.Count > 0;
            var pool = useWeak ? config.weakEncounterPool : config.normalEncounterPool;

            var pick = DrawWithAntiRepeat(pool, prev, rng);
            if (pick == null) break; // 풀이 비어 있으면 거기서 큐 생성을 멈춘다 (소비 시 currentEnemies로 폴백)
            queue.Add(pick);
            prev = pick;
        }
        return queue;
    }

    // 엘리트 전투 큐: 약적 우선 개념 없이 엘리트 풀에서만 뽑고, anti-repeat만 적용.
    public static List<EnemyData> BuildEliteQueue(MapConfig config, int length, System.Random rng)
    {
        var queue = new List<EnemyData>();
        EnemyData prev = null;
        for (int i = 0; i < length; i++)
        {
            var pick = DrawWithAntiRepeat(config.eliteEncounterPool, prev, rng);
            if (pick == null) break;
            queue.Add(pick);
            prev = pick;
        }
        return queue;
    }

    // 직전 조우(previous)와 같은 적이 뽑히면 다시 뽑는다. 풀 원소가 1개뿐이면 재추첨이 무의미하므로 그대로 반환.
    // guard는 사실상 선택지가 하나뿐인 풀에서 무한 루프에 빠지지 않게 하는 안전장치.
    private static EnemyData DrawWithAntiRepeat(List<EnemyData> pool, EnemyData previous, System.Random rng)
    {
        if (pool == null || pool.Count == 0) return null;
        if (pool.Count == 1) return pool[0];

        EnemyData pick;
        int guard = 0;
        do
        {
            pick = pool[rng.Next(0, pool.Count)];
        }
        while (pick == previous && ++guard < 10);
        return pick;
    }
}
