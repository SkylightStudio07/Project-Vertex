using System;
using System.Collections.Generic;
using UnityEngine;

// 거점의 구역 하나 (거주구 · 연구소 · 격납고 · 전진기지 · 옥상 …). 기획: Docs/기획/거점.md
// 좌표는 모두 1920×1080 화면 기준 · 왼쪽 위 원점(px). 배경이 보이는 띠는 y 110~962 (위 · 아래 바 사이).
[CreateAssetMenu(fileName = "BaseZone", menuName = "Lobby/Base Zone")]
public class BaseZoneData : ScriptableObject
{
    public enum FacilityLink { Deploy, Quest, Shop, Archive, Training, Armory }

    [Serializable]
    public class Spot
    {
        public string id;
        [Tooltip("발밑 위치 (1920×1080, 왼쪽 위 원점)")]
        public Vector2 feet;
        [Tooltip("이 자리에 선 동료 전신 키(px). 뒷줄은 작게")]
        public float height = 260f;
        public bool mirror;
        [Tooltip("켜면 이름표를 머리 왼쪽으로 뒤집는다 (오른쪽에 시설 표지가 있는 자리)")]
        public bool tagLeft;
        [Tooltip("이름표를 x로 더 미는 양(px). 왼쪽이 -")]
        public float tagShiftX;
    }

    // 배경 위에 놓는 소품. 발밑 y로 동료와 함께 앞뒤를 정렬하고 그림자를 깐다
    [Serializable]
    public class Prop
    {
        public Sprite sprite;
        [Tooltip("바닥선 가운데 (1920×1080, 왼쪽 위 원점)")]
        public Vector2 feet;
        [Tooltip("화면에서의 높이(px)")]
        public float height = 160f;
        public bool mirror;
    }

    // 눈 한 겹. 좌표 · 속도는 1920×1080 왼쪽 위 원점, 속도의 y는 아래쪽이 +
    [Serializable]
    public class SnowFx
    {
        [Tooltip("켜면 동료 앞, 끄면 동료 뒤에 그린다")]
        public bool front;
        [Tooltip("눈송이 그림. 비우면 기본 그림")]
        public Sprite sprite;
        [Tooltip("불투명한 곳에서만 눈이 보인다. 비우면 가리지 않는다")]
        public Sprite mask;
        public Rect maskArea;
        [Tooltip("눈송이가 태어나는 영역")]
        public Rect spawn;
        [Min(0)] public int count = 40;
        public Vector2 velocity = new(0f, 200f);
        public Vector2 jitter = new(20f, 30f);
        [Tooltip("반지름 범위(px)")]
        public Vector2 size = new(2f, 5f);
        [Tooltip("이동 방향으로 늘이는 배율")]
        public float stretch = 1f;
        [Tooltip("수명 범위(초)")]
        public Vector2 life = new(2f, 4f);
        [Tooltip("수명 끝에 커지는 비율. 카메라 쪽으로 오는 눈은 크게")]
        public float grow;
        [Range(0f, 1f)] public float alpha = 0.7f;
        [Tooltip("좌우로 흔들리는 폭(px)")]
        public float wobble;
    }

    public enum LightFxMode { Breathe, Flame, Faulty }

    // 배경에 그려진 불빛 위에 얹는 깜빡임
    [Serializable]
    public class LightFx
    {
        public LightFxMode mode = LightFxMode.Flame;
        [Tooltip("불빛 가운데 (1920×1080, 왼쪽 위 원점)")]
        public Vector2 center;
        public Vector2 size = new(300f, 300f);
        [Tooltip("빛 번짐 색. 알파가 가장 밝을 때의 세기")]
        public Color color = new(1f, 0.6f, 0.25f, 0.35f);
        [Range(0f, 1f), Tooltip("가장 약할 때의 밝기")]
        public float min = 0.4f;
        [Min(0.01f)] public float speed = 1f;
        [Range(0f, 1f), Tooltip("불이 꺼졌을 때 배경의 불빛을 눌러 주는 어둠의 세기")]
        public float darken;
    }

    [Serializable]
    public class FacilityPoint
    {
        public FacilityLink link;
        public string label;
        public string english;
        [Tooltip("표지 판 [x, y, w, h] (1920×1080, 왼쪽 위 원점)")]
        public Rect rect;
    }

    [Serializable]
    public class Expansion
    {
        [Tooltip("이 개발 레벨 전까지 막혀 있다 (표지만 보인다)")]
        public int devLevel;
        public Rect rect;
    }

    [Header("이름")]
    public string id;
    public string displayName;
    public string englishName;

    [Header("배경 (1920×1080)")]
    public Sprite background;

    [Header("조명 — 동료 · 소품을 방에 맞춘다")]
    [Tooltip("주 광원의 화면 x. 그림자가 이 반대쪽으로 늘어진다")]
    public float lightX = 960f;
    [Tooltip("평소 동료 색. 마우스를 올리면 흰색")]
    public Color characterTint = new(0.84f, 0.84f, 0.86f, 1f);
    [Tooltip("소품 색")]
    public Color propTint = Color.white;

    [Header("움직이는 효과")]
    public List<SnowFx> snow = new();
    public List<LightFx> lights = new();
    [Tooltip("불빛 번짐 그림. 비우면 기본 그림")]
    public Sprite lightSprite;
    [Range(0f, 1f), Tooltip("Faulty 불이 꺼질 때 방 전체가 어두워지는 세기")]
    public float faultyRoomDim;

    [Header("환경음 — 비우면 조용하다")]
    public AudioClip ambience;
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;
    [Tooltip("벽 너머처럼 먹먹하게 하려면 낮춘다 (Hz)")]
    public float ambienceCutoff = 22000f;

    [Header("해금 — 둘 다 비면 처음부터 열림")]
    [Min(0)] public int unlockDevLevel;
    public string unlockQuestId;
    [Tooltip("잠겼을 때 탭 · 잠김 판에 쓰는 조건 한 줄 (예: 의뢰 「용접공 구출」 완료 시 개방)")]
    public string unlockLabel;
    [TextArea(1, 2)] public string unlockDescription;

    [Header("동료 자리 · 시설 · 확장")]
    public List<Spot> spots = new();
    public List<Prop> props = new();
    public List<FacilityPoint> facilities = new();
    public List<Expansion> expansions = new();

    public bool IsUnlocked
    {
        get
        {
            if (unlockDevLevel > 0 && MetaProgress.DevelopmentLevel < unlockDevLevel) return false;
            if (!string.IsNullOrEmpty(unlockQuestId) && (QuestManager.Instance == null || !QuestManager.Instance.IsCompleted(unlockQuestId))) return false;
            return true;
        }
    }
}
