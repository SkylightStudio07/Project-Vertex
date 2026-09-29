using System.Collections.Generic;
using UnityEngine;

public enum EnemyActivityPatternType
{
    Sequential, // 순차 실행
    Random      // 랜덤 실행
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Asset/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int health;
    public Sprite enemyImage;

    [Header("스프라이트 연출")]
    [Tooltip("스프라이트 크기 배율 (기본값 1.0f). 적마다 0.9, 1.2 등으로 조절 가능합니다.")]
    public float spriteScale = 1.0f;

    [Tooltip("인텐트 UI 추가 Y 오프셋 (기본 0, 필요시 머리 위 위치 미세 조정)")]
    public float intentOffsetY = 0f;

    [Header("대기(Idle) 애니메이션")]
    [Tooltip("대기 스프라이트 시트 프레임 (비어있으면 enemyImage 단독 표시)")]
    public Sprite[] idleFrames;

    [Tooltip("대기 애니메이션 초당 프레임 수 (기본 12 FPS)")]
    public float idleFrameRate = 12f;

    [Header("공격(Attack) 애니메이션")]
    [Tooltip("공격 스프라이트 시트 프레임 (비어있으면 돌진 모션 단독 재생)")]
    public Sprite[] attackFrames;

    [Tooltip("공격 애니메이션 초당 프레임 수 (기본 16 FPS)")]
    public float attackFrameRate = 16f;

    [Header("시트 여백 (대기/공격 크기 맞추기)")]
    [Tooltip("대기 시트의 여백 비율 — 캐릭터 크기 대비 한쪽 여백. 1할 = 0.1 (프레임 = 캐릭터 × (1 + 2×여백))")]
    [Range(0f, 1f)] public float idleSheetPadding = 0.1f;
    [Tooltip("공격 시트의 여백 비율. 보통 3할 = 0.3. 대기보다 여백이 크면 공격 재생 중 그만큼 키워서 캐릭터 크기·발 위치를 맞춘다.\n" +
             "우클릭 → '시트 여백 자동 측정'으로 실제 그림(알파)에서 잴 수 있다.")]
    [Range(0f, 1f)] public float attackSheetPadding = 0.3f;

    // 공격 프레임 재생 중 Image에 곱할 배율. 여백이 위아래·좌우 대칭이라 가운데 기준으로 키우면 발 위치도 맞는다.
    public float AttackScaleMultiplier => (1f + 2f * attackSheetPadding) / (1f + 2f * idleSheetPadding);

#if UNITY_EDITOR
    // 프레임별 불투명 영역 높이의 중앙값으로 여백 비율을 역산한다: 캐릭터 높이 비율 h = 1 / (1 + 2p) → p = (1/h - 1) / 2
    [ContextMenu("시트 여백 자동 측정")]
    private void MeasureSheetPadding()
    {
        float idle = MeasurePadding(idleFrames), attack = MeasurePadding(attackFrames);
        UnityEditor.Undo.RecordObject(this, "시트 여백 측정");
        if (idle >= 0f) idleSheetPadding = idle;
        if (attack >= 0f) attackSheetPadding = attack;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[EnemyData] '{enemyName}' 시트 여백 — 대기 {idleSheetPadding:0.00}, 공격 {attackSheetPadding:0.00} (공격 배율 ×{AttackScaleMultiplier:0.00})");
    }

    private static float MeasurePadding(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0 || frames[0] == null) return -1f;
        var path = UnityEditor.AssetDatabase.GetAssetPath(frames[0].texture);
        var tex = new Texture2D(2, 2);
        if (!tex.LoadImage(System.IO.File.ReadAllBytes(path))) return -1f; // 임포트 설정(Read/Write)과 무관하게 원본 PNG를 읽는다
        var scale = new Vector2((float)tex.width / frames[0].texture.width, (float)tex.height / frames[0].texture.height);
        var px = tex.GetPixels32();
        var ratios = new List<float>();
        foreach (var s in frames)
        {
            if (s == null) continue;
            Rect r = s.rect;
            int x0 = Mathf.RoundToInt(r.x * scale.x), y0 = Mathf.RoundToInt(r.y * scale.y);
            int w = Mathf.RoundToInt(r.width * scale.x), h = Mathf.RoundToInt(r.height * scale.y);
            int minY = int.MaxValue, maxY = int.MinValue;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (px[(y0 + y) * tex.width + x0 + x].a > 20) { minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); break; }
            if (maxY >= minY) ratios.Add((maxY - minY + 1f) / h);
        }
        DestroyImmediate(tex);
        if (ratios.Count == 0) return -1f;
        ratios.Sort();
        float median = ratios[ratios.Count / 2];
        return Mathf.Round((1f / median - 1f) * 0.5f * 100f) / 100f;
    }
#endif

    [Header("행동 패턴")]
    [Tooltip("행동 패턴 타입(랜덤/순차)")]
    public EnemyActivityPatternType activityPatternType;

    [Tooltip("전투 시작 시 순서대로 1회씩만 실행하는 오프닝 행동. 모두 소진되면 activityPatterns로 넘어간다.")]
    public List<EnemyAction> openingActions = new();

    [Tooltip("오프닝 이후 반복하는 행동 풀. Sequential이면 순서 순환, Random이면 매 턴 랜덤 1개.")]
    public List<EnemyAction> activityPatterns = new();

    [Header("페이즈 (보스·엘리트용, 비워 두면 위 패턴만 사용)")]
    [Tooltip("조건(HP 비율 / 동료 사망 수)을 만족하면 해당 페이즈의 행동 패턴으로 넘어간다. 위에서부터 순서대로 한 번씩만 진입")]
    public List<EnemyPhase> phases = new();
}

public enum EnemyPhaseTrigger
{
    HpRatio,        // 자신의 HP가 비율 이하
    AlliesDefeated, // 같은 전투의 다른 적이 N명 이상 쓰러짐 (3인 보스 등)
}

// 조건(HP 비율 또는 동료 사망)을 만족하면 바뀌는 행동 패턴 묶음.
[System.Serializable]
public class EnemyPhase
{
    public EnemyPhaseTrigger trigger = EnemyPhaseTrigger.HpRatio;
    [Tooltip("HpRatio: HP가 최대 HP의 이 비율 이하가 되면 진입 (0.5 = 절반)")]
    [Range(0f, 1f)] public float hpRatioThreshold = 0.5f;
    [Tooltip("AlliesDefeated: 다른 적이 이 수 이상 쓰러지면 진입")]
    [Min(1)] public int alliesDefeated = 1;
    [Tooltip("전환될 때 적 머리 위에 잠깐 띄우는 대사 (비우면 없음)")]
    [TextArea(1, 3)] public string transitionLine;
    [Tooltip("전환 직후 순서대로 1회씩 실행하는 행동")]
    public List<EnemyAction> openingActions = new();
    public EnemyActivityPatternType activityPatternType;
    public List<EnemyAction> activityPatterns = new();
}
