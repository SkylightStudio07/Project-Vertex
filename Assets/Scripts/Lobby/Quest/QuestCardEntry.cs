using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 의뢰 게시판 카드 한 장. QuestBoardView가 목록 수만큼 찍어 낸다.
// 상태: 기본 / 선택 / 수주됨(도장) / 잠김(게시 조건 미달) / 완료(완료 기록 탭, 완료 도장)
public class QuestCardEntry : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image backing;
    [SerializeField] private GameObject selectedMark;
    [SerializeField] private UICroppedArt art;
    [SerializeField] private Image typeIcon;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private Image badge;
    [SerializeField] private TextMeshProUGUI badgeText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI clientText;
    [SerializeField] private GameObject itemGroup;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI rewardLabel;
    [SerializeField] private TextMeshProUGUI rewardValue;
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private TextMeshProUGUI lockText;
    [SerializeField] private Image stamp;

    [Header("스프라이트")]
    [SerializeField] private Sprite backingNormal, backingHover, backingLocked;
    [SerializeField] private Sprite iconRecovery, iconDelivery, iconElimination, iconRescue;
    [SerializeField] private Sprite badgeStory, badgeRescue;
    [SerializeField] private Sprite stampAccepted, stampCompleted;

    public QuestData Quest { get; private set; }
    public bool Locked { get; private set; }
    public event Action<QuestCardEntry> OnClicked;

    private void Awake()
    {
        if (button != null) button.onClick.AddListener(() => OnClicked?.Invoke(this));
    }

    public void Bind(QuestData quest, bool locked, bool accepted, bool completed)
    {
        Quest = quest;
        Locked = locked;

        backing.sprite = locked ? backingLocked : backingNormal;
        var ss = button.spriteState; ss.highlightedSprite = locked ? null : backingHover; ss.pressedSprite = ss.highlightedSprite; button.spriteState = ss;
        button.transition = locked ? Selectable.Transition.None : Selectable.Transition.SpriteSwap;

        typeIcon.sprite = quest.goal switch
        {
            QuestGoal.DeliverToNode => iconDelivery,
            QuestGoal.DefeatCount => iconElimination,
            _ => quest.tag == QuestTag.Rescue ? iconRescue : iconRecovery,
        };
        typeText.text = quest.TypeName;

        badge.gameObject.SetActive(quest.tag != QuestTag.None);
        if (quest.tag != QuestTag.None)
        {
            bool story = quest.tag == QuestTag.Story;
            badge.sprite = story ? badgeStory : badgeRescue;
            badgeText.text = story ? "STORY" : "구출";
            badgeText.color = story ? Color.white : new Color(0.05f, 0.72f, 0.95f, 1f);
        }

        titleText.text = quest.title;
        clientText.text = quest.client;

        art.gameObject.SetActive(quest.cardArt != null);
        if (quest.cardArt != null) art.SetSprite(quest.cardArt);
        art.GetComponent<RawImage>().color = locked ? new Color(0.55f, 0.56f, 0.58f, 0.8f) : Color.white; // 잠기면 흐리게

        bool hasItem = !locked && quest.questItem != null;
        itemGroup.SetActive(hasItem);
        if (hasItem)
        {
            itemIcon.sprite = quest.questItem.ItemIcon;
            itemName.text = quest.questItem.ItemName;
        }

        rewardValue.text = locked ? "???" : $"{quest.rewardExp} <size=75%>EXP</size>";
        rewardValue.color = locked ? new Color(0.3f, 0.31f, 0.33f, 1f) : new Color(0.05f, 0.72f, 0.95f, 1f);
        summaryText.text = locked ? "" : quest.Summary;
        lockText.transform.parent.gameObject.SetActive(locked);
        lockText.text = string.IsNullOrEmpty(quest.lockedHint) ? "게시 조건 미달" : quest.lockedHint;

        stamp.gameObject.SetActive(accepted || completed);
        stamp.sprite = completed ? stampCompleted : stampAccepted;
        stamp.transform.localScale = Vector3.one;
        stamp.color = Color.white;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedMark != null) selectedMark.SetActive(selected);
    }

    // 도장이 위에서 크게 떨어져 쾅 찍히는 연출 (수주·완료)
    public void PlayStamp(bool completed, float delay = 0f)
    {
        stamp.sprite = completed ? stampCompleted : stampAccepted;
        stamp.gameObject.SetActive(true);
        var t = stamp.transform;
        t.DOKill();
        t.localScale = Vector3.one * 2.2f;
        stamp.color = new Color(1f, 1f, 1f, 0f);
        DOTween.Sequence().SetLink(stamp.gameObject).SetTarget(t)
            .AppendInterval(delay)
            .Append(t.DOScale(0.92f, 0.16f).SetEase(Ease.InQuad))
            .Join(stamp.DOFade(1f, 0.1f))
            .Append(t.DOScale(1f, 0.12f).SetEase(Ease.OutBack))
            .Join(((RectTransform)transform).DOShakeAnchorPos(0.18f, 5f, 18, 90f, false, true));
    }
}
