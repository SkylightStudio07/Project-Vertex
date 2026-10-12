using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 런 중 의뢰 진행 알림 (전투·맵 공통). 화면 오른쪽 위 MAP·DECK 버튼 아래에 2초 떴다 사라진다. 입력은 막지 않는다.
// 여러 개면 아래로 쌓이고(최대 3개, 간격 8) 앞의 것이 사라지면 위로 당겨진다.
// 틀·기호는 QuestRunSkin. 이 오브젝트는 1920×1080 Canvas 아래 화면 전체를 덮는 RectTransform이어야 한다.
public class QuestRunToastView : MonoBehaviour
{
    [SerializeField] private Vector2 origin = new(1470f, 182f); // 1920×1080 기준 왼쪽 위
    [SerializeField] private float spacing = 8f;
    [SerializeField] private int maxCount = 3;
    [SerializeField] private float holdSeconds = 2f;
    [SerializeField] private int sortingOrder = 96; // 턴 배너(95) 위

    private const float W = 420f, H = 76f;
    private readonly List<RectTransform> _live = new();
    private QuestManager _quests;

    private void Awake()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
    }

    private void OnEnable()
    {
        _quests = QuestManager.Instance;
        _quests.OnNotice += Show;
    }

    private void OnDisable()
    {
        if (_quests != null) _quests.OnNotice -= Show;
    }

    public void Show(QuestManager.Notice notice)
    {
        var skin = QuestRunSkin.Instance;
        if (skin == null || notice.Quest == null) return;

        while (_live.Count >= maxCount) Dismiss(_live[0], instant: true);

        var root = QuestRunSkin.Rect("QuestToast", transform, origin.x, origin.y + _live.Count * (H + spacing), W, H);
        var group = root.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        var bg = QuestRunSkin.Image("Bg", root, skin.Toast(notice.Kind), 0f, 0f, W, H);
        bg.preserveAspect = false;

        QuestRunSkin.Image("Icon", root, skin.Icon(notice.Quest.IconKind), 16f, 18f, 40f, 40f);

        bool accentCaption = notice.Kind is QuestManager.NoticeKind.Acquired or QuestManager.NoticeKind.Completed;
        var caption = skin.Text("Caption", root, 82f, 10f, 260f, 22f, 15f,
                                accentCaption ? QuestRunSkin.Accent : QuestRunSkin.GraphiteSoft);
        caption.text = notice.Caption;
        caption.fontStyle = FontStyles.Bold;
        var body = skin.Text("Body", root, 82f, 32f, 250f, 34f, 25f, QuestRunSkin.Graphite);
        body.text = notice.Body;
        body.fontStyle = FontStyles.Bold;

        if (notice.Kind == QuestManager.NoticeKind.Completed)
        {
            var stamp = QuestRunSkin.Image("Stamp", root, skin.stampCompleted, 318f, 8f, 92f, 60f);
            stamp.rectTransform.localEulerAngles = new Vector3(0f, 0f, 8f);
            var st = stamp.transform;
            st.localScale = Vector3.one * 2f;
            stamp.color = new Color(1f, 1f, 1f, 0f);
            DOTween.Sequence().SetLink(stamp.gameObject).SetUpdate(true)
                .AppendInterval(0.3f)
                .Append(st.DOScale(0.95f, 0.14f).SetEase(Ease.InQuad))
                .Join(stamp.DOFade(1f, 0.08f))
                .Append(st.DOScale(1f, 0.1f).SetEase(Ease.OutBack));
        }
        else if (notice.Quest.tag != QuestTag.None)
        {
            bool story = notice.Quest.tag == QuestTag.Story;
            var badge = QuestRunSkin.Image("Badge", root, story ? skin.badgeStory : skin.badgeRescue, 344f, 8f, 66f, 20f);
            badge.preserveAspect = false;
            var label = skin.Text("Label", badge.transform, 0f, 0f, 66f, 20f, 12f,
                                  Color.white, TextAlignmentOptions.Center);
            label.text = story ? "STORY" : "구출";
            label.fontStyle = FontStyles.Bold;
        }

        _live.Add(root);
        // 오른쪽에서 미끄러져 들어온다
        root.anchoredPosition += new Vector2(60f, 0f);
        group.alpha = 0f;
        DOTween.Sequence().SetLink(root.gameObject).SetUpdate(true).SetTarget(root)
            .Append(root.DOAnchorPosX(origin.x, 0.25f).SetEase(Ease.OutCubic))
            .Join(group.DOFade(1f, 0.18f))
            .AppendInterval(holdSeconds + (notice.Kind == QuestManager.NoticeKind.Completed ? 0.6f : 0f))
            .AppendCallback(() => Dismiss(root, instant: false));
    }

    private void Dismiss(RectTransform root, bool instant)
    {
        if (!_live.Remove(root)) return;
        Relayout();
        DOTween.Kill(root);
        if (instant) { Destroy(root.gameObject); return; }
        var group = root.GetComponent<CanvasGroup>();
        DOTween.Sequence().SetLink(root.gameObject).SetUpdate(true)
            .Append(group.DOFade(0f, 0.25f))
            .Join(root.DOAnchorPosX(origin.x + 40f, 0.25f).SetEase(Ease.InCubic))
            .OnComplete(() => Destroy(root.gameObject));
    }

    private void Relayout()
    {
        for (int i = 0; i < _live.Count; i++)
            _live[i].DOAnchorPosY(-(origin.y + i * (H + spacing)), 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);
    }
}
