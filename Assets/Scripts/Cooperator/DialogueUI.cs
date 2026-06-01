using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private Image charImage;
    [SerializeField] private TextMeshProUGUI charNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    public void SetDialogue(Sprite charImage, string charNameText, string dialogueText)
    {
        this.charImage.sprite = charImage;
        this.charNameText.text = charNameText;
        this.dialogueText.text = dialogueText;
    }
}
