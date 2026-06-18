using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 화면 슬롯 하나(최대 4개 중 1개)에 캐릭터 초상화·이름을 표시.
// 말하는 중이 아니면 톤 다운(어둡게) 처리.
public class CharacterSlotView : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI nameText;

    private static readonly Color ColorActive = Color.white;
    private static readonly Color ColorDimmed = new(0.5f, 0.5f, 0.5f, 1f);

    public string CharacterId { get; private set; }

    public void Bind(DialogueCharacterData character)
    {
        if (character == null)
        {
            Debug.LogWarning("[Dialogue] CharacterSlotView.Bind에 null 캐릭터 데이터 전달됨.");
            return;
        }

        CharacterId = character.id;
        nameText.text = character.name;
        gameObject.SetActive(true);
        SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        portraitImage.color = highlighted ? ColorActive : ColorDimmed;
    }

    // 캐릭터별 emotion → Sprite 매핑 테이블이 아직 없어 비워둠.
    // 캐릭터 SO에 감정별 스프라이트 목록이 추가되면 여기서 portraitImage.sprite를 갱신.
    public void SetEmotion(string emotion)
    {
    }
}
