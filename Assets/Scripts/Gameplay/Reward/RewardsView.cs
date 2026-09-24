using Coffee.UIEffects;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 전투 승리 시 BattleManager에서 보상 데이터 받아와서 버튼 띄우는 뷰.
public class RewardsView : MonoBehaviour
{
    [SerializeField] private RewardItemButton rewardButtonPrefab;
    [SerializeField] private Transform rewardButtonContainer;
    [SerializeField] private CardRewardView cardRewardView;
    [SerializeField] private MapUIController mapUIController;

    [Header("등장 연출")]
    // 막타 카드를 쓰자마자 결과창이 덮이면 적이 쓰러지는 연출을 못 보고 끝나버린다.
    // 승리 판정(BattleManager)은 즉시 끝내고, 화면에 띄우는 것만 늦춘다.
    [SerializeField, Min(0f)] private float openDelay = 0.75f;
    [SerializeField] private Image background;          // RewardBackground — 원래 알파까지 페이드
    [SerializeField] private RectTransform rewardsArea; // 본문 패널 — 페이드 + 살짝 커지며 등장
    [SerializeField, Min(0f)] private float fadeDuration = 0.35f;
    [SerializeField, Min(0f)] private float buttonStagger = 0.08f;
    // RewardsArea 패널 이미지에 붙은 UIEffect(Dissolve). 비워두면 패널 전체 페이드로 대체된다.
    // 디졸브 모양(노이즈 텍스처·가장자리 색·폭)은 UIEffect 컴포넌트 인스펙터에서 조정.
    [SerializeField] private UIEffect panelDissolve;
    [SerializeField, Min(0f)] private float dissolveDuration = 0.6f;

    [Header("획득 연출")]
    // 아이템 아이콘이 날아가 꽂힐 상단 아이템바. 비우면 아이템도 골드처럼 제자리에서 떠오르며 사라진다.
    [SerializeField] private ItemInventoryView itemBar;
    [SerializeField, Min(0f)] private float collectDuration = 0.5f;

    private BattleReward reward;
    private List<RewardItemButton> buttons;
    private Sequence _openSequence;
    // 페이드·스케일 목표값. 씬에 배치된 값(RewardsArea는 0.67배 등)을 첫 등장 때 기억해 두고 거기로 돌아간다.
    private float _backgroundAlpha = -1f;
    private Vector3 _areaScale;

    private void Start()
    {
        BattleManager.Instance.OnBattleVictory += Open;
    }

    private void Open(BattleReward reward)
    {
        this.reward = reward;

        // DOTween 타이머는 오브젝트 활성 여부와 무관하게 돌아가서, 루트가 꺼져 있어도 지연이 정상 동작한다.
        _openSequence?.Kill();
        _openSequence = DOTween.Sequence()
            .AppendInterval(openDelay)
            .AppendCallback(Show)
            .SetLink(gameObject);
    }

    private void Show()
    {
        // OnProceedClicked()가 루트 자체를 꺼두기 때문에, 다음 승리에서도 보이려면
        // 자식들뿐 아니라 루트도 다시 켜야 한다. (안 켜면 두 번째 승리부터 화면에 안 뜨는 버그)
        gameObject.SetActive(true);

        buttons = new List<RewardItemButton>();
        Refresh();
        foreach (Transform child in transform)
            child.gameObject.SetActive(true);

        PlayOpenAnimation();
    }

