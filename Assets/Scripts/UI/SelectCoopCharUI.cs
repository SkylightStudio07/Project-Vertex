using UnityEngine;
using System.Collections.Generic;


// 협력자 캐릭터 선택을 관리하는 통합 UI
public class SelectCoopCharUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectedUI;
    [SerializeField] private List<SelectCoopCharBtn> selectCoopCharBtns;
    [SerializeField] private FadeController fadeController;
    [SerializeField] private DialogueView dialogueView;

    public DialogueView DialogueView => dialogueView;

    // 협력자 선택 이벤트 활성화 시 UI를 초기화하는 메소드

    private void Awake()
    {
        selectCoopCharBtns = new List<SelectCoopCharBtn>(GetComponentsInChildren<SelectCoopCharBtn>());
    }

    public void Init()
    {
        List<string> candidates = CollectCandidates();

        // 후보가 없으면 성소를 건너뛰고 바로 맵으로 돌아간다.
        // 이 UI는 캐릭터를 고르는 것 외에 나갈 방법(닫기 버튼)이 없어서, 빈 화면을 띄우면 갇힌다.
        if (candidates.Count == 0)
        {
            Debug.Log("[성소] 현재 층에 선택 가능한 협력자가 없어 성소를 건너뜁니다.");
            CloseUI();
            return;
        }

        fadeController.FadeIn();

        // 후보 수와 버튼 수가 다를 수 있으므로 남는 버튼은 빈 슬롯 이미지로 표시한다.
        for (int i = 0; i < selectCoopCharBtns.Count; i++)
        {
            bool hasCandidate = i < candidates.Count;
            selectCoopCharBtns[i].gameObject.SetActive(true);
            if (hasCandidate) selectCoopCharBtns[i].SetBtn(candidates[i]);
            else selectCoopCharBtns[i].SetEmptySlot();
        }
    }

    // 이번 층의 성소 후보 중 아직 합류하지 않은 캐릭터만 추린다.
    // 주의: GetSeletableChar()는 HolyPlaceData(SO) 내부 리스트의 참조를 그대로 반환하므로
    //       반환된 리스트를 직접 수정하면 에셋이 영구 변경된다. 반드시 새 리스트에 담는다.
    private List<string> CollectCandidates()
    {
        var candidates = new List<string>();

        if (HolyPlaceManager.Instance == null)
        {
            Debug.LogWarning("[SelectCoopCharUI] HolyPlaceManager.Instance가 없음. 씬(또는 부트 씬)에 HolyPlaceManager가 있는지 확인 필요.");
            return candidates;
        }

        List<string> selectable = HolyPlaceManager.Instance.GetSeletableChar(RunData.Instance.currentFloor);
        if (selectable == null) return candidates;

        foreach (string charID in selectable)
        {
            if (CooperationManager.Instance != null && CooperationManager.Instance.IsJoinedInRun(charID)) continue;
            candidates.Add(charID);
        }

        return candidates;
    }

    // 선택된 캐릭터 창의 위치에 선택 표시 UI를 이동시키는 메소드
    public void Selected(Transform transform)
    {
        selectedUI.position = transform.position;

    }

    public void CloseUI()
    {
        // FadeOut()을 부르면서 동시에 SetActive(false)하면 페이드 애니메이션이 재생될 틈도 없이
        // 오브젝트가 꺼져버린다. 페이드 비주얼을 살리려면 타임라인 종료 Signal에 맞춰 닫는 작업이
        // 추후 필요함 (지금은 정확성 우선 — 닫혔는데 맵 클릭이 막히는 버그를 피하는 쪽을 택함).
        gameObject.SetActive(false);
        if (MapUIController.Instance != null) MapUIController.Instance.OpenMap();
    }
}
