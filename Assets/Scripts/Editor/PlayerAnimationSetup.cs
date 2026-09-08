using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ============================================================
// filename   : PlayerAnimationSetup.cs
// description: player_fidle_sprite_sheet(62프레임) 스프라이트를 로드하여
//              PlayerChar.asset의 idleFrames에 할당하고,
//              PlayerDraft 오브젝트에 UISpriteSheetAnimator를 설정하는 도구
// ============================================================
public static class PlayerAnimationSetup
{
    [MenuItem("Tools/Player/Setup Player F Idle (Sprite Sheet)")]
    public static string Setup()
    {
        string spriteSheetPath = "Assets/Art/Characters/Joining Characters/Player/Player_F/player_fidle_sprite_sheet.png";
        string playerCharPath = "Assets/Data/Player/PlayerChar.asset";

        // 1. 스프라이트 로드 및 인덱스 순 정렬
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

        int ExtractIndex(string name)
        {
            int idx = name.LastIndexOf('_');
            if (idx >= 0 && int.TryParse(name.Substring(idx + 1), out int res))
                return res;
            return -1;
        }

        sprites.Sort((a, b) => ExtractIndex(a.name).CompareTo(ExtractIndex(b.name)));

        // 2. PlayerChar.asset에 idleFrames 및 FPS 할당
        PlayerCharData playerChar = AssetDatabase.LoadAssetAtPath<PlayerCharData>(playerCharPath);
        if (playerChar != null)
        {
            playerChar.idleFrames = sprites.ToArray();
            playerChar.idleFrameRate = 24f;
            EditorUtility.SetDirty(playerChar);
            AssetDatabase.SaveAssets();
        }

        // 3. 씬 내 PlayerDraft 오브젝트 설정
        GameObject playerGo = GameObject.Find("PlayerDraft");
        if (playerGo != null)
        {
            // 불필요해진 Animator 제거
            var anim = playerGo.GetComponent<Animator>();
            if (anim != null) Object.DestroyImmediate(anim);

            // UISpriteSheetAnimator 부착 및 프레임 설정
            var sheetAnim = playerGo.GetComponent<UISpriteSheetAnimator>();
            if (sheetAnim == null) sheetAnim = playerGo.AddComponent<UISpriteSheetAnimator>();
            sheetAnim.FrameRate = 24f;
            sheetAnim.SetFrames(sprites.ToArray(), true);

            // PartyView 연결
            var partyViewer = Object.FindFirstObjectByType<PartyView>();
            if (partyViewer != null)
            {
                var so = new SerializedObject(partyViewer);
                var sheetProp = so.FindProperty("playerSheetAnimator");
                if (sheetProp != null)
                {
                    sheetProp.objectReferenceValue = sheetAnim;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(partyViewer);
                }
            }

            EditorUtility.SetDirty(playerGo);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(playerGo.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        return $"Success! Assigned {sprites.Count} frames to PlayerChar.asset.idleFrames and configured PlayerDraft UISpriteSheetAnimator.";
    }
}