    private void PlayOpenAnimation()
    {
        var seq = DOTween.Sequence().SetLink(gameObject);
        _openSequence = seq;

        if (background != null)
        {
            if (_backgroundAlpha < 0f) _backgroundAlpha = background.color.a;
            background.DOKill();
            var c = background.color;
            background.color = new Color(c.r, c.g, c.b, 0f);
            seq.Join(background.DOFade(_backgroundAlpha, fadeDuration));
        }

        // 패널 디졸브가 끝나갈 즈음 내용물(제목·버튼)이 올라오도록 시작 시점을 뒤로 민다.
        float contentStart = 0.2f;

        if (rewardsArea != null)
        {
            // 연출 중에 버튼이 눌리면 보상을 두 번 받는 등 꼬일 수 있어 끝날 때까지 입력을 막는다.
            var group = GetOrAddCanvasGroup(rewardsArea.gameObject);
            group.interactable = false;
            if (_areaScale == Vector3.zero) _areaScale = rewardsArea.localScale;
            rewardsArea.DOKill();
            rewardsArea.localScale = _areaScale * 0.94f;
            seq.Insert(0.1f, rewardsArea.DOScale(_areaScale, fadeDuration + 0.1f).SetEase(Ease.OutCubic));

            if (panelDissolve != null)
            {
                // UIEffect는 자기 Graphic(패널 이미지)에만 먹고 자식에는 번지지 않는다.
                // 그래서 패널은 디졸브로, 자식 내용물은 CanvasGroup 페이드로 따로 올린다.
                group.alpha = 1f;
                panelDissolve.transitionRate = 1f;
                seq.Insert(0.05f, DOTween.To(() => panelDissolve.transitionRate,
                                             r => panelDissolve.transitionRate = r,
                                             0f, dissolveDuration).SetEase(Ease.OutQuad));
                contentStart = 0.05f + dissolveDuration * 0.6f;

                foreach (Transform child in rewardsArea)
                {
                    // 보상 버튼은 아래에서 하나씩 따로 올리고, 꺼져 있는 카드 선택 패널은 건드리지 않는다.
                    if (child == rewardButtonContainer || !child.gameObject.activeSelf) continue;
                    var childGroup = GetOrAddCanvasGroup(child.gameObject);
                    childGroup.alpha = 0f;
                    seq.Insert(contentStart, childGroup.DOFade(1f, fadeDuration));
                }
            }
            else
            {
                group.alpha = 0f;
                seq.Insert(0.1f, group.DOFade(1f, fadeDuration));
            }

            seq.OnComplete(() => group.interactable = true);
            seq.OnKill(() =>
            {
                group.interactable = true;
                if (panelDissolve != null) panelDissolve.transitionRate = 0f;
            });
        }

        // 버튼 위치는 VerticalLayoutGroup이 잡으므로 위치는 건드리지 않고 알파·크기만 순차로 올린다.
        float start = contentStart;
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            var group = GetOrAddCanvasGroup(button.gameObject);
            group.alpha = 0f;
            // 버튼은 매번 새로 생성되므로 지금 스케일이 곧 프리팹 원래 스케일이다.
            Vector3 scale = button.transform.localScale;
            button.transform.localScale = scale * 0.9f;

            float at = start + i * buttonStagger;
            seq.Insert(at, group.DOFade(1f, fadeDuration));
            seq.Insert(at, button.transform.DOScale(scale, fadeDuration).SetEase(Ease.OutBack));
        }
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
        => go.TryGetComponent<CanvasGroup>(out var group) ? group : go.AddComponent<CanvasGroup>();

    // 진행 버튼 onClick에 연결
    public void OnProceedClicked()
    {
        _openSequence?.Kill();
        gameObject.SetActive(false);
        mapUIController.OpenMap();
    }

    private void DestroyButton(RewardItemButton destroyedButton)
    {
        if (buttons == null) return;

        if (buttons.Contains(destroyedButton))
        {
            destroyedButton.OnDestroyed -= DestroyButton;
            buttons.Remove(destroyedButton);
            PlayCollectEffect(destroyedButton);
            Destroy(destroyedButton.gameObject);
        }
        // 보상 다 소진해도 자동 닫힘 없음 — 진행 버튼으로 처리
    }

