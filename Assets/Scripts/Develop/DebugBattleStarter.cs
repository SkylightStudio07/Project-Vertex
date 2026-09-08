using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugBattleStarter : MonoBehaviour
{
    [SerializeField] private List<CardData> deck;
    [SerializeField] private List<EnemyData> enemies;
    [SerializeField] private int debugSeed;

    void Start()
    {
        var names = enemies != null && enemies.Count > 0 ? string.Join(", ", enemies.Where(e => e != null).Select(e => e.enemyName)) : "없음";
        Debug.Log($"<color=#FBBF24>[DebugBattleStarter] 디버그 전투 시작! 적 목록: [{names}]</color>");
        BattleManager.Instance.StartBattle(enemies, deck, debugSeed);
        BattleManager.Instance.PlayerTurnStart(false); // 전투 첫 진입이라 턴 배너는 건너뜀
    }
}