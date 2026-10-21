using System.Collections.Generic;
using UnityEngine;

// 챕터 순서 목록. 로비 스토리 카드가 이 목록에서 "지금 챕터"를 고른다.
[CreateAssetMenu(fileName = "StoryChapters", menuName = "Game Asset/Story Chapter List")]
public class StoryChapterList : ScriptableObject
{
    public List<StoryChapterData> chapters = new();

    public readonly struct Current
    {
        public readonly StoryChapterData chapter;
        public readonly StoryObjective next;   // 남은 첫 목표. 모든 챕터를 끝냈으면 null
        public readonly int doneCount, totalCount;
        public bool AllCleared => next == null;

        public Current(StoryChapterData chapter, StoryObjective next, int done, int total)
        {
            this.chapter = chapter; this.next = next; doneCount = done; totalCount = total;
        }
    }

    // 지금 챕터 = 목표가 하나라도 남은 첫 챕터. 다 끝났으면 마지막 챕터 + next null.
    // 목표가 비어 있는 챕터(아직 내용이 없는 다음 이야기)는 건너뛰지 않고 "준비 중"으로 멈춘다.
    public Current GetCurrent()
    {
        StoryChapterData last = null;
        foreach (var ch in chapters)
        {
            if (ch == null) continue;
            last = ch;
            if (ch.objectives.Count == 0) return new Current(ch, null, 0, 0);
            int done = 0;
            StoryObjective next = null;
            foreach (var o in ch.objectives)
            {
                if (o == null) continue;
                if (o.IsDone) done++;
                else next ??= o;
            }
            if (next != null) return new Current(ch, next, done, ch.objectives.Count);
        }
        return new Current(last, null, last != null ? last.objectives.Count : 0, last != null ? last.objectives.Count : 0);
    }
}