    // 버튼은 바로 사라지고, 아이콘(과 골드는 "+N Gold" 글자) 복제본이 대신 연출을 맡는다.
    // 카드 보상은 CardRewardView가 카드를 덱으로 날리는 연출을 이미 했으므로 여기선 생략.
    private void PlayCollectEffect(RewardItemButton button)
    {
        var item = button.Item;
        if (item.Type == RewardType.Card || button.Icon == null || !button.Icon.enabled) return;

        var icon = CloneForEffect(button.Icon.gameObject);
        GameObject label = null;
        var seq = DOTween.Sequence().SetLink(icon);
        Transform slot = item.Type == RewardType.Item && itemBar != null && ItemInventoryManager.Instance != null
            ? itemBar.GetSlotRect(ItemInventoryManager.Instance.Items.Count - 1)
            : null;

        if (slot != null)
        {
            // 아이템: 아이템바의 새로 채워진 슬롯으로 날아가 꽂힌다.
            // 인벤토리는 획득 즉시 갱신되므로, 아이콘이 도착할 때까지 슬롯을 숨겨 둔다.
            if (!slot.TryGetComponent<CanvasGroup>(out var slotGroup)) slotGroup = slot.gameObject.AddComponent<CanvasGroup>();
            slotGroup.alpha = 0f;
            seq.Append(icon.transform.DOMove(slot.position, collectDuration).SetEase(Ease.InOutCubic));
            seq.Join(icon.transform.DOScale(icon.transform.localScale * 0.6f, collectDuration).SetEase(Ease.InCubic));
            seq.AppendCallback(() =>
            {
                slotGroup.alpha = 1f;
                slot.DOKill(true);
                slot.DOPunchScale(Vector3.one * 0.2f, 0.3f, 6, 0.6f);
            });
        }
        else
        {
            // 골드(또는 아이템바 미연결): 제자리에서 위로 떠오르며 사라진다
            Vector3 rise = icon.transform.up * 80f * icon.transform.lossyScale.y;
            seq.Append(icon.transform.DOMove(icon.transform.position + rise, collectDuration).SetEase(Ease.OutCubic));
            seq.Join(icon.GetComponent<Graphic>().DOFade(0f, collectDuration).SetEase(Ease.InQuad));

            if (item.Type == RewardType.Gold && button.Label != null)
            {
                label = CloneForEffect(button.Label.gameObject);
                var text = label.GetComponent<TMPro.TextMeshProUGUI>();
                text.text = $"+{item.Data} Gold";
                seq.Join(label.transform.DOMove(label.transform.position + rise, collectDuration).SetEase(Ease.OutCubic));
                seq.Join(text.DOFade(0f, collectDuration).SetEase(Ease.InQuad));
            }
        }
        // OnKill은 하나만 등록되므로(나중 것이 덮어씀) 정리는 한 곳에서 한다
        var slotGroupToRestore = slot != null ? slot.GetComponent<CanvasGroup>() : null;
        seq.OnKill(() =>
        {
            Destroy(icon);
            if (label != null) Destroy(label);
            if (slotGroupToRestore != null) slotGroupToRestore.alpha = 1f; // 중간에 끊겨도 슬롯이 숨은 채 남지 않게
        });
    }

    // 버튼이 곧 파괴되므로 연출용 복제본을 RewardUI 바로 아래(월드 위치 유지)에 만든다.
    private GameObject CloneForEffect(GameObject source)
    {
        var clone = Instantiate(source, transform, true);
        clone.transform.SetAsLastSibling();
        foreach (var g in clone.GetComponentsInChildren<Graphic>()) g.raycastTarget = false;
        return clone;
    }

    private void Refresh()
    {
        foreach (Transform child in rewardButtonContainer){
            if(child.TryGetComponent<RewardItemButton>(out var button))
            {
                button.OnCardReward -= cardRewardView.Open;
            }
            Destroy(child.gameObject);
        }

        if (reward == null) return;

        // 버튼마다 RewardItem 구조체를 바인딩
        foreach (RewardItem item in reward.GetRewardList())
        {
            var button = Instantiate(rewardButtonPrefab, rewardButtonContainer);
            buttons.Add(button);
            button.Bind(item);
            button.OnCardReward += cardRewardView.Open;
            button.OnDestroyed += DestroyButton;
        }
    }

    private void OnDestroy()
    {
        _openSequence?.Kill();
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnBattleVictory -= Open;
    }
}
