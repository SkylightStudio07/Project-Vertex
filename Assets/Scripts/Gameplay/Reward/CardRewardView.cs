using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 보상 카드 선택 UI 띄우는 스크립트. 카드 보상 버튼 클릭 시 열리는 뷰.
public class CardRewardView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform cardContainer;

    [Header("연출")]
    [SerializeField, Min(0f)] private float flipDuration = 0.25f;
    [SerializeField, Min(0f)] private float flipStagger = 0.12f;
    // 고른 카드가 날아가 꽂힐 곳(상단 DECK 버튼). 비우면 제자리에서 작아지며 사라진다.
    [SerializeField] private RectTransform deckButton;
    [SerializeField, Min(0f)] private float flyDuration = 0.45f;
    [SerializeField] private Texture dissolveNoise;
    [SerializeField, Min(0f)] private float dissolveDuration = 0.45f;

    RewardItemButton button;
    private CanvasGroup _group;
    private Sequence _seq;

    public void Open(List<CardData> cardRewardList, RewardItemButton button)
    {
        this.button = button;
        gameObject.SetActive(true);
        Refresh(cardRewardList);
        PlayFlipIn();
    }
    public void Close()
    {
        _seq?.Kill();
        gameObject.SetActive(false);
    }

    // 연출 중 클릭을 막을 대상은 카드들뿐이다. 패널 전체(CanvasGroup on 이 오브젝트)를 막으면
    // 패널 배경까지 클릭이 통과해서, 뒤에 있는 "Cards" 보상 버튼이 다시 눌려 보상 창이 한 번 더 열린다.
    private CanvasGroup Group
    {
        get
        {
            if (_group == null && !cardContainer.TryGetComponent(out _group)) _group = cardContainer.gameObject.AddComponent<CanvasGroup>();
            return _group;
        }
    }

    private void Refresh(List<CardData> cardRewardList)
    {
        // 기존 카드 뷰 전부 제거.
        foreach (Transform child in cardContainer){
            if (child.TryGetComponent<CardReward>(out var cardReward))
                cardReward.Onclick -= OnCardChosen;
            // Destroy는 프레임 끝에 처리된다. 바로 꺼두지 않으면 아래 연출 시퀀스에 섞여 들어가고,
            // 파괴되는 순간 DOTween 안전 모드가 시퀀스 전체를 죽여 새 카드가 폭 0인 채로 멈춘다.
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // 선택 연출에서 레이아웃을 꺼두므로(아래 OnCardChosen) 다시 열 때 켠다
        if (cardContainer.TryGetComponent<LayoutGroup>(out var layout)) layout.enabled = true;

        foreach (var cardData in cardRewardList)
        {
            var view = Instantiate(cardPrefab, cardContainer);
            view.SetCard(cardData);
            // 카드 클릭 시 연출 후 UI 닫기 + 버튼 삭제
            if(view.gameObject.TryGetComponent<CardReward>(out var reward))
                reward.Onclick += OnCardChosen;
        }
    }

    // 카드가 한 장씩 뒤집히듯(가로 폭 0 → 원래 폭) 나타난다. 위치는 레이아웃 그룹이 잡으므로 스케일·알파만 만진다.
    private void PlayFlipIn()
    {
        _seq?.Kill();
        Group.blocksRaycasts = false; // 다 펼쳐지기 전에 눌리면 안 보이는 카드를 고를 수 있어 막는다
        _seq = DOTween.Sequence().SetLink(gameObject);

        // 파괴 대기 중인 이전 카드(비활성)는 건너뛰고 이번에 새로 만든 카드만 연출한다
        int i = 0;
        foreach (Transform child in cardContainer)
        {
            if (!child.gameObject.activeSelf || !child.TryGetComponent<CardReward>(out _)) continue;
            // 루트가 아니라 안쪽 비주얼(Background Image)을 뒤집는다. 컨테이너 레이아웃이 자식 스케일을 반영하는
            // 설정이라, 루트 폭이 0인 순간 배치가 계산되면 스케일이 돌아와도 재배치되지 않아 카드가 화면 밖에 남는다.
            Transform visual = child.childCount > 0 ? child.GetChild(0) : child;
            Vector3 scale = visual.localScale;
            visual.localScale = new Vector3(0f, scale.y, scale.z);
            _seq.Insert(i * flipStagger, visual.DOScaleX(scale.x, flipDuration).SetEase(Ease.OutBack));
            i++;
        }
        // 시퀀스가 도중에 끊겨도(다시 열기 등) 카드를 못 누르는 상태로 남지 않게 OnKill에서 푼다
        _seq.OnKill(() => Group.blocksRaycasts = true);
    }

    // 고른 카드는 DECK 버튼으로 날아가고, 나머지는 디졸브로 사라진 뒤 창이 닫힌다.
    // 덱 추가는 CardReward가 클릭 즉시 처리했으므로 여기선 연출만 한다.
    private void OnCardChosen(CardReward chosen)
    {
        _seq?.Kill();
        Group.blocksRaycasts = false; // 연타로 같은 보상을 두 번 받는 것 방지
        if (button != null) button.MarkClaimed(); // 날아가는 동안 보상 버튼이 다시 눌려도 반응하지 않게

        // 날아가는 동안 나머지 카드가 재배치되며 밀리지 않도록 레이아웃을 멈춘다
        if (cardContainer.TryGetComponent<LayoutGroup>(out var layout)) layout.enabled = false;

        _seq = DOTween.Sequence().SetLink(gameObject);
        foreach (Transform child in cardContainer)
        {
            if (!child.gameObject.activeSelf || !child.TryGetComponent<CardReward>(out var reward)) continue;
            if (reward == chosen) continue;
            _seq.Join(UIDissolve.Out(child.gameObject, dissolveNoise, dissolveDuration, UIDissolve.DefaultEdge));
        }

        Transform card = chosen.transform;
        card.SetAsLastSibling(); // 사라지는 카드들 위로
        if (deckButton != null)
        {
            _seq.Join(card.DOMove(deckButton.position, flyDuration).SetEase(Ease.InCubic));
            _seq.Join(card.DOScale(card.localScale * 0.15f, flyDuration).SetEase(Ease.InCubic));
            _seq.Join(card.DORotate(new Vector3(0f, 0f, -12f), flyDuration));
            _seq.AppendCallback(() => deckButton.DOPunchScale(Vector3.one * 0.15f, 0.25f, 6, 0.6f));
        }
        else
        {
            _seq.Join(card.DOScale(card.localScale * 0.6f, flyDuration).SetEase(Ease.InBack));
        }

        var rewardButton = button;
        _seq.OnComplete(() =>
        {
            Close();
            if (rewardButton != null) rewardButton.CompleteReward();
        });
    }

    private void OnDisable()
    {
        if (_group != null) _group.blocksRaycasts = true;
    }
}
