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
        BattleManager.Instance.PlayerTurnStart();
    }
}