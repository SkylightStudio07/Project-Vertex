using UnityEditor;
using UnityEngine;

public static class WeaponTestCardMenu
{
    [MenuItem("Tools/Combat/Add Weapon Test Cards to Hand")]
    private static void AddToHand()
    {
        var battle = BattleManager.Instance;
        if (!Application.isPlaying || battle == null || !battle.IsInBattle)
        {
            Debug.LogWarning("[WeaponTestCardMenu] PlayMode에서 전투를 시작한 뒤 실행하세요.");
            return;
        }
        battle.AddCardsToHand(new[]
        {
            AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Cards/Player/테스트/전환사격.asset"),
            AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Cards/Player/테스트/빠른 후퇴.asset"),
        });
    }
}
