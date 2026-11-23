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
    [Tooltip("속성 (여러 개 가능). 카드의 특효가 이 속성에 걸린다")]
    public EnemyTrait traits;

    [Header("마테리얼")]
    [Tooltip("적 등급 — 마테리얼이 몇 번 격퇴마다 열리는지 정한다 (일반 3 · 엘리트 2 · 보스 1). 훈련장 해금도 첫 기록 기준")]
    public EnemyEncounterType rank = EnemyEncounterType.Normal;
    [Tooltip("마테리얼 글 JSON (형식: Docs/기획/마테리얼.md)")]
    public TextAsset materialJson;

    [Header("스프라이트 연출")]
    [Tooltip("스프라이트 크기 배율 (기본값 1.0f). 적마다 0.9, 1.2 등으로 조절 가능합니다.")]
    public float spriteScale = 1.0f;

    [Tooltip("인텐트 UI 추가 Y 오프셋 (기본 0, 필요시 머리 위 위치 미세 조정)")]
    public float intentOffsetY = 0f;

    [Header("화면 고정 배치 (화면 끝에 붙는 거대 보스)")]
    [Tooltip("켜면 슬롯 대신 screenRect 화면 좌표에 그림을 놓는다. spriteScale은 무시된다")]
    public bool useScreenRect;
    [Tooltip("그림 영역 (1920×1080 기준, 좌상단 원점 x, y, 너비, 높이)")]
    public Rect screenRect;
    [Tooltip("그림 안 INTENT 기준점 (0~1, 좌하단 원점). 이 점 바로 위에 INTENT가 붙는다")]
    public Vector2 intentAnchor = new(0.5f, 1f);
    [Tooltip("그림 안 HP 바 중심 (0~1, 좌하단 원점)")]
    public Vector2 hpBarAnchor = new(0.5f, 0f);
    [Tooltip("이 적의 배치 자리를 기본 슬롯에서 옮긴다 (슬롯 단위 px, x 오른쪽 · y 위). 체력바가 겹칠 때 이 적만 따로 벌린다")]
    public Vector2 slotOffset;
    [Tooltip("HP 바 · 상태 줄을 기본 자리에서 옮긴다 (캔버스 px, y 아래가 −). 화면 고정 배치가 아닐 때만. 공격 연출이 바와 겹칠 때 쓴다")]
    public Vector2 hpBarOffset;
    [Tooltip("상태 줄만 추가로 옮긴다 (캔버스 px). 앞에 선 적의 그림에 상태 아이콘이 가릴 때 쓴다")]
    public Vector2 statusRowOffset;
    [Tooltip("공격할 때 앞으로 튀어나오지 않고 제자리에서 크기만 살짝 튄다")]
    public bool noLunge;

    [Header("대기(Idle) 애니메이션")]
    [Tooltip("대기 스프라이트 시트 프레임 (비어있으면 enemyImage 단독 표시)")]
    public Sprite[] idleFrames;

    [Tooltip("대기 애니메이션 초당 프레임 수 (기본 12 FPS)")]
    public float idleFrameRate = 12f;
    [Tooltip("켜면 대기 중 몇 초마다 그림이 멈칫했다가 툭 꺾이며 경련한다 (잠식된 시체 같은 움직임)")]
    public bool idleTwitch;

    [Header("공격(Attack) 애니메이션")]
    [Tooltip("공격 스프라이트 시트 프레임 (비어있으면 돌진 모션 단독 재생)")]
    public Sprite[] attackFrames;

    [Tooltip("켜면 공격 인텐트일 때만 공격 모션을 재생한다. 대기 · 강화 턴에는 제자리에서 행동 이름만 뜬다")]
    public bool attackMotionOnAttackOnly;

    [Tooltip("공격 애니메이션 초당 프레임 수 (기본 16 FPS)")]
    public float attackFrameRate = 16f;

    [Header("격퇴 후 대화 (비우면 평소처럼 사라진다)")]
    [Tooltip("채우면 쓰러뜨려도 그 자리에 서 있고, 보상 화면을 닫은 뒤 클릭하면 대화할 수 있다 (호감도 · 합류)")]
    public DefeatedTalkData defeatedTalk;

    [Header("공격 연출 — 키프레임 (시트 대신 포즈 몇 장 + 움직임)")]
    [Tooltip("채우면 attackFrames 대신 이 키프레임으로 공격한다 (포즈 · 위치 · 회전 · 시간). 반동으로 밀려났다 돌아오는 사격 연출 등. 돌진 모션은 쓰지 않는다")]
    public List<AttackKeyframe> attackKeyframes = new();
    [Tooltip("공격을 시작하고 몇 초 뒤에 피해가 들어가는지 (전진 0.2초 뒤부터). 음수면 전투 기본값(BattleManager)")]
    public float attackHitDelay = -1f;

    [Header("공격 연출 — 이펙트 (BattleImpactFx)")]
    [Tooltip("기본: 탄착만 / 사격: 총구 섬광 + 탄도선 / 곡사 포격: 하늘에서 포탄이 떨어져 플레이어 발밑에서 폭발 (targetBlastFrames 대신)")]
    public EnemyAttackFx attackFx = EnemyAttackFx.Default;
    [Tooltip("사격 총구 위치 — 적 그림 사각형 안의 비율 (0,0 왼쪽 아래 · 1,1 오른쪽 위). 공격 모션 중 그림이 바뀌어도 이 자리에서 쏜다")]
    public Vector2 muzzleAnchor = new(0.12f, 0.6f);
    [Tooltip("총 · 포가 실제로 나가는 공격 시트 프레임 (0부터, 점사면 여러 개). 비우면 피해가 들어가는 순간에 한 발. 곡사 포격은 첫 프레임에 포탄이 위로 솟아 화면 밖으로 나갔다가 착탄 때 떨어진다")]
    public int[] muzzleFireFrames = new int[0];
    [Tooltip("공격 시트에 총구 섬광이 이미 그려져 있으면 켠다 (탄도선만 긋는다)")]
    public bool sheetDrawsMuzzleFlash;

    [Header("근접 궤적 (attackFx = Melee)")]
    [Tooltip("휘두름 궤적 모양. 그림은 Resources/BattleFx/EnemyAction/Arc_<모양>_*.png")]
    public EnemyMeleeArc meleeArc = EnemyMeleeArc.Blunt;
    [Tooltip("궤적 크기 (1920 기준 px, 가로 × 세로). 그림 원본은 위에서 아래로 왼쪽으로 불룩한 '(' 모양. 음수면 그 축으로 뒤집는다. " +
             "위치는 muzzleAnchor(그림 칸 기준 0~1), 나오는 프레임은 muzzleFireFrames(여러 개면 연타)")]
    public Vector2 meleeArcSize = new(300f, 420f);
    [Tooltip("궤적 회전 (도, 반시계). 0 = 위에서 아래로 내려치기, -90 = 오른쪽에서 왼쪽으로 가로 휘두르기")]
    public float meleeArcAngle;
    [Tooltip("궤적에 곱할 색 (그림은 흰색). 알파 = 밝기")]
    public Color meleeArcColor = new(1f, 0.96f, 0.9f, 0.9f);
    [Tooltip("연속 프레임 휘두르기 이펙트 이름 (Resources/BattleFx/EnemyAction/<이름>_NN). 채우면 낱장 궤적 대신 이 프레임들을 " +
             "muzzleFireFrames 프레임부터 공격 시트와 같은 속도로 한 번 재생한다. 색은 meleeArcColor")]
    public string meleeSwingSheet;
    [Tooltip("이펙트를 놓을 영역 (공격 시트 한 칸 기준 0~1, 칸 밖으로 나가도 된다). 기본값 = 칸을 왼쪽으로 1/3 넓힌 영역")]
    public Rect meleeSwingRect = new(-1f / 3f, 0f, 4f / 3f, 1f);
    [Tooltip("앞에서부터 이 장수까지만 공격 시트와 1:1로 맞물리고, 그 뒤(먼지 · 잔상)는 절반 속도로 천천히 사라진다. 0 = 전부 같은 속도")]
    public int meleeSwingSyncFrames = 5;

    [Header("플레이어 피격 시트 (공격 종류와 무관)")]
    [Tooltip("공격이 플레이어에게 들어가는 순간 플레이어 위치에서 한 번 재생할 연속 프레임 이펙트 (Resources/BattleFx/EnemyAction/<이름>_NN). " +
             "사격 적은 공용 Hit_Gunshot. 비우면 없음")]
    public string playerHitSheet;
    [Tooltip("플레이어 피격 시트에 곱할 색 (그림은 흰색)")]
    public Color playerHitColor = new(1f, 0.85f, 0.65f, 1f);

    public bool HasAttackKeyframes => attackKeyframes != null && attackKeyframes.Count > 0;
    public float AttackKeyframeDuration
    {
        get
        {
            float t = 0f;
            if (attackKeyframes != null) foreach (var k in attackKeyframes) if (k != null) t += k.moveDuration + k.holdDuration;
            return t;
        }
    }

    [Header("공격 연출 — 몸놀림 (공격 시트 위에 덧입히는 이동)")]
    [Tooltip("공격 시트를 재생하는 동안 그림을 이 단계대로 순서대로 움직인다 (도약 · 반동 · 공중제비 등). 마지막 단계는 제자리(0,0)로. 채우면 돌진 · 움찔 대신 쓴다")]
    public List<AttackMotionStep> attackMotion = new();

    [System.Serializable]
    public class AttackMotionStep
    {
        [Tooltip("대기 자리 기준 위치 (캔버스 px, 왼쪽이 −)")]
        public Vector2 offset;
        [Tooltip("이 단계 끝의 회전 (도, 시계 반대가 +). 360 단위로 돌리면 공중제비")]
        public float rotation;
        [Tooltip("0보다 크면 포물선으로 뛰어서 간다 (최고점 높이 px)")]
        public float jumpHeight;
        public float duration = 0.2f;
        [Tooltip("도착한 뒤 멈춰 있는 시간")]
        public float hold;
        public DG.Tweening.Ease ease = DG.Tweening.Ease.OutQuad;
    }

    [Header("공격 연출 — 땅 파임")]
    [Tooltip("총구마다 탄착점에서 땅이 파이는 충격. 공격 프레임 번호와 터지는 자리를 한 쌍씩 적는다. 비우면 없음")]
    public List<GroundFxShot> groundFxShots = new();
    [Tooltip("충격 크기 배율")]
    public float groundFxScale = 1f;

    [System.Serializable]
    public struct GroundFxShot
    {
        [Tooltip("공격 애니메이션 프레임 번호 (0부터)")]
        public int frame;
        [Tooltip("탄착점 (그림 안 0~1, 좌하단 원점)")]
        public Vector2 point;
    }

    [Header("공격 연출 — 플레이어 위치 착탄 폭발")]
    [Tooltip("피해가 들어가는 순간 플레이어 발밑에서 터지는 폭발 프레임 (곡사 포격 등). 비우면 없음")]
    public Sprite[] targetBlastFrames;
    [Tooltip("폭발 그림 한 변의 크기 (1920×1080 기준 px)")]
    public float targetBlastSize = 640f;

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

    [Header("고유 패시브")]
    [Tooltip("전투 시작 시 이 적에게 붙는 상태 (예: 차원의 공포 '몸부림')")]
    public List<StartingStatus> startingStatuses = new();

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

[System.Serializable]
public class StartingStatus
{
    public StatusDefinition status;
    public int stacks = 1;
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

public enum EnemyAttackFx
{
    Default,
    Gunshot,
    Artillery,
    Melee,      // 무기 · 발톱을 휘두르는 순간 궤적을 그린다
}

public enum EnemyMeleeArc
{
    Blunt,  // 둔기: 굵은 초승달
    Slash,  // 베기: 가늘고 빠른 호
    Claw,   // 발톱: 나란한 세 줄
}
