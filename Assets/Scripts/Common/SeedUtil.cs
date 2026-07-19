// System.Random은 런타임 구현에 따라 가까운 시드끼리 초반 출력이 상관관계를 갖는다.
// (Unity Mono는 시드가 내부 상태 일부에만 들어가서, +1/+100 차이 시드는 첫 굴림값이
//  일정 간격으로 회전하는 격자 패턴을 만들 수 있음 — 노드마다 보상 유니크가 몰리던 원인.)
// 노드 좌표처럼 서로 조금씩만 다른 재료로 시드를 만들 땐 반드시 이 해시를 거칠 것.
public static class SeedUtil
{
    // SplitMix 계열 정수 해시 — 입력 1비트 차이가 출력 전 비트로 퍼진다(눈사태 효과).
    // 같은 입력이면 항상 같은 출력이라 "같은 노드 = 같은 결과" 결정론은 그대로 유지된다.
    // floor/node: 노드 좌표. salt: 같은 노드에서 용도별 RNG 스트림을 분리하는 구분값.
    // 게임 코드에서는 이 함수를 직접 부르지 말고 RunRng.For / RunRng.SeedFor를 쓸 것 —
    // salt 번호 명단이 RngStream 열거형 한곳에서 관리된다.
    // 주의: 재료를 더해서 하나로 합친 뒤 해시하면 안 된다 — (노드2,salt1)과 (노드1,salt2)처럼
    // 합이 같은 조합이 같은 시드가 되는 별칭 충돌이 생긴다. 반드시 재료별로 분리 투입.
    public static int Mix(int seed, int floor = 0, int node = 0, int salt = 0)
    {
        unchecked
        {
            uint x = (uint)seed;
            // 황금비 상수 곱: 0,1,2처럼 작은 재료도 큰 간격으로 벌려서 섞는다.
            // 재료 하나 넣을 때마다 Scramble로 판을 갈아 재료 간 상쇄를 방지.
            x = Scramble(x + 0x9E3779B9u * (uint)floor);
            x = Scramble(x + 0x9E3779B9u * (uint)node);
            x = Scramble(x + 0x9E3779B9u * (uint)salt);
            return (int)x;
        }
    }

    private static uint Scramble(uint x)
    {
        x ^= x >> 16; x *= 0x85EBCA6Bu;
        x ^= x >> 13; x *= 0xC2B2AE35u;
        x ^= x >> 16;
        return x;
    }
}
