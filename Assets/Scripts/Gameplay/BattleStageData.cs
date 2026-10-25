using System.Collections.Generic;
using UnityEngine;

// 전투 배경 한 장과 그 배경에 맞춘 조명 · 분위기 값. 막마다 여러 장(슬더스처럼 3~4장)을 두고 전투마다 고른다.
// 좌표는 전부 화면 비율(0~1, 좌하단 원점)이라 해상도와 무관하다.
// 붙이는 곳: BattleStageView (Canvas/Background), 연출: BattleAmbienceFx, 유닛 조명: BattleUnitLook
[CreateAssetMenu(fileName = "BattleStage", menuName = "Game Asset/Battle Stage")]
public class BattleStageData : ScriptableObject
{
    [Header("배경")]
    public Sprite background;
    [Tooltip("어느 막의 배경인지 (막 개념이 들어오면 이걸로 거른다)")]
    [Min(1)] public int actNumber = 1;

    [Header("층 범위 (일반 전투용)")]
    [Tooltip("이 배경이 나오는 층 (화면에 보이는 1층부터, 최소~최대 포함). 둘 다 0이면 아무 층에서나")]
    [Min(0)] public int minFloor;
    [Min(0)] public int maxFloor;

    [Header("전용 배경 · 아군 위치")]
    [Tooltip("이 적이 나오는 전투에서만 쓰는 배경 (엘리트 · 보스). 비우면 일반 배경 풀에서 무작위로 뽑힌다")]
    public List<EnemyData> featuredEnemies = new();
    [Tooltip("아군 진형 중심을 옮긴다 (1920×1080 기준 px, x 오른쪽 · y 위). 배경마다 바닥 높이 · 서는 자리가 달라서")]
    public Vector2 partyOffset;

    [Header("유닛 조명 — 배경의 주광에 맞춘다")]
    [Tooltip("주광이 오는 방향 (화면 기준, x 오른쪽 · y 위). 림라이트 방향")]
    public Vector2 lightFrom = new(-0.7f, 0.7f);
    public Color rimColor = new(0.78f, 0.88f, 1f, 0.65f);
    [Tooltip("아군 그림에 곱할 색 (배경 색온도 쪽으로)")]
    public Color allyGrade = new(0.92f, 0.95f, 1f, 1f);
    [Range(0.5f, 1.5f)] public float allyBrightness = 0.95f;
    public Color enemyGrade = new(0.82f, 0.9f, 1f, 1f);
    [Range(0f, 1.2f)] public float enemySaturation = 0.5f;
    [Range(0.5f, 1.5f)] public float enemyBrightness = 0.92f;

    [Header("위에서 내려오는 조명 (2.5D 무대 조명)")]
    [Tooltip("그림 위쪽(머리 · 어깨)에 더해지는 빛. a = 세기 (0이면 끔)")]
    public Color topLight = new(0.7f, 0.85f, 1f, 0.22f);
    [Range(0.5f, 6f)] public float topFalloff = 2.4f;
    [Tooltip("그림 아래쪽(다리)을 어둡게")]
    [Range(0f, 0.8f)] public float bottomShade = 0.3f;
    [Tooltip("유닛 머리 위에서 내려오는 은은한 스포트라이트 (standPools 위치마다). a = 0이면 끔")]
    public Color spotlightColor = new(0.65f, 0.82f, 1f, 0.1f);

    [Header("떠다니는 빛 알갱이")]
    [Range(0, 60)] public int moteCount = 22;
    public Color moteColor = new(0.72f, 0.88f, 1f, 0.9f);
    [Tooltip("알갱이가 생기는 영역 (화면 비율 xMin, yMin, 폭, 높이)")]
    public Rect moteArea = new(0.02f, 0.18f, 0.96f, 0.42f);

    [Header("꽃잎")]
    [Range(0, 30)] public int petalCount = 7;
    public Color petalColor = new(0.86f, 0.93f, 1f, 0.85f);

    [Header("빛줄기 (달빛 등)")]
    public bool lightShaft = true;
    [Tooltip("빛줄기 위 끝 위치 (화면 비율)")]
    public Vector2 shaftOrigin = new(0.12f, 1.02f);
    [Tooltip("기울기 (도, 0 = 수직 아래, 양수 = 오른쪽 아래로)")]
    public float shaftAngle = 28f;
    public Vector2 shaftSize = new(0.34f, 0.85f);
    public Color shaftColor = new(0.62f, 0.8f, 1f, 0.16f);

    [Header("낮은 안개")]
    public Color fogColor = new(0.55f, 0.68f, 0.82f, 0.22f);
    [Tooltip("안개 띠들의 세로 위치 (화면 비율)")]
    public List<float> fogBands = new() { 0.47f, 0.3f };

    [Header("깜빡이는 작은 광원 (경고등 등)")]
    public List<BlinkLight> blinkLights = new();

    [Header("무대 정리")]
    [Tooltip("유닛이 서는 자리 바닥의 은은한 빛 (화면 비율 중심)")]
    public List<Vector2> standPools = new() { new(0.27f, 0.36f), new(0.74f, 0.36f) };
    public Color standPoolColor = new(0.6f, 0.78f, 1f, 0.16f);
    [Tooltip("손패 뒤 아래쪽을 어둡게 (0 = 안 함)")]
    [Range(0f, 1f)] public float screenBottomShade = 0.55f;
    [Range(0f, 1f)] public float vignette = 0.45f;
    [Tooltip("밝은 배경용 HUD 장식판(흰 그라데이션 · 가장자리 판)에 곱할 색. 어두운 배경이면 어둡게 — 흰 판이 떠 보이지 않게")]
    public Color hudDecorTint = Color.white;

    [System.Serializable]
    public class BlinkLight
    {
        public Vector2 position = new(0.92f, 0.59f);
        public Color color = new(1f, 0.25f, 0.2f, 0.9f);
        [Tooltip("빛 번짐 크기 (화면 높이 비율)")]
        public float size = 0.05f;
    }
}
