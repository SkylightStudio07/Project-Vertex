using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 대화창 v1(블루아카식 구조 + 버텍스 겉옷)의 이름 줄과 넘기기 표시. DialogueView(스토리 대화)·NpcDialogueOverlay(하단 대사창) 공용.
//   이름(먹색 굵게) + 옆 작은 소속 글자(청록) / 이름 아래 캐릭터 고유색 가는 선 / 오른쪽 아래 청록 마름모 넘기기 표시(깜빡임)
//   하단 대사창은 초상 칸에 얼굴을 얹고, 초상이 없으면 칸을 숨긴다.
public class DialogueNameplate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [Tooltip("이름 아래 가는 선 (흰색 스프라이트, 캐릭터 색으로 tint)")]
    [SerializeField] private Image accentLine;
    [SerializeField] private GameObject advanceIndicator;
    [Header("초상 (하단 대사창만)")]
    [SerializeField] private GameObject portraitCell;
    [SerializeField] private Image portraitImage;

    private static readonly Color DefaultAccent = new(0.05f, 0.72f, 0.95f, 1f);
    private CanvasGroup _advanceGroup;

    public void Set(string speakerName, string affiliation, Color? accent, Sprite portrait = null)
    {
        bool hasName = !string.IsNullOrEmpty(speakerName);
        if (nameText != null)
        {
            nameText.gameObject.SetActive(hasName);
            nameText.text = string.IsNullOrEmpty(affiliation)
                ? speakerName
                : $"{speakerName}<space=0.6em><size=40%><color=#0DB8F2><cspace=0.12em>{affiliation}</cspace></color></size>";
        }
        if (accentLine != null)
        {
            accentLine.gameObject.SetActive(hasName);
            accentLine.color = accent ?? DefaultAccent;
        }
        if (portraitCell != null) portraitCell.SetActive(portrait != null);
        if (portraitImage != null && portrait != null) { portraitImage.sprite = portrait; portraitImage.preserveAspect = true; }
    }

    // 넘기기 표시: 문장이 다 나온 뒤에만, 천천히 깜빡인다
    public void SetAdvanceVisible(bool visible)
    {
        if (advanceIndicator == null || advanceIndicator.activeSelf == visible) return;
        advanceIndicator.SetActive(visible);
        if (_advanceGroup == null && !advanceIndicator.TryGetComponent(out _advanceGroup))
            _advanceGroup = advanceIndicator.AddComponent<CanvasGroup>();
        _advanceGroup.DOKill();
        if (!visible) return;
        _advanceGroup.alpha = 1f;
        _advanceGroup.DOFade(0.25f, 0.55f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(advanceIndicator);
    }
}
