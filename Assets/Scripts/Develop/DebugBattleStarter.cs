using System.Collections.Generic;
using UnityEngine;

public class DebugBattleStarter : MonoBehaviour
{
    [SerializeField] private List<CardData> deck;
    [SerializeField] private List<EnemyData> enemies;
    [SerializeField] private int debugSeed;

    void Start()
    {
        BattleManager.Instance.StartBattle(enemies, deck, debugSeed);
        BattleManager.Instance.PlayerTurnStart(false); // 전투 첫 진입이라 턴 배너는 건너뜀
    }
}