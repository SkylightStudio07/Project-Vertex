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

        var machinaSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Blessing/Machina/machina_idle_sprite_64_sheet.png").OfType<Sprite>().ToList();
        if (machinaSprites.Count == 0)
        {
            machinaSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Blessing/Machina/machina_idle_sprite_sheet.png").OfType<Sprite>().ToList();
        }
        Sprite speakerAvatar = machinaSprites.Find(s => s.name == "sheet_64_af572cf5dedeca31_0" || s.name == "machina_idle_sprite_sheet_0") ?? machinaSprites.FirstOrDefault();

        // Load Petal Sprite for Talk Choice Icon
        var petalSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Blessing/Machina/Effects/Petals.png").OfType<Sprite>().ToList();
        Sprite petalIcon = petalSprites.FirstOrDefault() ?? iconItem;

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
            AssetDatabase.CreateAsset(blessingData, assetPath);
        }

        blessingData.entityId = "machina";
        blessingData.entityName = "마키나";
        blessingData.entityTitle = "백색 피안화의 사신";
        blessingData.speakerIcon = speakerAvatar;
        blessingData.chapter = 0;
        blessingData.dialogueText = "「눈을 떠라, 방랑자여... 길을 떠나기 전 그대에게 한 가지 은총을 베풀어주마.」";

        // 조우 핑퐁 대화 풀 구성 (Tier 0 ~ Tier 4 및 치하라 쇼 반응)
        blessingData.encounterSequences = new List<BlessingDialogueSequence>
        {
            // 치하라 쇼(Cp_01) 동료 합류 특수 반응
            new BlessingDialogueSequence
            {
                sequenceId = "enc_sho_reaction",
                priority = 100,
                minAffinity = 0f,
                maxAffinity = 999f,
                requiredCompanionCharId = "Cp_01",
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「그대의 곁에 선 자... 칼날에 너무나 짙은 피와 후회의 냄새가 배어있구나.」",
                        playerAnswerText = "[치하라 쇼 / 침묵] ...내 피는 내가 알아서 다룬다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「후후... 날을 너무 바짝 세우지 마렴, 아이야. 피안화 앞에서는 그 어떤 원망도 결국 고요해지는 법이란다.」",
                        playerAnswerText = "[방랑자] 쇼를 진정시키며 고개를 끄덕인다."
                    }
                }
            },
            // Tier 0: 낯선 발걸음 (0.0 ~ 1.9)
            new BlessingDialogueSequence
            {
                sequenceId = "enc_tier0",
                priority = 10,
                minAffinity = 0f,
                maxAffinity = 1.9f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「눈을 떠라, 방랑자여... 이 꽃밭은 생과 사의 경계. 아직 그대의 발걸음이 완전히 멎지는 않았구나.」",
                        playerAnswerText = "[방랑자] 마키나의 백색 옷자락을 가만히 응시한다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「버텍스의 심층으로 향하고자 한다면, 내 작은 가호를 그대의 영혼에 새겨두어라.」",
                        playerAnswerText = "[방랑자] 고개를 끄덕이며 손을 뻗는다."
                    }
                }
            },
            // Tier 1: 반복되는 호흡 (2.0 ~ 3.9)
            new BlessingDialogueSequence
            {
                sequenceId = "enc_tier1",
                priority = 10,
                minAffinity = 2.0f,
                maxAffinity = 3.9f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「또다시 흩어졌다 모였구나. 붉게 물든 발자국이 지쳐 보이지만... 눈빛만은 또렷하군.」",
                        playerAnswerText = "[방랑자] 덤덤하게 숨을 고른다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「하지만 조심하렴. 버텍스는 잊으려 하는 자의 기억부터 차례로 삼켜버린단다.」",
                        playerAnswerText = "[방랑자] 기억을 붙잡겠다는 듯 주먹을 쥔다."
                    }
                }
            },
            // Tier 2: 스며드는 신뢰 (4.0 ~ 5.9)
            new BlessingDialogueSequence
            {
                sequenceId = "enc_tier2",
                priority = 10,
                minAffinity = 4.0f,
                maxAffinity = 5.9f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「이제는 네 발소리만 들어도 알겠구나. 차가운 피안의 꽃밭에도 너의 온기가 조금은 남는 모양이야.」",
                        playerAnswerText = "[방랑자] 마키나에게 옅은 안도를 표한다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「너의 검이 무뎌지지 않도록, 오늘도 내가 곁에서 길을 비추어주마.」",
                        playerAnswerText = "[방랑자] 고개를 숙여 감사를 전한다."
                    }
                }
            },
            // Tier 3: 왜곡의 심층 (6.0 ~ 7.9)
            new BlessingDialogueSequence
            {
                sequenceId = "enc_tier3",
                priority = 10,
                minAffinity = 6.0f,
                maxAffinity = 7.9f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「더 깊은 왜곡의 안쪽까지 다다랐군... 그곳의 어둠이 네 뺨을 스친 흔적이 보여.」",
                        playerAnswerText = "[방랑자] 심층에서 느꼈던 한기를 떠올린다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「두려워하지 마라. 꽃잎이 흩날리는 한, 너는 결코 완전히 길을 잃지 않을 테니.」",
                        playerAnswerText = "[방랑자] 결의에 찬 눈으로 앞을 바라본다."
                    }
                }
            },
            // Tier 4: 영혼의 맹약 (8.0+)
            new BlessingDialogueSequence
            {
                sequenceId = "enc_tier4",
                priority = 10,
                minAffinity = 8.0f,
                maxAffinity = 999.0f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「...나의 방랑자여. 이제 그대에게서 망설임의 그림자는 찾아볼 수 없구나.」",
                        playerAnswerText = "[방랑자] 마키나의 눈을 똑바로 마주한다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「끝까지 가보렴. 굴레를 끊어낼 마지막 순간까지, 내가 너의 곁을 지키겠다.」",
                        playerAnswerText = "[방랑자] 맹세를 담아 고개를 끄덕인다."
                    }
                }
            }
        };

        // 일반 은총 선택지 풀 (1~3번)
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

        // 4번째 [교감] 전용 선택지
        blessingData.talkChoice = new BlessingChoice
        {
            choiceId = "affinity_talk",
            title = "교감",
            description = "마키나와 대화를 나누어 친밀도를 1 상승시키고, 버텍스의 숨겨진 기억을 듣습니다.",
            icon = petalIcon,
            effectType = BlessingEffectType.AffinityTalk,
            valueCount = 1,
            titleColor = new Color(0.78f, 0.58f, 0.98f) // 연보라
        };

        // 4번 선택 시 심화 핑퐁 대화 풀 (Tier 0 ~ Tier 4)
        blessingData.affinityTalkSequences = new List<BlessingDialogueSequence>
        {
            // Talk Tier 0
            new BlessingDialogueSequence
            {
                sequenceId = "talk_tier0",
                priority = 10,
                minAffinity = 0f,
                maxAffinity = 1.9f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「나에 대해 묻고 싶은 게 많은 눈치구나. 하지만 서두를 것 없다.」",
                        playerAnswerText = "[방랑자] 조용히 귀를 기울인다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「이곳은 버텍스... 수많은 시간의 파편과 멈춰버린 생명이 똬리를 튼 관문이란다.」",
                        playerAnswerText = "[방랑자] 주변에 흩날리는 백색 꽃잎을 바라본다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「언젠가 네가 더 깊은 곳에 도달했을 때, 그때 더 많은 이야기를 들려주마. 지금은 무기를 다듬으렴.」",
                        playerAnswerText = "[방랑자] 고개를 끄덕이며 다음 길을 준비한다."
                    }
                }
            },
            // Talk Tier 1
            new BlessingDialogueSequence
            {
                sequenceId = "talk_tier1",
                priority = 10,
                minAffinity = 2.0f,
                maxAffinity = 3.9f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「기억에 대해 묻는 것인가... 네가 잊은 과거가 무엇인지 나 역시 전부 알지는 못한단다.」",
                        playerAnswerText = "[방랑자] 잃어버린 기억의 단편을 더듬는다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「하지만 이것만은 명심하렴. 버텍스는 잃어버린 것을 찾아 헤매는 자를 가장 잔혹하게 기만하지.」",
                        playerAnswerText = "[방랑자] 침묵 속에서 경각심을 새긴다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「네가 붙잡아야 할 것은 지나간 어제가 아니라, 지금 네 손에 쥐어진 무기란다.」",
                        playerAnswerText = "[방랑자] 무기의 손잡이를 단단히 쥔다."
                    }
                }
            },
            // Talk Tier 2
            new BlessingDialogueSequence
            {
                sequenceId = "talk_tier2",
                priority = 10,
                minAffinity = 4.0f,
                maxAffinity = 5.9f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「...내가 왜 사신이라 불리며 이곳에 남았는지 궁금한 모양이군.」",
                        playerAnswerText = "[방랑자] 마키나의 슬픈 눈동자를 바라본다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「나 역시 한때는 무언가를 되돌리려 저 안쪽으로 향했었단다. 소중한 것을 구원할 수 있으리라 믿었지.」",
                        playerAnswerText = "[방랑자] '그 결과가... 이 꽃밭입니까?'"
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「...결국 남은 건 이 백색 피안화들뿐이었어. 하지만 너라면... 나와는 다른 결말을 맺을지도 모르겠구나.」",
                        playerAnswerText = "[방랑자] 그녀의 짐까지 짊어지듯 결연히 끄덕인다."
                    }
                }
            },
            // Talk Tier 3
            new BlessingDialogueSequence
            {
                sequenceId = "talk_tier3",
                priority = 10,
                minAffinity = 6.0f,
                maxAffinity = 7.9f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「버텍스의 중심부에 대해 듣고 싶다고? ...좋다. 이제는 말해줄 때가 된 것 같군.」",
                        playerAnswerText = "[방랑자] 긴장 속에 집중한다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「그곳에는 구원이 존재하지 않는다. 오직 누군가의 가장 지독한 후회와, 멈춰버린 간절한 바람이 굳어 만든 거대한 감옥일 뿐이지.」",
                        playerAnswerText = "[방랑자] '그렇다면 제가 깨뜨려야 할 것은...'"
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「그래. 그 굳어버린 시공간의 매듭을 끊어내야 한다. 그러지 않으면 이 끔찍한 윤회는 영원히 끝나지 않아.」",
                        playerAnswerText = "[방랑자] 맹세를 가슴에 품는다."
                    }
                }
            },
            // Talk Tier 4
            new BlessingDialogueSequence
            {
                sequenceId = "talk_tier4",
                priority = 10,
                minAffinity = 8.0f,
                maxAffinity = 999.0f,
                steps = new List<BlessingDialogueStep>
                {
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「...진정한 끝을 향해 갈 준비가 되었느냐, 나의 방랑자여.」",
                        playerAnswerText = "[방랑자] 망설임 없이 검을 들어 올린다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「내 가슴 속 가장 깊은 곳에 간직해 두었던 백색 피안화의 씨앗을 그대에게 맡기마. 이것이 진실된 관문을 여는 열쇠가 될 것이다.」",
                        playerAnswerText = "[방랑자] 피안화의 씨앗을 받아 든다."
                    },
                    new BlessingDialogueStep
                    {
                        speakerName = "마키나",
                        npcDialogue = "「두려워하지 말고 나아가라. 네가 이 굴레의 끝에서 다시 눈뜰 때까지, 나는 항상 이곳에서 너를 기다릴 터이니.」",
                        playerAnswerText = "[방랑자] 약속을 남기고 문을 향해 걸어 나간다."
                    }
                }
            }
        };

        EditorUtility.SetDirty(blessingData);
        AssetDatabase.SaveAssets();

        // 2. ChoiceOverlay hierarchy under BlessingView
        Transform overlayTrans = blessingGo.transform.Find("ChoiceOverlay");
        if (overlayTrans == null)
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
        GameObject nameplateGo = nameplateTrans != null ? nameplateTrans.gameObject : new GameObject("Nameplate", typeof(RectTransform));
        nameplateGo.transform.SetParent(overlayTrans, false);

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
        GameObject bubbleGo = bubbleTrans != null ? bubbleTrans.gameObject : new GameObject("DialogueBubble", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(HorizontalLayoutGroup));
        bubbleGo.transform.SetParent(overlayTrans, false);

        var bubbleRt = bubbleGo.GetComponent<RectTransform>();
        bubbleRt.anchorMin = new Vector2(0.5f, 0f);
        bubbleRt.anchorMax = new Vector2(0.5f, 0f);
        bubbleRt.pivot = new Vector2(0.5f, 0f);
        bubbleRt.anchoredPosition = new Vector2(0f, 260f);
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

        // 5. Player Answer Button (핑퐁 대화용 응답 버튼)
        Transform answerBtnTrans = overlayTrans.Find("PlayerAnswerButton");
        GameObject answerBtnGo = answerBtnTrans != null ? answerBtnTrans.gameObject : new GameObject("PlayerAnswerButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(HorizontalLayoutGroup));
        answerBtnGo.transform.SetParent(overlayTrans, false);

        var answerRt = answerBtnGo.GetComponent<RectTransform>();
        answerRt.anchorMin = new Vector2(0.5f, 0f);
        answerRt.anchorMax = new Vector2(0.5f, 0f);
        answerRt.pivot = new Vector2(0.5f, 0f);
        answerRt.anchoredPosition = new Vector2(0f, 202f);
        answerRt.sizeDelta = new Vector2(820f, 44f);

        var answerImg = answerBtnGo.GetComponent<Image>();
        answerImg.color = new Color(0.10f, 0.13f, 0.19f, 0.95f);

        var answerBtn = answerBtnGo.GetComponent<Button>();
        var answerColors = answerBtn.colors;
        answerColors.normalColor = new Color(0.10f, 0.13f, 0.19f, 0.95f);
        answerColors.highlightedColor = new Color(0.18f, 0.25f, 0.38f, 1.0f);
        answerColors.pressedColor = new Color(0.25f, 0.35f, 0.50f, 1.0f);
        answerColors.selectedColor = new Color(0.18f, 0.25f, 0.38f, 1.0f);
        answerBtn.colors = answerColors;

        var answerOutline = answerBtnGo.GetComponent<Outline>();
        if (answerOutline == null) answerOutline = answerBtnGo.AddComponent<Outline>();
        answerOutline.effectColor = new Color(0.38f, 0.52f, 0.72f, 0.55f);
        answerOutline.effectDistance = new Vector2(1f, -1f);

        var answerHlg = answerBtnGo.GetComponent<HorizontalLayoutGroup>();
        answerHlg.padding = new RectOffset(20, 20, 6, 6);
        answerHlg.spacing = 10f;
        answerHlg.childAlignment = TextAnchor.MiddleLeft;
        answerHlg.childControlWidth = true;
        answerHlg.childControlHeight = true;
        answerHlg.childForceExpandWidth = true;
        answerHlg.childForceExpandHeight = true;

        Transform answerTextTrans = answerBtnGo.transform.Find("PlayerAnswerText");
        GameObject answerTextGo = answerTextTrans != null ? answerTextTrans.gameObject : new GameObject("PlayerAnswerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        answerTextGo.transform.SetParent(answerBtnGo.transform, false);
        var answerTmp = answerTextGo.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) answerTmp.font = fontAsset;
        answerTmp.fontSize = 17f;
        answerTmp.alignment = TextAlignmentOptions.MidlineLeft;
        answerTmp.color = new Color(0.88f, 0.92f, 0.98f, 1f);
        answerTmp.text = "[방랑자] ...";

        // 6. ChoicePillList (4슬롯 알약 리스트)
        Transform pillListTrans = overlayTrans.Find("ChoicePillList");
        GameObject pillListGo = pillListTrans != null ? pillListTrans.gameObject : new GameObject("ChoicePillList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        pillListGo.transform.SetParent(overlayTrans, false);

        var pillListRt = pillListGo.GetComponent<RectTransform>();
        pillListRt.anchorMin = new Vector2(0.5f, 0f);
        pillListRt.anchorMax = new Vector2(0.5f, 0f);
        pillListRt.pivot = new Vector2(0.5f, 0f);
        pillListRt.anchoredPosition = new Vector2(0f, 42f);
        pillListRt.sizeDelta = new Vector2(820f, 216f);

        var vlg = pillListGo.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var choiceRowUIs = new List<BlessingChoiceRowUI>();

        for (int i = 0; i < 4; i++)
        {
            string rowName = $"ChoicePill_{i}";
            Transform rowTrans = pillListGo.transform.Find(rowName);
            GameObject rowGo = rowTrans != null ? rowTrans.gameObject : new GameObject(rowName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(LayoutElement), typeof(HorizontalLayoutGroup), typeof(BlessingChoiceRowUI));
            rowGo.transform.SetParent(pillListGo.transform, false);

            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(820f, 46f);

            var le = rowGo.GetComponent<LayoutElement>();
            if (le == null) le = rowGo.AddComponent<LayoutElement>();
            le.preferredHeight = 46f;
            le.minHeight = 46f;

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
            rowHlg.padding = new RectOffset(16, 16, 4, 4);
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
            iconRt.sizeDelta = new Vector2(30f, 30f);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Title
            Transform titleTrans = rowGo.transform.Find("Title");
            GameObject titleGo = titleTrans != null ? titleTrans.gameObject : new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(rowGo.transform, false);
            var tRt = titleGo.GetComponent<RectTransform>();
            tRt.sizeDelta = new Vector2(90f, 34f);
            var tTmp = titleGo.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) tTmp.font = fontAsset;
            tTmp.fontSize = 19f;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.alignment = TextAlignmentOptions.MidlineLeft;
            tTmp.raycastTarget = false;

            // Desc
            Transform descTrans = rowGo.transform.Find("Desc");
            GameObject descGo = descTrans != null ? descTrans.gameObject : new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            descGo.transform.SetParent(rowGo.transform, false);
            var dRt = descGo.GetComponent<RectTransform>();
            dRt.sizeDelta = new Vector2(630f, 34f);
            var dTmp = descGo.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) dTmp.font = fontAsset;
            dTmp.fontSize = 15f;
            dTmp.alignment = TextAlignmentOptions.MidlineLeft;
            dTmp.color = new Color(0.77f, 0.82f, 0.88f, 1f);
            dTmp.raycastTarget = false;

            var rowUI = rowGo.GetComponent<BlessingChoiceRowUI>();
            if (rowUI == null) rowUI = rowGo.AddComponent<BlessingChoiceRowUI>();

            var rowSo = new SerializedObject(rowUI);
            rowSo.Update();
            rowSo.FindProperty("button").objectReferenceValue = rowBtn;
            rowSo.FindProperty("iconImage").objectReferenceValue = iconImg;
            rowSo.FindProperty("titleText").objectReferenceValue = tTmp;
            rowSo.FindProperty("descText").objectReferenceValue = dTmp;
            rowSo.ApplyModifiedProperties();

            choiceRowUIs.Add(rowUI);
        }

        // 7. Serialized properties on BlessingView
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
        bvSo.FindProperty("playerAnswerButton").objectReferenceValue = answerBtn;
        bvSo.FindProperty("playerAnswerText").objectReferenceValue = answerTmp;
        bvSo.FindProperty("choiceContainer").objectReferenceValue = pillListGo.transform;

        var choiceRowsProp = bvSo.FindProperty("choiceRows");
        choiceRowsProp.ClearArray();
        for (int i = 0; i < choiceRowUIs.Count; i++)
        {
            choiceRowsProp.InsertArrayElementAtIndex(i);
            choiceRowsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceRowUIs[i];
        }

        var mapCtrl = Object.FindObjectOfType<MapUIController>(true);
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

        // Ensure [BlessingAffinityManager] exists in scene hierarchy for easy Inspector debugging
        var affinityMgr = Object.FindObjectOfType<BlessingAffinityManager>();
        if (affinityMgr == null)
        {
            var mgrGo = new GameObject("[BlessingAffinityManager]");
            affinityMgr = mgrGo.AddComponent<BlessingAffinityManager>();
            Undo.RegisterCreatedObjectUndo(mgrGo, "Create BlessingAffinityManager");
        }
        affinityMgr.SyncInspectorFields();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        return $"Blessing UI Setup Complete! BlessingData '{blessingData.name}' connected with 4 pill rows and PlayerAnswerButton.";
    }
}
