using System.Collections.Generic;
using UnityEngine;

// 런 전체에서 유지되는 상태 데이터.
// 맵, 현재 위치, 덱, HP 등 런이 끝날 때까지 살아있어야 하는 데이터를 여기서 관리한다.
public class RunData : MonoBehaviour
{

    // 차후 게임매니저가 들고 있는 hp, player 덱 등은 다 여기서 원칙적으로 관리해야 함.
    // 아직은 맵 관련 처리하고 있으니 여기서 일단 멈춤.

    public static RunData Instance { get; private set; }

    // ==== 상점(카드 제거) ====
    private const int CardRemoveBasePrice = 75;
    private const int CardRemovePriceIncrement = 25;
    public int cardRemoveCount = 0;
    public int GetCardRemovePrice() => CardRemoveBasePrice + CardRemovePriceIncrement * Instance.cardRemoveCount;


    [Header("맵")]
    public MapData mapData;          // MapGenerator가 생성한 맵 전체 구조
    public int currentFloor;         // 현재 층 인덱스
    public int currentNodeIndex;     // 현재 층에서의 노드 인덱스

    // Inspector에서 현재 노드 타입 확인용. 
    // MapManager가 이동할 때마다 갱신한다.
    // 프로퍼티라 field 문법 사용
    [field: SerializeField] public NodeType CurrentNodeType { get; set; }

    // 현재 위치의 MapNode를 바로 꺼내는 편의 프로퍼티
    public MapNode CurrentNode => mapData?.GetNode(currentFloor, currentNodeIndex);

    // ── 전투 조우 큐 (런/막 시작 시 생성, 메모리 전용) ──────────────────
    // "몇 번째 전투냐"로 조우를 정하는 소비형 큐. 설계 상세는 EncounterQueueBuilder 참고.
    // [NonSerialized] — 저장하지 않는다(A안: 런은 앱 재시작을 넘겨 이어지지 않음).
    [System.NonSerialized] public List<EnemyData> normalEncounterQueue = new();
    [System.NonSerialized] public List<EnemyData> eliteEncounterQueue  = new();
    [System.NonSerialized] public EnemyData bossEncounter;   // 보스는 런 시작 때 한 번 뽑아 고정
    [System.NonSerialized] public int combatsFought;         // 소비 인덱스 (일반 전투)
    [System.NonSerialized] public int elitesFought;          // 소비 인덱스 (엘리트)

    // 런(막) 시작 시 호출 — 맵 시드에서 조우 큐를 새로 뽑고 소비 카운터를 0으로 초기화한다.
    // 맵 시드가 세팅된 뒤(MapManager.InitializeMap) 불려야 한다.
    // 막(Act) 개념이 생기면 막 전환 시점에 다시 호출해 큐를 갈아끼우면 된다.
    public void BuildEncounterQueues(MapConfig config)
    {
        // 다른 난수 스트림과 겹치지 않도록 Encounter salt로 시드를 분리한다(RunRng 참고).
        var rng = new System.Random(SeedUtil.Mix(mapData.seed, 0, 0, (int)RngStream.Encounter));
        int length = config.totalFloors; // 한 런에서 치를 수 있는 전투 수 상한(층당 최대 1전투)

        normalEncounterQueue = EncounterQueueBuilder.BuildNormalQueue(config, length, rng);
        eliteEncounterQueue  = EncounterQueueBuilder.BuildEliteQueue(config, length, rng);
        bossEncounter = (config.bossEncounterPool != null && config.bossEncounterPool.Count > 0)
            ? config.bossEncounterPool[rng.Next(0, config.bossEncounterPool.Count)]
            : null;

        combatsFought = 0;
        elitesFought  = 0;
    }

    // 다음 일반 전투 조우를 꺼내고 인덱스를 전진시킨다. 큐가 바닥나면 null(호출측에서 폴백).
    public EnemyData PullNextCombatEncounter()
    {
        if (normalEncounterQueue == null || combatsFought >= normalEncounterQueue.Count) return null;
        return normalEncounterQueue[combatsFought++];
    }

    // 다음 엘리트 조우를 꺼내고 인덱스를 전진시킨다. 큐가 바닥나면 null(호출측에서 폴백).
    public EnemyData PullNextEliteEncounter()
    {
        if (eliteEncounterQueue == null || elitesFought >= eliteEncounterQueue.Count) return null;
        return eliteEncounterQueue[elitesFought++];
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
