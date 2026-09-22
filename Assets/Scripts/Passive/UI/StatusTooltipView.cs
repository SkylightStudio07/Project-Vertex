using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상태(버프/디버프) 설명 툴팁. 화면 전체에 하나만 존재하는 싱글톤 패널로,
// 마우스를 어느 상태 칩(플레이어/적 공용 StatusChipView)에 올리든 이 패널 하나를 채워서 보여준다.
//
// 마우스 커서를 따라다니지 않고 씬에 미리 배치해둔 고정 위치(아군 스프라이트 앞)에 뜬다 —
// 좁은 칩 옆에 바로 띄우면 화면 밖으로 나가거나 다른 UI에 가릴 수 있어서, 항상 같은
// 자리에서 읽을 수 있게 고정했다. 위치를 바꾸고 싶으면 이 오브젝트 자체를 씬에서 옮기면 된다.
public class StatusTooltipView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public static StatusTooltipView Instance { get; private set; }

    private void Awake()
    {
        // 씬에 하나만 있다고 가정한다(여러 개면 마지막에 Awake된 것이 우선).
        Instance = this;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(StatusDefinition definition)
    {
        if (definition == null) return;

        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite  = definition.Icon;
            iconImage.enabled = definition.Icon != null;
        }

        if (titleText != null) titleText.text = definition.DisplayName;
        if (descriptionText != null) descriptionText.text = definition.Description;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
