using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SanctuaryDetailSetup
{
    [MenuItem("Tools/Vertex/Setup Sanctuary Detail UI")]
    public static string Apply()
    {
        SelectCoopCharUI ui = Object.FindAnyObjectByType<SelectCoopCharUI>(FindObjectsInactive.Include);
        if (ui == null) return "SelectCoopCharUI not found";
        if (ui.transform.Find("DetailView") != null) return "DetailView already exists";

        Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Setup Sanctuary Detail UI");
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/강원교육튼튼 SDF.asset");
        Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Sanctuary/Sanctuary_Background.png");

        var detail = NewImage("DetailView", ui.transform, Color.white);
        Stretch(detail.rectTransform);
        detail.sprite = background;
        detail.raycastTarget = true;
        detail.gameObject.AddComponent<RectMask2D>();
        detail.transform.SetAsLastSibling();

        var art = NewImage("FullArt", detail.transform, Color.white);
        Fixed(art.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero,
            new Vector2(1000f, 1080f));
        art.enabled = false;
        art.raycastTarget = false;

        var placeholder = NewText("FullArtPlaceholder", detail.transform, font,
            "전신 아트 미지정", 32, new Color(.35f, .43f, .47f, 1f), TextAlignmentOptions.Center);
        Fixed(placeholder.rectTransform, new Vector2(.73f, .5f), new Vector2(.5f, .5f), Vector2.zero,
            new Vector2(700f, 100f));

        var info = NewImage("InfoPanel", detail.transform, new Color(.055f, .075f, .095f, .94f));
        Fixed(info.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(70f, -100f), new Vector2(770f, 780f));
        info.raycastTarget = false;

        var accent = NewImage("Accent", info.transform, new Color(.27f, .78f, .88f, 1f));
        Fixed(accent.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 0f), new Vector2(7f, 780f));
        accent.raycastTarget = false;

        var eyebrow = NewText("Eyebrow", info.transform, font,
            "SANCTUARY  /  COMPANION FILE", 23, new Color(.36f, .81f, .9f, 1f), TextAlignmentOptions.Left);
        Fixed(eyebrow.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(45f, -38f), new Vector2(680f, 40f));

        var name = NewText("CharacterName", info.transform, font, "캐릭터", 66, Color.white, TextAlignmentOptions.Left);
        Fixed(name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(45f, -104f), new Vector2(680f, 95f));

        var description = NewText("CharacterDescription", info.transform, font, "", 32,
            new Color(.92f, .96f, .97f, 1f), TextAlignmentOptions.TopLeft);
        Fixed(description.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(45f, -230f), new Vector2(680f, 240f));
        description.enableWordWrapping = true;

        var divider = NewImage("Divider", info.transform, new Color(.38f, .52f, .57f, .85f));
        Fixed(divider.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(45f, -485f), new Vector2(680f, 2f));
        divider.raycastTarget = false;

        var cardLabel = NewText("CardLabel", info.transform, font, "합류 카드", 30,
            new Color(.36f, .81f, .9f, 1f), TextAlignmentOptions.Left);
        Fixed(cardLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(45f, -510f), new Vector2(680f, 45f));

        var card = NewImage("JoinCardPanel", info.transform, new Color(.14f, .18f, .21f, .98f));
        Fixed(card.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(45f, -565f), new Vector2(680f, 185f));
        card.raycastTarget = false;

        var cardArt = NewImage("CardArt", card.transform, Color.white);
        Fixed(cardArt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(12f, -12f), new Vector2(160f, 160f));
        cardArt.preserveAspect = true;
        cardArt.enabled = false;
        cardArt.raycastTarget = false;

        var cardName = NewText("CardName", card.transform, font, "", 32, Color.white, TextAlignmentOptions.Left);
        Fixed(cardName.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(192f, -13f), new Vector2(470f, 45f));

        var cardCost = NewText("CardCost", card.transform, font, "", 23,
            new Color(.36f, .81f, .9f, 1f), TextAlignmentOptions.Left);
        Fixed(cardCost.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(192f, -60f), new Vector2(470f, 35f));

        var cardDescription = NewText("CardDescription", card.transform, font, "", 24,
            new Color(.9f, .94f, .95f, 1f), TextAlignmentOptions.TopLeft);
        Fixed(cardDescription.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(192f, -98f), new Vector2(470f, 78f));
        cardDescription.enableWordWrapping = true;

        Button back = NewButton("BackButton", detail.transform, font, "←  뒤로가기",
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(70f, 48f),
            new Color(.9f, .94f, .95f, 1f), new Color(.06f, .09f, .11f, 1f));
        Button proceed = NewButton("ProceedButton", detail.transform, font, "진행  →",
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-70f, 48f),
            new Color(.08f, .18f, .22f, 1f), Color.white);

        var serialized = new SerializedObject(ui);
        Set(serialized, "detailPanel", detail.rectTransform);
        Set(serialized, "detailFullArt", art);
        Set(serialized, "detailArtPlaceholder", placeholder);
        Set(serialized, "detailName", name);
        Set(serialized, "detailDescription", description);
        Set(serialized, "joinCardPanel", card.rectTransform);
        Set(serialized, "joinCardArt", cardArt);
        Set(serialized, "joinCardName", cardName);
        Set(serialized, "joinCardDescription", cardDescription);
        Set(serialized, "joinCardCost", cardCost);
        Set(serialized, "backButton", back);
        Set(serialized, "proceedButton", proceed);
        serialized.ApplyModifiedProperties();

        detail.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
        return "Sanctuary detail UI created and wired";
    }

    private static Image NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, TMP_FontAsset font,
        string value, float size, Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }

    private static Button NewButton(string name, Transform parent, TMP_FontAsset font, string label,
        Vector2 anchor, Vector2 pivot, Vector2 offset, Color fill, Color textColor)
    {
        var image = NewImage(name, parent, fill);
        Fixed(image.rectTransform, anchor, pivot, offset, new Vector2(270f, 84f));
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = fill;
        colors.highlightedColor = new Color(.48f, .84f, .91f, 1f);
        colors.pressedColor = new Color(.25f, .68f, .78f, 1f);
        colors.selectedColor = fill;
        button.colors = colors;
        var text = NewText("Label", button.transform, font, label, 31, textColor, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        return button;
    }

    private static void Fixed(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 offset, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = offset;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Set(SerializedObject serialized, string property, Object value)
    {
        serialized.FindProperty(property).objectReferenceValue = value;
    }
}
