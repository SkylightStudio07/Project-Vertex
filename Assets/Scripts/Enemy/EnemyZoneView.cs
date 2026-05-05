// ============================================================
// filename   : EnemyZoneView.cs
// description   : BattleManager.Enemies 참조하고 EnemyView 인스턴스 생성/갱신.
//             적 컨테이너(호리존탈 레이아웃 달린) 오브젝트에 부착.
// ============================================================

using UnityEngine;

public class EnemyZoneView : MonoBehaviour
{
    [SerializeField] private EnemyView enemyPrefab;
    [SerializeField] private Transform enemyContainer;

    private void Start()
    {
        BattleManager.Instance.OnEnemiesChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnEnemiesChanged -= Refresh;
    }

    private void Refresh()
    {
        foreach (Transform child in enemyContainer)
            Destroy(child.gameObject);

        foreach (var enemy in BattleManager.Instance.Enemies)
        {
            var view = Instantiate(enemyPrefab, enemyContainer);
            view.Bind(enemy);
        }
    }
}
