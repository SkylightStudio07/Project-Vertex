using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class BlessingUISetup
{
    [MenuItem("Tools/Vertex/Setup Blessing UI (Reference Style)")]
    public static string Setup()
    {
        var blessingGo = GameObject.Find("BlessingView");
        if (blessingGo == null)
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return "Canvas not found!";
            var bvTrans = canvas.transform.Find("BlessingView");
            if (bvTrans != null) blessingGo = bvTrans.gameObject;
        }

        if (blessingGo == null) return "BlessingView GameObject not found!";

        Undo.RegisterFullObjectHierarchyUndo(blessingGo, "Setup Blessing UI (Reference Style)");

        var gr = blessingGo.GetComponent<GraphicRaycaster>();
        if (gr == null) gr = blessingGo.AddComponent<GraphicRaycaster>();

        var blessingView = blessingGo.GetComponent<BlessingView>();
        if (blessingView == null) blessingView = blessingGo.AddComponent<BlessingView>();

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/강원교육튼튼 SDF.asset");

        // Load Icon & Avatar Sprites
        var itemIcons = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Items/ItemIcon.png").OfType<Sprite>().ToList();
        Sprite iconRemove = itemIcons.Find(s => s.name == "ItemIcon_0") ?? itemIcons.FirstOrDefault();
        Sprite iconUpgrade = itemIcons.Find(s => s.name == "ItemIcon_1") ?? itemIcons.FirstOrDefault();
        Sprite iconItem = itemIcons.Find(s => s.name == "ItemIcon_2") ?? itemIcons.FirstOrDefault();

        var machinaSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Blessing/Machina/machina_idle_sprite_sheet.png").OfType<Sprite>().ToList();
        Sprite speakerAvatar = machinaSprites.Find(s => s.name == "machina_idle_sprite_sheet_0") ?? machinaSprites.FirstOrDefault();

        // 1. Create or Load BlessingData SO Asset
        string dirPath = "Assets/Data/Blessing";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
            AssetDatabase.Refresh();
        }

        string assetPath = "Assets/Data/Blessing/Blessing_Machina_Floor0.asset";
        BlessingData blessingData = AssetDatabase.LoadAssetAtPath<BlessingData>(assetPath);
        if (blessingData == null)
        {
            blessingData = ScriptableObject.CreateInstance<BlessingData>();
            blessingData.entityId = "machina";
            blessingData.entityName = "마키나";
            blessingData.entityTitle = "백색 피안화의 사신";
            blessingData.speakerIcon = speakerAvatar;
            blessingData.chapter = 0;
            blessingData.dialogueText = "「눈을 떠라, 방랑자여... 길을 떠나기 전 그대에게 한 가지 은총을 베풀어주마.」";

            blessingData.choices = new List<BlessingChoice>
            {
                new BlessingChoice
                {
                    choiceId = "cleanse",
                    title = "정화",
                    description = "덱에서 불필요한 카드 1장을 완전히 제거합니다.",
                    icon = iconRemove,
                    effectType = BlessingEffectType.RemoveCard,
                    valueCount = 1,
                    titleColor = new Color(1f, 0.82f, 0.4f) // Gold
                },
                new BlessingChoice
                {
                    choiceId = "refine",
                    title = "연마",
                    description = "기본 카드 2장을 선택하여 강화합니다.",
                    icon = iconUpgrade,
                    effectType = BlessingEffectType.UpgradeCards,
                    valueCount = 2,
                    titleColor = new Color(0.35f, 0.8f, 0.95f) // Cyan
                },
                new BlessingChoice
                {
                    choiceId = "supply",
                    title = "보급",
                    description = "모험에 도움이 되는 소모품 아이템 2개를 가방에 획득합니다.",
                    icon = iconItem,
                    effectType = BlessingEffectType.GainRandomItems,
                    valueCount = 2,
                    titleColor = new Color(0.3f, 0.88f, 0.65f) // Emerald
                }
            };

            AssetDatabase.CreateAsset(blessingData, assetPath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            // Update sprites on existing asset
            blessingData.speakerIcon = speakerAvatar;
            if (blessingData.choices.Count >= 3)
            {
                blessingData.choices[0].icon = iconRemove;
                blessingData.choices[1].icon = iconUpgrade;
                blessingData.choices[2].icon = iconItem;
            }
            EditorUtility.SetDirty(blessingData);
            AssetDatabase.SaveAssets();
        }

        // 2. Clear old ChoiceOverlay / Content hierarchy under BlessingView
        Transform overlayTrans = blessingGo.transform.Find("ChoiceOverlay");
        if (overlayTrans != null)
        {
            // Remove old HeaderPanel & ChoicePanel
            Transform oldHeader = overlayTrans.Find("HeaderPanel");
            if (oldHeader != null) Object.DestroyImmediate(oldHeader.gameObject);

            Transform oldChoice = overlayTrans.Find("ChoicePanel");
            if (oldChoice != null) Object.DestroyImmediate(oldChoice.gameObject);
        }
        else
        {
            var overlayGo = new GameObject("ChoiceOverlay", typeof(RectTransform));
            overlayGo.transform.SetParent(blessingGo.transform, false);
            overlayTrans = overlayGo.transform;
        }

        var overlayRt = overlayTrans.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;
        overlayRt.pivot = new Vector2(0.5f, 0.5f);

        // 3. Left-Bottom: Nameplate
        Transform nameplateTrans = overlayTrans.Find("Nameplate");
        GameObject nameplateGo;
        if (nameplateTrans == null)
        {
            nameplateGo = new GameObject("Nameplate", typeof(RectTransform));
            nameplateGo.transform.SetParent(overlayTrans, false);
        }
        else
        {
            nameplateGo = nameplateTrans.gameObject;
        }

        var nameplateRt = nameplateGo.GetComponent<RectTransform>();
        nameplateRt.anchorMin = new Vector2(0f, 0f);
        nameplateRt.anchorMax = new Vector2(0f, 0f);
        nameplateRt.pivot = new Vector2(0f, 0f);
        nameplateRt.anchoredPosition = new Vector2(80f, 60f);
        nameplateRt.sizeDelta = new Vector2(360f, 90f);

        // EntityNameText
        Transform nameTextTrans = nameplateGo.transform.Find("EntityNameText");
        GameObject nameTextGo = nameTextTrans != null ? nameTextTrans.gameObject : new GameObject("EntityNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameTextGo.transform.SetParent(nameplateGo.transform, false);
        var nameTextRt = nameTextGo.GetComponent<RectTransform>();
        nameTextRt.anchorMin = new Vector2(0f, 0.45f);
        nameTextRt.anchorMax = new Vector2(1f, 1f);
        nameTextRt.offsetMin = Vector2.zero;
        nameTextRt.offsetMax = Vector2.zero;
        var nameTmp = nameTextGo.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) nameTmp.font = fontAsset;
        nameTmp.text = blessingData.entityName;
        nameTmp.fontSize = 32f;
        nameTmp.fontStyle = FontStyles.Bold;
        nameTmp.alignment = TextAlignmentOptions.BottomLeft;
        nameTmp.color = Color.white;

        // EntityTitleText
        Transform titleTextTrans = nameplateGo.transform.Find("EntityTitleText");
        GameObject titleTextGo = titleTextTrans != null ? titleTextTrans.gameObject : new GameObject("EntityTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleTextGo.transform.SetParent(nameplateGo.transform, false);
        var titleTextRt = titleTextGo.GetComponent<RectTransform>();
        titleTextRt.anchorMin = new Vector2(0f, 0f);
        titleTextRt.anchorMax = new Vector2(1f, 0.45f);
        titleTextRt.offsetMin = Vector2.zero;
        titleTextRt.offsetMax = Vector2.zero;
        var titleTmp = titleTextGo.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) titleTmp.font = fontAsset;
        titleTmp.text = blessingData.entityTitle;
        titleTmp.fontSize = 17f;
        titleTmp.alignment = TextAlignmentOptions.TopLeft;
        titleTmp.color = new Color(0.55f, 0.65f, 0.76f, 1f);

        // 4. Bottom-Center: DialogueBubble
        Transform bubbleTrans = overlayTrans.Find("DialogueBubble");
        GameObject bubbleGo;
        if (bubbleTrans == null)
        {
            bubbleGo = new GameObject("DialogueBubble", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(HorizontalLayoutGroup));
            bubbleGo.transform.SetParent(overlayTrans, false);
        }
        else
        {
            bubbleGo = bubbleTrans.gameObject;
        }

        var bubbleRt = bubbleGo.GetComponent<RectTransform>();
        bubbleRt.anchorMin = new Vector2(0.5f, 0f);
        bubbleRt.anchorMax = new Vector2(0.5f, 0f);
        bubbleRt.pivot = new Vector2(0.5f, 0f);
        bubbleRt.anchoredPosition = new Vector2(0f, 245f);
        bubbleRt.sizeDelta = new Vector2(820f, 54f);

        var bubbleImg = bubbleGo.GetComponent<Image>();
        bubbleImg.color = new Color(0.16f, 0.18f, 0.26f, 0.92f);

        var bubbleOutline = bubbleGo.GetComponent<Outline>();
        if (bubbleOutline == null) bubbleOutline = bubbleGo.AddComponent<Outline>();
        bubbleOutline.effectColor = new Color(0.35f, 0.42f, 0.58f, 0.5f);
        bubbleOutline.effectDistance = new Vector2(1f, -1f);

        var bubbleHlg = bubbleGo.GetComponent<HorizontalLayoutGroup>();
        bubbleHlg.padding = new RectOffset(12, 18, 6, 6);
        bubbleHlg.spacing = 14f;
        bubbleHlg.childAlignment = TextAnchor.MiddleLeft;
        bubbleHlg.childControlWidth = false;
        bubbleHlg.childControlHeight = false;
        bubbleHlg.childForceExpandWidth = false;
        bubbleHlg.childForceExpandHeight = false;

        // SpeakerAvatar
        Transform avatarTrans = bubbleGo.transform.Find("SpeakerAvatar");
        GameObject avatarGo = avatarTrans != null ? avatarTrans.gameObject : new GameObject("SpeakerAvatar", typeof(RectTransform), typeof(Image), typeof(Outline));
        avatarGo.transform.SetParent(bubbleGo.transform, false);
        var avatarRt = avatarGo.GetComponent<RectTransform>();
        avatarRt.sizeDelta = new Vector2(38f, 38f);
        var avatarImg = avatarGo.GetComponent<Image>();
        avatarImg.sprite = speakerAvatar;
        avatarImg.preserveAspect = true;
        var avatarOutline = avatarGo.GetComponent<Outline>();
        if (avatarOutline == null) avatarOutline = avatarGo.AddComponent<Outline>();
        avatarOutline.effectColor = new Color(0.6f, 0.7f, 0.9f, 0.6f);
        avatarOutline.effectDistance = new Vector2(1f, -1f);

        // DialogueText
        Transform diagTextTrans = bubbleGo.transform.Find("DialogueText");
        GameObject diagTextGo = diagTextTrans != null ? diagTextTrans.gameObject : new GameObject("DialogueText", typeof(RectTransform), typeof(TextMeshProUGUI));
        diagTextGo.transform.SetParent(bubbleGo.transform, false);
        var diagTextRt = diagTextGo.GetComponent<RectTransform>();
        diagTextRt.sizeDelta = new Vector2(740f, 40f);
        var diagTmp = diagTextGo.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) diagTmp.font = fontAsset;
        diagTmp.text = blessingData.dialogueText;
        diagTmp.fontSize = 18f;
        diagTmp.alignment = TextAlignmentOptions.MidlineLeft;
        diagTmp.color = Color.white;

        // 5. Bottom-Center: ChoicePillList
        Transform pillListTrans = overlayTrans.Find("ChoicePillList");
        GameObject pillListGo;
        if (pillListTrans == null)
        {
            pillListGo = new GameObject("ChoicePillList", typeof(RectTransform), typeof(VerticalLayoutGroup));
            pillListGo.transform.SetParent(overlayTrans, false);
        }
        else
        {
            pillListGo = pillListTrans.gameObject;
        }

        var pillListRt = pillListGo.GetComponent<RectTransform>();
        pillListRt.anchorMin = new Vector2(0.5f, 0f);
        pillListRt.anchorMax = new Vector2(0.5f, 0f);
        pillListRt.pivot = new Vector2(0.5f, 0f);
        pillListRt.anchoredPosition = new Vector2(0f, 65f);
        pillListRt.sizeDelta = new Vector2(820f, 168f);

        var vlg = pillListGo.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var choiceRowUIs = new List<BlessingChoiceRowUI>();

        for (int i = 0; i < 3; i++)
        {
            string rowName = $"ChoicePill_{i}";
            Transform rowTrans = pillListGo.transform.Find(rowName);
            GameObject rowGo;
            if (rowTrans == null)
            {
                rowGo = new GameObject(rowName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(LayoutElement), typeof(HorizontalLayoutGroup), typeof(BlessingChoiceRowUI));
                rowGo.transform.SetParent(pillListGo.transform, false);
            }
            else
            {
                rowGo = rowTrans.gameObject;
            }

            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(820f, 48f);

            var le = rowGo.GetComponent<LayoutElement>();
            if (le == null) le = rowGo.AddComponent<LayoutElement>();
            le.preferredHeight = 48f;
            le.minHeight = 48f;

            var rowImg = rowGo.GetComponent<Image>();
            rowImg.color = new Color(0.08f, 0.11f, 0.16f, 0.88f);

            var rowBtn = rowGo.GetComponent<Button>();
            var cb = rowBtn.colors;
            cb.normalColor = new Color(0.08f, 0.11f, 0.16f, 0.88f);
            cb.highlightedColor = new Color(0.16f, 0.22f, 0.32f, 0.98f);
            cb.pressedColor = new Color(0.24f, 0.32f, 0.46f, 1.0f);
            cb.selectedColor = new Color(0.16f, 0.22f, 0.32f, 0.98f);
            rowBtn.colors = cb;

            var rowOutline = rowGo.GetComponent<Outline>();
            if (rowOutline == null) rowOutline = rowGo.AddComponent<Outline>();
            rowOutline.effectColor = new Color(0.28f, 0.38f, 0.52f, 0.4f);
            rowOutline.effectDistance = new Vector2(1f, -1f);

            var rowHlg = rowGo.GetComponent<HorizontalLayoutGroup>();
            rowHlg.padding = new RectOffset(16, 16, 6, 6);
            rowHlg.spacing = 16f;
            rowHlg.childAlignment = TextAnchor.MiddleLeft;
            rowHlg.childControlWidth = false;
            rowHlg.childControlHeight = false;
            rowHlg.childForceExpandWidth = false;
            rowHlg.childForceExpandHeight = false;

            // Icon
            Transform iconTrans = rowGo.transform.Find("Icon");
            GameObject iconGo = iconTrans != null ? iconTrans.gameObject : new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(rowGo.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(32f, 32f);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Title
            Transform titleTrans = rowGo.transform.Find("Title");
            GameObject titleGo = titleTrans != null ? titleTrans.gameObject : new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(rowGo.transform, false);
            var tRt = titleGo.GetComponent<RectTransform>();
            tRt.sizeDelta = new Vector2(80f, 36f);
            var tTmp = titleGo.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) tTmp.font = fontAsset;
            tTmp.fontSize = 20f;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.alignment = TextAlignmentOptions.MidlineLeft;
            tTmp.raycastTarget = false;

            // Desc
            Transform descTrans = rowGo.transform.Find("Desc");
            GameObject descGo = descTrans != null ? descTrans.gameObject : new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            descGo.transform.SetParent(rowGo.transform, false);
            var dRt = descGo.GetComponent<RectTransform>();
            dRt.sizeDelta = new Vector2(640f, 36f);
            var dTmp = descGo.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) dTmp.font = fontAsset;
            dTmp.fontSize = 16f;
            dTmp.alignment = TextAlignmentOptions.MidlineLeft;
            dTmp.color = new Color(0.77f, 0.82f, 0.88f, 1f);
            dTmp.raycastTarget = false;

            var rowUI = rowGo.GetComponent<BlessingChoiceRowUI>();
            if (rowUI == null) rowUI = rowGo.AddComponent<BlessingChoiceRowUI>();

            // Serialized properties on BlessingChoiceRowUI
            var rowSo = new SerializedObject(rowUI);
            rowSo.Update();
            rowSo.FindProperty("button").objectReferenceValue = rowBtn;
            rowSo.FindProperty("iconImage").objectReferenceValue = iconImg;
            rowSo.FindProperty("titleText").objectReferenceValue = tTmp;
            rowSo.FindProperty("descText").objectReferenceValue = dTmp;
            rowSo.ApplyModifiedProperties();

            choiceRowUIs.Add(rowUI);
        }

        // 6. Serialized properties on BlessingView
        var bvSo = new SerializedObject(blessingView);
        bvSo.Update();

        var bgTrans = blessingGo.transform.Find("Background");
        if (bgTrans != null) bvSo.FindProperty("backgroundImage").objectReferenceValue = bgTrans.GetComponent<Image>();

        var charTrans = blessingGo.transform.Find("BlessingCharacter");
        if (charTrans != null) bvSo.FindProperty("characterImage").objectReferenceValue = charTrans.GetComponent<Image>();

        bvSo.FindProperty("petalEffect").objectReferenceValue = blessingGo.GetComponent<PetalFloatingEffect>();
        bvSo.FindProperty("defaultBlessingData").objectReferenceValue = blessingData;
        bvSo.FindProperty("nameplatePanel").objectReferenceValue = nameplateGo;
        bvSo.FindProperty("entityNameText").objectReferenceValue = nameTmp;
        bvSo.FindProperty("entityTitleText").objectReferenceValue = titleTmp;
        bvSo.FindProperty("dialogueBubblePanel").objectReferenceValue = bubbleGo;
        bvSo.FindProperty("speakerAvatarImage").objectReferenceValue = avatarImg;
        bvSo.FindProperty("dialogueText").objectReferenceValue = diagTmp;
        bvSo.FindProperty("choiceContainer").objectReferenceValue = pillListGo.transform;

        var choiceRowsProp = bvSo.FindProperty("choiceRows");
        choiceRowsProp.ClearArray();
        for (int i = 0; i < choiceRowUIs.Count; i++)
        {
            choiceRowsProp.InsertArrayElementAtIndex(i);
            choiceRowsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceRowUIs[i];
        }

        var mapCtrl = GameObject.FindObjectOfType<MapUIController>(true);
        if (mapCtrl != null)
        {
            bvSo.FindProperty("mapUIController").objectReferenceValue = mapCtrl;

            var mapSo = new SerializedObject(mapCtrl);
            mapSo.Update();
            var blessingProp = mapSo.FindProperty("blessingView");
            if (blessingProp != null)
            {
                blessingProp.objectReferenceValue = blessingView;
                mapSo.ApplyModifiedProperties();
            }
        }

        bvSo.ApplyModifiedProperties();

        // 7. Setup BlessingView with the data
        blessingView.Setup(blessingData);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        return $"Blessing UI (Reference Style) Setup Complete! BlessingData '{blessingData.name}' connected with 3 pill rows.";
    }
}
