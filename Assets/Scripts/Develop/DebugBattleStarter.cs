using System.Collections.Generic;
using UnityEngine;

public class DebugBattleStarter : MonoBehaviour
{
    [SerializeField] private List<CardData> deck;
    [SerializeField] private List<EnemyData> enemies;

    void Start()
    {
        BattleManager.Instance.StartBattle(enemies, deck);
        BattleManager.Instance.PlayerTurnStart();
    }
}