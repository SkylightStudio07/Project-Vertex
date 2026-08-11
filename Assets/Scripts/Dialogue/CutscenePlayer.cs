using System;
using System.Collections.Generic;
using UnityEngine;

// CutsceneSequenceSO 하나를 받아서 순서대로 재생하는 범용 플레이어.
// 특정 시퀀스(프롤로그 등)에 종속되지 않는다 — 씬에 이 컴포넌트 하나만 두고,
// Play() 호출할 때마다 다른 CutsceneSequenceSO를 넘기면 챕터 인트로/엔딩 등 어디서든 재사용 가능.
// CutsceneView/DialogueView는 서로의 존재를 몰라도 된다 — 각자 Play(json, onComplete)만
// 구현하면 되고, "다음에 뭘 재생할지"는 이 클래스가 시퀀스 순서대로만 정한다.
public class CutscenePlayer : MonoBehaviour
{
    [SerializeField] private CutsceneView cutsceneView;
    [SerializeField] private DialogueView dialogueView;

    private List<CutsceneSequenceSO.Segment> segments;
    private int index;
    private Action onComplete;

    public void Play(CutsceneSequenceSO sequence, Action onComplete)
    {
        if (sequence == null || sequence.segments == null || sequence.segments.Count == 0)
        {
            Debug.LogWarning("[Cutscene] 재생할 CutsceneSequenceSO가 비어있음. 즉시 완료 처리.");
            onComplete?.Invoke();
            return;
        }

        segments = sequence.segments;
        this.onComplete = onComplete;
        index = 0;
        PlayNext();
    }

    private void PlayNext()
    {
        if (segments == null || index >= segments.Count)
        {
            onComplete?.Invoke();
            return;
        }

        CutsceneSequenceSO.Segment segment = segments[index++];
        if (segment == null || segment.json == null)
        {
            Debug.LogWarning($"[Cutscene] segments[{index - 1}]이 비어있음. 건너뜀.");
            PlayNext();
            return;
        }

        switch (segment.type)
        {
            case CutsceneSequenceSO.Segment.SegmentType.Cutscene:
                if (cutsceneView == null)
                {
                    Debug.LogWarning("[Cutscene] cutsceneView가 비어있음. Inspector 연결 확인 필요. 건너뜀.");
                    PlayNext();
                    return;
                }
                cutsceneView.Play(segment.json, PlayNext);
                break;

            case CutsceneSequenceSO.Segment.SegmentType.Dialogue:
                if (dialogueView == null)
                {
                    Debug.LogWarning("[Cutscene] dialogueView가 비어있음. Inspector 연결 확인 필요. 건너뜀.");
                    PlayNext();
                    return;
                }
                dialogueView.Play(segment.json, PlayNext);
                break;
        }
    }
}
