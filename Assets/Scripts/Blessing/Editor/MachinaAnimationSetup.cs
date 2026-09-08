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
        string spriteSheetPath = "Assets/Art/Blessing/Machina/machina_idle_sprite_sheet.png";
        string animFolder = "Assets/Art/Blessing/Machina/Animations";
        string clipPath = $"{animFolder}/machina_idle.anim";
        string controllerPath = $"{animFolder}/Machina_Animator.controller";

        // 1. 스프라이트 32종 로드 및 번호순 정렬
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);
        List<Sprite> sprites = new List<Sprite>();
        foreach (var a in subAssets)
        {
            if (a is Sprite s) sprites.Add(s);
        }

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

        // 2. AnimationClip 생성 또는 로드
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        bool clipCreated = false;
        if (clip == null)
        {
            clip = new AnimationClip();
            clipCreated = true;
        }

        clip.frameRate = 16f; // 32프레임 기준 16FPS = 2초 1루프

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

        if (charObj == null)
        {
            return $"Clip and Controller generated ({sprites.Count} frames), but BlessingCharacter GameObject not found in open scene.";
        }

        Undo.RegisterFullObjectHierarchyUndo(charObj, "Setup Machina Animation");

        // Image 세팅
        Image img = charObj.GetComponent<Image>();
        if (img == null) img = Undo.AddComponent<Image>(charObj);
        img.sprite = sprites[0];
        img.preserveAspect = true;
        img.raycastTarget = false;

        // RectTransform 위치 & 크기 최적화 (1080p 화면에 맞춤)
        RectTransform rt = charObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        // 원본 해상도 577x1040에 맞추어 적절한 크기로 설정 (너비 560, 높이 1000)
        rt.sizeDelta = new Vector2(560f, 1000f);
        rt.anchoredPosition = new Vector2(0f, -40f); // 바닥에 가깝게 자연스럽게 배치

        // Animator 컴포넌트 부착 및 컨트롤러 연결
        Animator animator = charObj.GetComponent<Animator>();
        if (animator == null) animator = Undo.AddComponent<Animator>(charObj);
        animator.runtimeAnimatorController = controller;
        animator.updateMode = AnimatorUpdateMode.Normal;

        // UISpriteSheetAnimator 컴포넌트도 함께 부착 (스크립트 제어용)
        UISpriteSheetAnimator sheetAnim = charObj.GetComponent<UISpriteSheetAnimator>();
        if (sheetAnim == null) sheetAnim = Undo.AddComponent<UISpriteSheetAnimator>(charObj);
        sheetAnim.SetFrames(sprites.ToArray());
        sheetAnim.FrameRate = 16f;

        EditorUtility.SetDirty(charObj);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        return $"Success! Loaded {sprites.Count} frames. AnimationClip + AnimatorController created and BlessingCharacter configured.";
    }

    private static int ExtractIndex(string name)
    {
        int lastUnder = name.LastIndexOf('_');
        if (lastUnder >= 0 && int.TryParse(name.Substring(lastUnder + 1), out int val))
            return val;
        return 0;
    }
}
