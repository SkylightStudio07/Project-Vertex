// ============================================================
// filename   : EnemyZoneView.cs
// description: BattleManager.Enemies를 참조하여 적(EnemyView)을 생성하고 배치하는 뷰 컴포넌트.
//              다수의 적 등장 시 겹침 현상을 방지하고, 2.5D 전후열 깊이감과 명확한 체력바/인텐트 간격을 보장합니다.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyZoneView : MonoBehaviour
{
    [System.Serializable]
    public class FormationPreset
    {
        public int enemyCount;
        public Vector2[] positions;

        public FormationPreset(int count, params Vector2[] pos)
        {
            enemyCount = count;
            positions = pos;
        }
    }

    [Header("프리팹 및 컨테이너")]
    [SerializeField] private EnemyView enemyPrefab;
    [SerializeField] private Transform enemyContainer;

    [Header("적 배치 프리셋 (마리 수별 오프셋)")]
    [Tooltip("적 수에 따른 각 적의 anchoredPosition 오프셋 목록입니다. 인스펙터에서 마리 수별로 세부 조정할 수 있습니다.")]
    [SerializeField] private List<FormationPreset> formationPresets = new()
    {
        new FormationPreset(1, new Vector2(50f, 0f)),
        new FormationPreset(2, new Vector2(-140f, -20f), new Vector2(140f, 15f)),
        new FormationPreset(3, new Vector2(-200f, -30f), new Vector2(0f, -5f), new Vector2(200f, 20f)),
        new FormationPreset(4, new Vector2(-240f, -35f), new Vector2(-80f, -15f), new Vector2(80f, 5f), new Vector2(240f, 25f))
    };

    private void Awake()
    {
        DisableConflictingLayoutGroup();
    }

    private void Start()
    {
        DisableConflictingLayoutGroup();
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnEnemiesChanged += Refresh;
            Refresh();
        }
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnEnemiesChanged -= Refresh;
    }

    /// <summary>
    /// 기존 HorizontalLayoutGroup 등 수동 포메이션 배치를 덮어쓰는 컴포넌트를 안전하게 비활성화합니다.
    /// </summary>
    private void DisableConflictingLayoutGroup()
    {
        Transform container = enemyContainer != null ? enemyContainer : transform;
        if (container.TryGetComponent<LayoutGroup>(out var lg) && lg.enabled)
        {
            lg.enabled = false;
        }
    }

    public void Refresh()
    {
        DisableConflictingLayoutGroup();

        Transform container = enemyContainer != null ? enemyContainer : transform;

        // 기존 생성된 뷰 정리
        var toDestroy = new List<GameObject>();
        foreach (Transform child in container)
            toDestroy.Add(child.gameObject);

        foreach (var obj in toDestroy)
        {
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        if (BattleManager.Instance == null || BattleManager.Instance.Enemies == null)
            return;

        var enemies = BattleManager.Instance.Enemies;
        int count = enemies.Count;
        if (count == 0) return;

        Vector2[] positions = GetSlotPositions(count);
        List<EnemyView> spawnedViews = new();

        for (int i = 0; i < count; i++)
        {
            if (enemies[i] == null) continue;

            var view = Instantiate(enemyPrefab, container);
            view.Bind(enemies[i]);

            RectTransform rt = view.GetComponent<RectTransform>();
            if (rt != null && i < positions.Length)
            {
                rt.anchoredPosition = positions[i];
            }

            spawnedViews.Add(view);
        }

        // 전열(y좌표가 낮고 플레이어 쪽에 가까운 적)이 후열 적보다 위에 그려지도록 SiblingIndex 정렬
        // (uGUI는 SiblingIndex가 클수록 나중에/위에 그려짐)
        spawnedViews.Sort((a, b) =>
        {
            float yA = a.GetComponent<RectTransform>().anchoredPosition.y;
            float yB = b.GetComponent<RectTransform>().anchoredPosition.y;
            return yB.CompareTo(yA); // y 내림차순: 큰 y(후열) -> 작은 y(전열)
        });

        for (int i = 0; i < spawnedViews.Count; i++)
        {
            spawnedViews[i].transform.SetSiblingIndex(i);
        }
    }

    /// <summary>
    /// 적 마리 수에 알맞은 슬롯 좌표 목록을 반환합니다.
    /// </summary>
    public Vector2[] GetSlotPositions(int count)
    {
        if (count <= 0) return Array.Empty<Vector2>();

        // 1. 인스펙터 커스텀 프리셋 확인
        if (formationPresets != null)
        {
            var match = formationPresets.Find(p => p != null && p.enemyCount == count);
            if (match != null && match.positions != null && match.positions.Length >= count)
                return match.positions;
        }

        // 2. 기본 하드코딩 프리셋
        switch (count)
        {
            case 1:
                return new[] { new Vector2(50f, 0f) };
            case 2:
                return new[] { new Vector2(-140f, -20f), new Vector2(140f, 15f) };
            case 3:
                return new[] { new Vector2(-200f, -30f), new Vector2(0f, -5f), new Vector2(200f, 20f) };
            case 4:
                return new[] { new Vector2(-240f, -35f), new Vector2(-80f, -15f), new Vector2(80f, 5f), new Vector2(240f, 25f) };
        }

        // 3. 5마리 이상 다이나믹 계산
        Vector2[] result = new Vector2[count];
        float totalSpan = Mathf.Min(600f, (count - 1) * 160f);
        float startX = 50f - (totalSpan / 2f);
        float stepX = totalSpan / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            float x = startX + (i * stepX);
            float y = Mathf.Lerp(-40f, 30f, t);
            result[i] = new Vector2(x, y);
        }

        return result;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform container = enemyContainer != null ? enemyContainer : transform;
        Gizmos.color = new Color(1f, 0.35f, 0.2f, 0.8f);

        for (int c = 1; c <= 4; c++)
        {
            var positions = GetSlotPositions(c);
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 worldPos = container.TransformPoint(new Vector3(positions[i].x, positions[i].y, 0f));
                Gizmos.DrawWireSphere(worldPos, 15f);
            }
        }
    }
#endif
}
