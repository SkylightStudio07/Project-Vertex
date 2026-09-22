using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// ============================================================
// filename   : EnemyAnimationSetup.cs
// description: 견마_idle_sheet.png(16프레임, 8x2)을 정확한 그리드로 슬라이스하고
//              제파러(EnemyData)에 idleFrames(12 FPS) 및 기본 스프라이트로 연결
// ============================================================
public static class EnemyAnimationSetup
{
    public static string SetupGyeonma()
    {
        string texturePath = "Assets/Art/Characters/Enemy/견마/견마_idle_sheet.png";
        string enemyDataPath = "Assets/Data/Enemy/EnemyDatas/제파러/제파러.asset";

        // 1. TextureImporter 설정
        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null) return "Error: TextureImporter not found for " + texturePath;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = 8192;
        importer.filterMode = FilterMode.Bilinear;

        var defaultSettings = importer.GetDefaultPlatformTextureSettings();
        defaultSettings.maxTextureSize = 8192;
        defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SetPlatformTextureSettings(defaultSettings);

        var standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
        standaloneSettings.maxTextureSize = 8192;
        standaloneSettings.textureCompression = TextureImporterCompression.Uncompressed;
        standaloneSettings.overridden = true;
        importer.SetPlatformTextureSettings(standaloneSettings);

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        // 2. ISpriteEditorDataProvider를 이용한 안전한 그리드 슬라이스 (Safe Core Pattern)
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var editCapability = dataProvider.GetDataProvider<ISpriteFrameEditCapability>();
        if (editCapability == null || !editCapability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite))
        {
            return "Error: CreateAndDeleteSprite capability not supported by importer.";
        }

        int cols = 8;
        int rows = 2;
        int fw = 832;
        int fh = 1216;
        int totalH = 2432;

        var rects = new List<SpriteRect>();
        var namePairs = new List<SpriteNameFileIdPair>();

        for (int r = 0; r < rows; r++)
        {
            // 상단 행이 r = 0 -> 유니티 좌하단 원점 기준 y 좌표 = totalH - (r + 1) * fh
            int unityY = totalH - (r + 1) * fh;
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                int unityX = c * fw;
                string spriteName = $"견마_idle_sheet_{idx}";
                var sr = new SpriteRect
                {
                    name = spriteName,
                    spriteID = GUID.Generate(),
                    rect = new Rect(unityX, unityY, fw, fh),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
                rects.Add(sr);
                namePairs.Add(new SpriteNameFileIdPair(spriteName, sr.spriteID));
            }
        }

        dataProvider.SetSpriteRects(rects.ToArray());

        var nameFileIdProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (nameFileIdProvider != null)
        {
            nameFileIdProvider.SetNameFileIdPairs(namePairs);
        }

        dataProvider.Apply();
        importer.SaveAndReimport();
        AssetDatabase.Refresh();

        // 3. 슬라이스된 스프라이트 로드 및 인덱스 정렬
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        var sprites = new List<Sprite>();
        foreach (var a in subAssets)
        {
            if (a is Sprite s) sprites.Add(s);
        }

        int ExtractIdx(string name)
        {
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore >= 0 && int.TryParse(name.Substring(lastUnderscore + 1), out int res))
                return res;
            return -1;
        }

        sprites.Sort((a, b) => ExtractIdx(a.name).CompareTo(ExtractIdx(b.name)));

        if (sprites.Count == 0)
        {
            return "Error: No sprites loaded after slicing " + texturePath;
        }

        // 4. 제파러.asset에 할당
        var enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(enemyDataPath);
        if (enemyData == null) return "Error: EnemyData not found at " + enemyDataPath;

        enemyData.enemyImage = sprites[0];
        enemyData.idleFrames = sprites.ToArray();
        enemyData.idleFrameRate = 12f; // 16프레임 기준 약 1.33초 1루프

        EditorUtility.SetDirty(enemyData);
        AssetDatabase.SaveAssets();

        return $"Success! Sliced {sprites.Count} frames (832x1216) for 견마_idle_sheet and assigned to '{enemyData.name}' (12 FPS loop).";
    }

    public static string AttachAnimatorToPrefab()
    {
        string prefabPath = "Assets/Data/Enemy/Prefabs/EnemyView.prefab";
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        var spriteTrans = root.transform.Find("Enemy Sprite");
        if (spriteTrans != null)
        {
            var anim = spriteTrans.GetComponent<UISpriteSheetAnimator>();
            if (anim == null) anim = spriteTrans.gameObject.AddComponent<UISpriteSheetAnimator>();
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            return "Added UISpriteSheetAnimator to EnemyView.prefab";
        }
        PrefabUtility.UnloadPrefabContents(root);
        return "Enemy Sprite not found";
    }
}
