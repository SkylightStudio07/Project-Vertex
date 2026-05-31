using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private Image charImage;
    [SerializeField] private TextMeshProUGUI charNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    public void SetDialogue(Image charImage, string charNameText, string dialogueText)
    {
        this.charImage = charImage;
        this.charNameText.text = charNameText;
        this.dialogueText.text = dialogueText;
    }
}
