using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// filename   : BlessingChoiceRowUI.cs
// description: 축복 화면의 개별 선택지 가로 알약형 버튼 UI 컴포넌트.
//              [아이콘] [타이틀(강조색)] [설명문] 구조를 제어합니다.
// ============================================================

public class BlessingChoiceRowUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;

    public Button Button => button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    public void Setup(BlessingChoice choice, Action onClick)
    {
        if (button == null) button = GetComponent<Button>();

        if (iconImage != null)
        {
            if (choice.icon != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = choice.icon;
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        if (titleText != null)
        {
            titleText.text = choice.title;
            titleText.color = choice.titleColor;
        }

        if (descText != null)
        {
            descText.text = choice.description;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
        }
    }
}
