using System.Collections.Generic;

// 런 내 모든 결정론적 난수 스트림의 통합 관리자.
// "무엇이 어떤 salt를 쓰는지"가 RngStream 열거형 하나에 모여 있어, 새 용도를 추가할 때
// 여기만 보면 충돌 없이 번호를 고를 수 있다.
//
// 보관형이지만 일회용처럼 동작한다:
// 스트림별 Random 인스턴스를 보관하되, 유일한 출입구인 For()가 호출될 때마다
// 노드 좌표로 재시드된 새 인스턴스로 갈아끼운다. 재시드 없이 보관분을 꺼내는
// getter는 의도적으로 없다 — 그 순간 소비 순서 의존성(같은 노드인데 경로에 따라
// 결과가 달라지는 문제)과 세이브 시 소비 카운터 저장 문제가 부활한다.
// (상세: 노션 "보상 테이블 결정론적 난수 문제")
public enum RngStream
{
    Reward = 1, // 전투 보상 생성 (BattleReward)
    Battle = 2, // 전투 내 난수 — 셔플·드로우·랜덤 타겟 (BattleManager._rnd)
    Event  = 3, // 이벤트 노드의 이벤트 선택 (MapUIController)
    // 새 용도는 여기에 추가. 번호는 기존과 겹치지만 않으면 됨.
    // 단, 한 번 배정한 번호를 바꾸면 같은 맵시드가 다른 결과를 뱉게 되므로 변경 금지.
}

public static class RunRng
{
    private static readonly Dictionary<RngStream, System.Random> held = new();

    // 유일한 출입구 — 호출 시마다 재시드된 Random을 보관하고 반환한다.
    // 같은 (스트림, 층, 노드)로 다시 불러도 같은 시드에서 새로 시작하므로 멱등하다.
    public static System.Random For(RngStream stream, int floor, int node)
    {
        var rng = new System.Random(SeedFor(stream, floor, node));
        held[stream] = rng;
        return rng;
    }

    // Random 객체 대신 시드 정수가 필요한 곳용.
    // BattleManager.StartBattle처럼 자기 스코프(전투 하나) 동안 스트림을 직접
    // 보관·소비하는 시스템은 시드만 받아서 내부에서 Random을 만든다.
    public static int SeedFor(RngStream stream, int floor, int node)
        => SeedUtil.Mix(RunData.Instance.mapData.seed, floor, node, (int)stream);
}
