using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로비 좌하단 스토리 카드 (명일방주 배너 자리). "02 / STORY CH.1-2 / 빗속에서 건넨 손 / ▶ 다음 목표: …".
// 누르면 다음 목표가 의뢰면 의뢰 게시판, 그 밖(보스·합류)이면 탐색 개시로 간다.
// 기획: Docs/기획/로비_좌하단.md
public class LobbyStoryCard : MonoBehaviour
{
    [SerializeField] private StoryChapterList chapters;
    [SerializeField] private TextMeshProUGUI chapterText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI nextText;
    [SerializeField] private Image thumbnail;
    [SerializeField] private GameObject nextMark;     // "다음 목표" 앞 청록 삼각형
    [SerializeField] private Button button;
    [Header("이동")]
    [SerializeField] private Button questBoardButton;  // 메뉴의 의뢰 버튼
    [SerializeField] private Button expeditionButton;  // 메뉴의 탐색 개시 버튼

    private StoryObjective _next;

    private void Awake()
    {
        if (button != null) button.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable() => MetaProgress.OnChanged -= Refresh;

    private void Refresh()
    {
        if (chapters == null) return;
        var cur = chapters.GetCurrent();
        _next = cur.next;
        if (cur.chapter == null) return;

        if (chapterText != null) chapterText.text = $"02 / STORY CH.{cur.chapter.chapterId}";
        if (titleText != null) titleText.text = cur.chapter.title;
        if (nextText != null)
        {
            nextText.text = cur.next != null ? $"다음 목표: {cur.next.Label}"
                : cur.totalCount == 0 ? "다음 이야기 준비 중"
                : "CLEAR";
        }
        if (nextMark != null) nextMark.SetActive(cur.next != null);
        if (thumbnail != null)
        {
            thumbnail.sprite = cur.chapter.thumbnail;
            thumbnail.enabled = cur.chapter.thumbnail != null;
        }
    }

    private void OnClick()
    {
        if (_next == null) return;
        var target = _next.type == StoryObjectiveType.Quest ? questBoardButton : expeditionButton;
        if (target != null && target.interactable) target.onClick.Invoke();
    }
}
