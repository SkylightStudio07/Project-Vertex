using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    void Awake()
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

    public List<EnemyData> currentEnemies; // 현재 전투에 참여하는 적들의 데이터 리스트

    [Header("덱 관련")]
    public List<CardData> playerDeck; // 플레이어가 얻은 전체 카드덱
    public List<CardData> DrawableDeck; // 드로우할 수 있는 카드 더미
    public List<CardData> DiscardedDeck; // 버린 카드 더미

    [Header("막, 층")]
    [SerializeField] private int chapter = 1; // 현재 막
    [SerializeField] private int floor = 1; // 현재 층

    [Header("기본 덱")]
    [SerializeField] private CardData strikeCard; // 기본 공격 카드
    [SerializeField] private CardData blockCard; // 기본 방어 카드

    [Header("현재 턴")]
    [SerializeField] private Turn currentTurn = Turn.PlayerTurn;






    void Start()
    {
        Instance = this;
        InitializeRun();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializeRun() // 런 첫 개시 프로세스. 런 시작시에만 딱 한 번 실행됨.
    {
        chapter++; // 1막 정의
        // 런 시작.
        // 기본 카드 초기화
        playerDeck = new List<CardData>();

        for (int i = 0; i < 5; i++) // 기본 카드 타타타타타/수수수수수
        {
            // SO 카드 인스턴스화하여 덱에 추가. SO는 런타임에서 수정되면 원 SO도 바뀌니까 건들 ㄴㄴ
            playerDeck.Add(Instantiate(strikeCard)); 
            playerDeck.Add(Instantiate(blockCard));
        }
        InitializeBattle();
    }

    void InitializeBattle()
    {
        // 메모리에 악영향이겠지만, 매 전투마다 플레이어 덱을 깊은 복사할 수밖에 없다. 고민은 좀 해 봤는데 다른 방법이랑 별달리 퍼포먼스 차이가 있을 것 같진 않으니까.
    
        foreach(var card in playerDeck)
        {
            DrawableDeck.Add(Instantiate(card)); // 플레이어 덱의 카드들을 드로우 가능한 카드 더미에 인스턴스화하여 추가
        }

        ShuffleDeck(DrawableDeck); // 드로우 가능한 카드 더미 섞기
    }

    void DeckDraw() // 카드 드로우.
    {
        if(currentTurn == Turn.PlayerTurn)
        {
            // 드로우.
        }
    }

    // 덱  섞기. Fisher-Yates 알고리즘 사용.
    // 처음 시작, 그리고 뽑을 카드 없을 때 버린 카드 더미를 드로우 가능한 카드 더미로 옮긴 후 섞을 때 사용.
    void ShuffleDeck(List<CardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            CardData temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    enum Turn
    {
        PlayerTurn,
        EnemyTurn
    }

    void UseCard(CardData card_to_use) // 이러면 이벤트로 호출하나?
    {
        // 카드 드로우 로직
        if(currentTurn == Turn.PlayerTurn)
        {
        }
        else
        {
            return;
        }
    }

}
