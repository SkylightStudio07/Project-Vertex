using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 이벤트 노드 UI 컨트롤러.
// 흐름: Open() → (dialogueJson 있으면 DialogueView 먼저 재생) → description + 선택지 표시
//       → 선택 시 효과 실행 + 결과 텍스트 → 진행 버튼 → 맵 복귀
public class EventView : MonoBehaviour
{
    public static EventView Instance { get; private set; }

    public bool IsEventOpen => gameObject.activeSelf;

    [Header("배경")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite defaultBackground;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("버튼")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Button continueButton;

    [Header("참조 - 다이얼로그 뷰 없어도 정상 작동")]
    [SerializeField] private MapUIController mapUIController;
    [SerializeField] private DialogueView dialogueView;

    private EventData _data;
    private EventJsonData _json;
    private readonly List<Button> _choiceButtons = new();

    private void Awake()
    {
        // 주의: 이 오브젝트는 씬에서 비활성 상태로 시작해야 함.
        // 비활성 오브젝트는 Awake가 호출되지 않다가 최초로 SetActive(true)될 때 호출되는데,
        // 여기서 SetActive(false)를 부르면 그 최초 활성화(Open()의 SetActive(true))를
        // 같은 프레임에서 즉시 취소해버려 첫 호출만 패널이 안 뜨는 버그가 생긴다.
        Instance = this;
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void Open(EventData data)
    {
        if (data == null || data.eventJson == null)
        {
            Debug.LogWarning("[Event] EventData 또는 eventJson이 null.");
            return;
        }

        _data = data;
        _json = JsonUtility.FromJson<EventJsonData>(data.eventJson.text);

        if (_json == null || _json.choices == null || _json.choices.Length == 0)
        {
            Debug.LogWarning($"[Event] '{data.name}'의 JSON 파싱 실패.");
            return;
        }

        backgroundImage.sprite = data.backgroundImage != null ? data.backgroundImage : defaultBackground;
        titleText.text = _json.title;
        resultText.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        HideChoices();

        gameObject.SetActive(true);

        if (_data.dialogueJson != null)
        {
            if (dialogueView == null)
            {
                Debug.LogWarning("[Event] dialogueJson이 설정됐지만 dialogueView 참조가 없음. 다이얼로그를 건너뜀. 없어도 문제없음");
                ShowDescriptionAndChoices();
            }
            else
            {
                dialogueView.Play(_data.dialogueJson, ShowDescriptionAndChoices);
            }
        }
        else
        {
            ShowDescriptionAndChoices();
        }
    }

    private void ShowDescriptionAndChoices()
    {
        descriptionText.gameObject.SetActive(true);
        descriptionText.text = _json.description;
        BuildChoices();
    }

    private void HideChoices()
    {
        foreach (var b in _choiceButtons)
            Destroy(b.gameObject);
        _choiceButtons.Clear();
    }

    private void BuildChoices()
    {
        HideChoices();

        for (int i = 0; i < _json.choices.Length; i++)
        {
            int index = i;
            var btn = Instantiate(choiceButtonPrefab, choiceContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = _json.choices[i].choiceText;
            btn.onClick.AddListener(() => OnChoiceSelected(index));
            _choiceButtons.Add(btn);
        }
    }

    private void OnChoiceSelected(int index)
    {
        foreach (var b in _choiceButtons)
            b.gameObject.SetActive(false);

        descriptionText.gameObject.SetActive(false);

        if (index < _json.choices.Length)
        {
            resultText.text = _json.choices[index].resultText;
            resultText.gameObject.SetActive(true);
        }

        // choiceEffects가 Inspector에서 비어있거나 선택지 수와 안 맞을 수 있으니.... 방어적으로 체크
        if (_data.choiceEffects != null && index < _data.choiceEffects.Count)
        {
            var effects = _data.choiceEffects[index].effects;
            if (effects != null)
            {
                var ctx = new CardContext();
                foreach (var effect in effects)
                    if (effect != null) effect.Execute(ctx);
            }
        }

        continueButton.gameObject.SetActive(true);
    }

    private void OnContinueClicked()
    {
        gameObject.SetActive(false);

        if (mapUIController == null)
        {
            Debug.LogWarning("[Event] mapUIController 참조가 없음. 인스펙터 체크 필요.");
            return;
        }
        mapUIController.OpenMap();
    }
}
