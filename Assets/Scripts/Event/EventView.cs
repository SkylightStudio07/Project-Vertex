using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventView : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("버튼")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Button continueButton;

    [Header("참조")]
    [SerializeField] private MapUIController mapUIController;

    private EventData _data;
    private EventJsonData _json;
    private readonly List<Button> _choiceButtons = new();

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        gameObject.SetActive(false);
    }

    public void Open(EventData data)
    {
        _data = data;
        _json = JsonUtility.FromJson<EventJsonData>(data.eventJson.text);

        titleText.text       = _json.title;
        descriptionText.text = _json.description;
        resultText.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        BuildChoices();
        gameObject.SetActive(true);
    }

    private void BuildChoices()
    {
        foreach (var b in _choiceButtons)
            Destroy(b.gameObject);
        _choiceButtons.Clear();

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

        if (index < _json.choices.Length)
        {
            resultText.text = _json.choices[index].resultText;
            resultText.gameObject.SetActive(true);
        }

        if (index < _data.choiceEffects.Count)
        {
            var ctx = new CardContext();
            foreach (var effect in _data.choiceEffects[index].effects)
                effect?.Execute(ctx);
        }

        continueButton.gameObject.SetActive(true);
    }

    private void OnContinueClicked()
    {
        gameObject.SetActive(false);
        mapUIController.OpenMap();
    }
}
