using UnityEngine;

// 이번 런의 집계 (결과 화면 기록 칸). 런이 시작될 때 GameManager.InitializeRun이 비운다. 저장하지 않는다.
//   - 격퇴: BattleManager.SetupEnemies (적이 쓰러질 때)
//   - 획득 카드: DeckManager.AddCardToPlayerDeck (보상·상점·합류·이벤트)
//   - 진행 턴: BattleManager.PlayerTurnStartSequence (플레이어 턴마다)
public static class RunStats
{
    public static int EnemiesDefeated { get; private set; }
    public static int CardsObtained { get; private set; }
    public static int Turns { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset()
    {
        EnemiesDefeated = 0;
        CardsObtained = 0;
        Turns = 0;
    }

    public static void AddDefeat() => EnemiesDefeated++;
    public static void AddCard() => CardsObtained++;
    public static void AddTurn() => Turns++;
}
