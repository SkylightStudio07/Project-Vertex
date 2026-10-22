using System;
using System.Collections.Generic;
using UnityEngine;

// 스토리 챕터 하나. 로비 좌하단 스토리 카드에 "CH.2-1 영원하지 않기에 빛나는 것들 / ▸ 다음 목표"로 나온다.
// 목표를 순서대로 다 이루면 다음 챕터로 넘어간다. 진행은 MetaProgress 이정표로 판정하므로 따로 저장하지 않는다.
// 기획: Docs/기획/로비_좌하단.md
[CreateAssetMenu(fileName = "Chapter", menuName = "Game Asset/Story Chapter")]
public class StoryChapterData : ScriptableObject
{
    [Tooltip("막 번호-편 번호. 예: 2-1")]
    public string chapterId = "1-1";
    [Tooltip("블루 아카이브식 제목. 예: 영원하지 않기에 빛나는 것들")]
    public string title;
    [Tooltip("스토리 카드 오른쪽 그림")]
    public Sprite thumbnail;
    [Tooltip("위에서부터 순서대로 하나씩 보여 준다")]
    public List<StoryObjective> objectives = new();
}

public enum StoryObjectiveType
{
    [InspectorName("막 보스 격파")] ActBoss,
    [InspectorName("의뢰 완료 (스토리·구출)")] Quest,
    [InspectorName("동료 합류")] CompanionJoin,
}

[Serializable]
public class StoryObjective
{
    public StoryObjectiveType type;
    [Tooltip("막 보스 격파: 막 번호")]
    [Min(1)] public int act = 1;
    [Tooltip("의뢰 완료: 스토리·구출 의뢰 (일반 의뢰는 이정표가 남지 않아 쓸 수 없다)")]
    public QuestData quest;
    [Tooltip("동료 합류")]
    public CoopCharData companion;
    [Tooltip("비우면 자동 문구 (\"1막 보스 격파\", \"의뢰 「블랙박스 회수」 완료\", \"치하라 쇼 합류\")")]
    public string labelOverride;

    // MetaProgress 이정표 키 — 보스 첫 격파 / 스토리·구출 의뢰 완료 / 동료 첫 합류 때 남는다
    public string MilestoneKey => type switch
    {
        StoryObjectiveType.ActBoss => $"boss:act{act}",
        StoryObjectiveType.Quest => quest != null ? "quest:" + quest.questId : null,
        StoryObjectiveType.CompanionJoin => companion != null ? "join:" + companion.charID : null,
        _ => null,
    };

    public bool IsDone => MetaProgress.HasMilestone(MilestoneKey);

    public string Label
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(labelOverride)) return labelOverride;
            return type switch
            {
                StoryObjectiveType.ActBoss => $"{act}막 보스 격파",
                StoryObjectiveType.Quest => quest != null ? $"의뢰 「{quest.title}」 완료" : "의뢰 완료",
                StoryObjectiveType.CompanionJoin => companion != null ? $"{companion.charName} 합류" : "동료 합류",
                _ => "",
            };
        }
    }
}
