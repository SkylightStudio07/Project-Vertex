using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// filename   : MachinaAnimationSetup.cs
// description: machina_idle_sprite_sheet(32프레임)으로
//              AnimationClip 및 AnimatorController를 생성하고
//              BlessingCharacter에 UISpriteSheetAnimator 및 Animator를 설정하는 도구
// ============================================================
public static class MachinaAnimationSetup
{
    [MenuItem("Tools/Blessing/Setup Machina Animation")]
    public static string Setup()
    {
        string spriteSheetPath = "Assets/Art/Blessing/Machina/machina_idle_sprite_64_sheet.png";
        string animFolder = "Assets/Art/Blessing/Machina/Animations";
        string clipPath = $"{animFolder}/machina_idle.anim";
        string controllerPath = $"{animFolder}/Machina_Animator.controller";

        // 1. 스프라이트 강제 재임포트 후 로드 및 번호순 정렬
        AssetDatabase.ImportAsset(spriteSheetPath, ImportAssetOptions.ForceUpdate);
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);
        List<Sprite> sprites = new List<Sprite>();
        foreach (var a in subAssets)
        {
            if (a is Sprite s) sprites.Add(s);
        }

        Debug.Log($"[MachinaAnimationSetup] Raw loaded subAssets: {subAssets.Length}, sprites: {sprites.Count} from {spriteSheetPath}");

        if (sprites.Count == 0)
        {
            return "Error: No sprites found in " + spriteSheetPath;
        }

        sprites.Sort((a, b) =>
        {
            int ia = ExtractIndex(a.name);
            int ib = ExtractIndex(b.name);
            return ia.CompareTo(ib);
        });

        // 64프레임 시트의 경우 마지막 2개 정지 패딩 프레임(62, 63)을 제외하고
        // 62프레임(0..61)을 루프로 사용 (31프레임 2사이클 완벽 순환)
        if (sprites.Count == 64)
        {
            sprites = sprites.GetRange(0, 62);
        }

        Debug.Log($"[MachinaAnimationSetup] Configured animation with {sprites.Count} frames.");

        // 2. AnimationClip 생성 또는 로드
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        bool clipCreated = false;
        if (clip == null)
        {
            clip = new AnimationClip();
            clipCreated = true;
        }

        clip.frameRate = 16f; // 62프레임 기준 16FPS = 3.875초 (1회 호흡당 약 1.93초)

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(Image),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / clip.frameRate,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // 루프 애니메이션 설정
        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        if (clipCreated)
        {
            AssetDatabase.CreateAsset(clip, clipPath);
        }
        else
        {
            EditorUtility.SetDirty(clip);
        }

        // 3. AnimatorController 생성 또는 로드
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddMotion(clip);
        }
        else
        {
            // 기본 상태에 모션 갱신
            if (controller.layers.Length > 0 && controller.layers[0].stateMachine.states.Length > 0)
            {
                controller.layers[0].stateMachine.states[0].state.motion = clip;
            }
            else
            {
                controller.AddMotion(clip);
            }
            EditorUtility.SetDirty(controller);
        }

        AssetDatabase.SaveAssets();

        // 4. 씬 내 BlessingCharacter 오브젝트 설정
        GameObject charObj = GameObject.Find("BlessingCharacter");
        if (charObj == null)
        {
            var bv = GameObject.Find("BlessingView");
            if (bv != null)
            {
                var t = bv.transform.Find("BlessingCharacter");
                if (t != null) charObj = t.gameObject;
            }
        }

        if (charObj != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(charObj, "Setup Machina Animation");

            // Image 세팅
            Image img = charObj.GetComponent<Image>();
            if (img == null) img = Undo.AddComponent<Image>(charObj);
            img.sprite = sprites[0];
            img.preserveAspect = true;
            img.raycastTarget = false;

            // UISpriteSheetAnimator 컴포넌트 부착 및 62프레임 직렬화 설정
            UISpriteSheetAnimator sheetAnim = charObj.GetComponent<UISpriteSheetAnimator>();
            if (sheetAnim == null) sheetAnim = Undo.AddComponent<UISpriteSheetAnimator>(charObj);

            SerializedObject sheetAnimSo = new SerializedObject(sheetAnim);
            sheetAnimSo.Update();
            SerializedProperty framesProp = sheetAnimSo.FindProperty("frames");
            framesProp.ClearArray();
            for (int i = 0; i < sprites.Count; i++)
            {
                framesProp.InsertArrayElementAtIndex(i);
                framesProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
            sheetAnimSo.FindProperty("frameRate").floatValue = 16f;
            sheetAnimSo.FindProperty("loop").boolValue = true;
            sheetAnimSo.FindProperty("playOnAwake").boolValue = true;
            sheetAnimSo.ApplyModifiedProperties();

            sheetAnim.SetFrames(sprites.ToArray());
            sheetAnim.FrameRate = 16f;

            // Animator가 중복되어 충돌하지 않도록 제거 (UISpriteSheetAnimator 단독 사용)
            Animator animator = charObj.GetComponent<Animator>();
            if (animator != null)
            {
                Object.DestroyImmediate(animator);
            }

            EditorUtility.SetDirty(charObj);
        }

        // 5. SpeakerAvatar 오브젝트 설정 (BlessingView 하위)
        GameObject avatarObj = GameObject.Find("SpeakerAvatar");
        if (avatarObj != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(avatarObj, "Setup Machina SpeakerAvatar");
            Image avatarImg = avatarObj.GetComponent<Image>();
            if (avatarImg != null)
            {
                avatarImg.sprite = sprites[0];
                avatarImg.preserveAspect = true;
                EditorUtility.SetDirty(avatarObj);
            }
        }

        // 6. BlessingData SO 갱신
        string blessingDataPath = "Assets/Data/Blessing/Blessing_Machina_Floor0.asset";
        BlessingData blessingData = AssetDatabase.LoadAssetAtPath<BlessingData>(blessingDataPath);
        if (blessingData != null)
        {
            Undo.RecordObject(blessingData, "Update Machina Speaker Icon");
            blessingData.speakerIcon = sprites[0];
            EditorUtility.SetDirty(blessingData);
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        return $"Success! Loaded {sprites.Count} frames. AnimationClip + AnimatorController updated and BlessingCharacter + SpeakerAvatar configured.";
    }

    private static int ExtractIndex(string name)
    {
        int lastUnder = name.LastIndexOf('_');
        if (lastUnder >= 0 && int.TryParse(name.Substring(lastUnder + 1), out int val))
            return val;
        return 0;
    }
}
