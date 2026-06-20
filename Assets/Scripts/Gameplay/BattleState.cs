using System.Collections.Generic;

public enum BattlePhase { PlayerTurn, EnemyTurn }

// 매니저와 독립된 전투 순수 데이터 컨테이너 (MonoBehaviour 아님)
// BattleManager가 소유·초기화하고, IPassiveLogic / CardEffect 에 파라미터로 전달한다.
public class BattleState
{
    public PlayerCombatant Player;
    public List<EnemyInstance> Enemies = new();

    public int Energy;
    public int MaxEnergy;
    public int Ammo;
    public int DrawCount;
    public int TurnNumber;
    public BattlePhase Phase;

    public List<CardData> Hand        = new();
    public List<CardData> DrawPile    = new();
    public List<CardData> DiscardPile = new();
    public List<CardData> ExhaustPile = new();
}
