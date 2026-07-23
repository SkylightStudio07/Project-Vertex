using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 이벤트 노드 UI 컨트롤러.
// 흐름: Open() → (dialogueJson 있으면 DialogueView 먼저 재생) → description 페이지 진행 → 선택지 표시
//       → 선택 시 효과 실행 + 결과 텍스트 페이지 진행 → 진행 버튼 → 맵 복귀
// description/resultText는 페이지(string[]) 단위 — textAdvanceButton(화살표) 또는 스페이스/엔터로
// 한 페이지씩 진행한다(DialogueView의 advanceButton과 동일한 관례).
public class EventView : MonoBehaviour
{
    public static EventView Instance { get; private set; }

    public bool IsEventOpen => gameObject.activeSelf;

    [Header("배경")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite defaultBackground;

    [Header("일러스트 (dialogueJson 없는 이벤트용 — 없으면 표시 안 함)")]
    [SerializeField] private Image illustrationImage;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("버튼")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button textAdvanceButton; // description/resultText 공용 페이지 진행 버튼

    [Header("참조 - 다이얼로그 뷰 없어도 정상 작동")]
    [SerializeField] private MapUIController mapUIController;
    [SerializeField] private DialogueView dialogueView;

    private EventData _data;
    private EventJsonData _json;
    private readonly List<Button> _choiceButtons = new();

    // 페이지 진행 상태 (description/resultText 공용)
    private TextMeshProUGUI _pageTarget;
    private string[] _pages;
    private int _pageIndex;
    private Action _onPagesComplete;

    private void Awake()
    {
        // 주의: 이 오브젝트는 씬에서 비활성 상태로 시작해야 함.
        // 비활성 오브젝트는 Awake가 호출되지 않다가 최초로 SetActive(true)될 때 호출되는데,
        // 여기서 SetActive(false)를 부르면 그 최초 활성화(Open()의 SetActive(true))를
        // 같은 프레임에서 즉시 취소해버려 첫 호출만 패널이 안 뜨는 버그가 생긴다.
        Instance = this;
        continueButton.onClick.AddListener(OnContinueClicked);
        textAdvanceButton.onClick.AddListener(OnTextAdvanceClicked);
    }

    // 선택지 표시 중(advanceButton 비활성)에는 키보드 입력을 무시 — DialogueView.Update()와 동일한 관례.
    // interactable도 같이 체크해서, 향후 타이프라이터 효과로 버튼을 잠가도 키보드가 우회 못 하게 한다.
    private void Update()
    {
        if (!textAdvanceButton.gameObject.activeSelf || !textAdvanceButton.interactable) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[Key.Space].wasPressedThisFrame || Keyboard.current[Key.Enter].wasPressedThisFrame)
            OnTextAdvanceClicked();
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

        if (illustrationImage != null)
        {
            illustrationImage.sprite = data.illustration;
            illustrationImage.enabled = data.illustration != null;
        }

        titleText.text = _json.title;
        resultText.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        textAdvanceButton.gameObject.SetActive(false);
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
        ShowPages(descriptionText, _json.description, BuildChoices);
    }

    // description/resultText 공용 페이지 진행기. pages를 한 페이지씩 target에 채우다가
    // 다 넘기면(또는 애초에 비어있으면) onComplete를 부른다.
    private void ShowPages(TextMeshProUGUI target, string[] pages, Action onComplete)
    {
        _pageTarget = target;
        _pages = pages;
        _pageIndex = 0;
        _onPagesComplete = onComplete;
        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        if (_pages == null || _pages.Length == 0)
        {
            textAdvanceButton.gameObject.SetActive(false);
            _onPagesComplete?.Invoke();
            return;
        }

        _pageTarget.text = _pages[_pageIndex];
        textAdvanceButton.gameObject.SetActive(true);
    }

    private void OnTextAdvanceClicked()
    {
        _pageIndex++;
        if (_pages == null || _pageIndex >= _pages.Length)
        {
            textAdvanceButton.gameObject.SetActive(false);
            _onPagesComplete?.Invoke();
            return;
        }
        ShowCurrentPage();
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

        bool hasResult = index < _json.choices.Length;
        if (hasResult)
            resultText.gameObject.SetActive(true);

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

        if (hasResult)
            ShowPages(resultText, _json.choices[index].resultText, () => continueButton.gameObject.SetActive(true));
        else
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
