using System.Collections.Generic;
using UnityEngine;

// 플레이어 덱 관리 전담 (시작 덱 구성, 런 중 카드 획득).
// 로비 등 GameManager(현재 런 상태: HP/골드/적/맵 진행)와 무관한 씬에서도
// 덱만 따로 참조할 수 있도록 분리했다.
public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("기본 덱")]
    [SerializeField] private CardData strikeCard;
    [SerializeField] private CardData blockCard;
    [SerializeField] private CardData reloadCard;

    [Header("디버그")]
    [SerializeField] private CardData strikeCard_Debug;
    [SerializeField] private CardData blockCard_Debug;

    public List<CardData> PlayerDeck { get; private set; } = new();

    // 새 런 시작 시 GameManager.InitializeRun()에서 호출.
    public void InitializeStartingDeck()
    {
        // SO 원본이 오염되지 않도록 Instantiate로 복사
        PlayerDeck = new List<CardData>();
        for (int i = 0; i < 5; i++)
        {
            PlayerDeck.Add(Instantiate(strikeCard));
            PlayerDeck.Add(Instantiate(blockCard));
        }
        PlayerDeck.Add(Instantiate(reloadCard));

        // 디버그용 시작 덱. 실제 릴리즈 시에는 이 부분 제거할 것.
        PlayerDeck.Add(Instantiate(strikeCard_Debug));
        PlayerDeck.Add(Instantiate(blockCard_Debug));
    }

    // 플레이어 덱에 획득 카드를 추가하는 메소드
    public void AddCardToPlayerDeck(CardData card)
    {
        if (card == null) return;

        PlayerDeck.Add(Instantiate(card));
        Debug.Log("플레이어 덱에 카드 추가완료.");
    }
}
