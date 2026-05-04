// ============================================================
// 파일명    : HandView.cs
// 설명      : BattleManager.Hand를 감시하여 CardView 인스턴스를 생성/갱신.
//             씬에 하나만 존재하는 손패 컨테이너 오브젝트에 부착.
// ============================================================

using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;    // 카드 프리팹 (CardView 부착된 것)
    [SerializeField] private Transform cardContainer; // 카드들이 나열될 부모 Transform

    private void Start()
    {
        BattleManager.Instance.OnHandChanged += Refresh;
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnHandChanged -= Refresh;
    }

    private void Refresh()
    {
        // 기존 카드 뷰 전부 제거
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        // 손패의 각 CardData마다 CardView 생성 후 바인딩
        foreach (var cardData in BattleManager.Instance.Hand)
        {
            var view = Instantiate(cardPrefab, cardContainer);
            view.SetCard(cardData);
        }
    }
}
