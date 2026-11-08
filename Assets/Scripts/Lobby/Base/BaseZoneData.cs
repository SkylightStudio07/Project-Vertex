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

    [Header("해금 — 둘 다 비면 처음부터 열림")]
    [Min(0)] public int unlockDevLevel;
    public string unlockQuestId;
    [Tooltip("잠겼을 때 탭 · 잠김 판에 쓰는 조건 한 줄 (예: 의뢰 「용접공 구출」 완료 시 개방)")]
    public string unlockLabel;
    [TextArea(1, 2)] public string unlockDescription;

    [Header("동료 자리 · 시설 · 확장")]
    public List<Spot> spots = new();
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
