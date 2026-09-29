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

    public static string SetupHund()
    {
        string texturePath = "Assets/Art/Characters/Enemy/개 - 훈트/훈트_idle_sheet.png";
        string enemyDataPath = "Assets/Data/Enemy/EnemyDatas/게회언터 훈트/게회언터 훈트.asset";

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

        // 2. ISpriteEditorDataProvider를 이용한 안전한 그리드 슬라이스
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
        int rows = 1;
        int fw = 832;
        int fh = 1216;
        int totalH = 1216;

        var rects = new List<SpriteRect>();
        var namePairs = new List<SpriteNameFileIdPair>();

        for (int r = 0; r < rows; r++)
        {
            int unityY = totalH - (r + 1) * fh;
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                int unityX = c * fw;
                string spriteName = $"훈트_idle_sheet_{idx}";
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

        // 3. 슬라이스된 스프라이트 로드 및 정렬
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

        // 4. 게회언터 훈트.asset에 할당
        var enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(enemyDataPath);
        if (enemyData == null) return "Error: EnemyData not found at " + enemyDataPath;

        enemyData.enemyImage = sprites[0];
        enemyData.idleFrames = sprites.ToArray();
        enemyData.idleFrameRate = 8f; // 8프레임 기준 1.0초 1루프

        EditorUtility.SetDirty(enemyData);
        AssetDatabase.SaveAssets();

        return $"Success! Sliced {sprites.Count} frames (832x1216) for 훈트_idle_sheet and assigned to '{enemyData.name}' (8 FPS loop).";
    }

    public static string SetupAbandonedOne()
    {
        string idlePath = "Assets/Art/Characters/Enemy/버려진 자/탈주자_idle_sheet.png";
        string attackPath = "Assets/Art/Characters/Enemy/버려진 자/탈주자_attack_sheet.png";
        string enemyDataPath = "Assets/Data/Enemy/EnemyDatas/버려진 자/버려진 자.asset";

        // 1. Idle 시트 슬라이스 (8x2, 16프레임)
        var idleSprites = SliceGrid(idlePath, 8, 2, 832, 1216, "탈주자_idle_sheet");
        if (idleSprites == null || idleSprites.Count == 0) return "Error: Failed to slice idle sheet";

        // 2. Attack 시트 슬라이스 (8x4, 32프레임)
        var allAttackSprites = SliceGrid(attackPath, 8, 4, 832, 1216, "탈주자_attack_sheet");
        if (allAttackSprites == null || allAttackSprites.Count == 0) return "Error: Failed to slice attack sheet";

        // 3. 움직임이 큰 활성 프레임만 분절 (10번~24번 프레임: 3점사 사격 화염, 반동, 탄피 배출, 자세 복귀 총 15프레임)
        int ExtractIdx(string name)
        {
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore >= 0 && int.TryParse(name.Substring(lastUnderscore + 1), out int res))
                return res;
            return -1;
        }

        var activeAttackSprites = new List<Sprite>();
        foreach (var s in allAttackSprites)
        {
            int idx = ExtractIdx(s.name);
            if (idx >= 10 && idx <= 24)
            {
                activeAttackSprites.Add(s);
            }
        }

        // 4. 버려진 자.asset에 할당
        var enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(enemyDataPath);
        if (enemyData == null) return "Error: EnemyData not found at " + enemyDataPath;

        enemyData.enemyImage = idleSprites[0];
        enemyData.idleFrames = idleSprites.ToArray();
        enemyData.idleFrameRate = 12f; // 16프레임 기준 약 1.33초 1루프
        enemyData.attackFrames = activeAttackSprites.ToArray();
        enemyData.attackFrameRate = 16f; // 15프레임 기준 약 0.94초 3점사 사격

        EditorUtility.SetDirty(enemyData);
        AssetDatabase.SaveAssets();

        return $"Success! Sliced {idleSprites.Count} idle frames (12 FPS) and {activeAttackSprites.Count} active attack frames (16 FPS, extracted 10-24 from 32) for '버려진 자'.";
    }

    public static string SetupZephyrusAttack()
    {
        string attackPath = "Assets/Art/Characters/Enemy/견마/제파러_attack_sheet.png";
        string enemyDataPath = "Assets/Data/Enemy/EnemyDatas/제파러/제파러.asset";

        // 1. Attack 시트 슬라이스 (8x4, 32프레임)
        var allAttackSprites = SliceGrid(attackPath, 8, 4, 832, 1216, "제파러_attack_sheet");
        if (allAttackSprites == null || allAttackSprites.Count == 0) return "Error: Failed to slice attack sheet for 제파러";

        // 2. 움직임이 큰 활성 프레임만 분절 (8번~23번 프레임: 주포 발사 화염, 반동, 기체 반동 제어, 자세 복귀 총 16프레임)
        int ExtractIdx(string name)
        {
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore >= 0 && int.TryParse(name.Substring(lastUnderscore + 1), out int res))
                return res;
            return -1;
        }

        var activeAttackSprites = new List<Sprite>();
        foreach (var s in allAttackSprites)
        {
            int idx = ExtractIdx(s.name);
            if (idx >= 8 && idx <= 23)
            {
                activeAttackSprites.Add(s);
            }
        }

        // 3. 제파러.asset에 할당
        var enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(enemyDataPath);
        if (enemyData == null) return "Error: EnemyData not found at " + enemyDataPath;

        enemyData.attackFrames = activeAttackSprites.ToArray();
        enemyData.attackFrameRate = 16f; // 16프레임 기준 1.0초 사격 및 반동 모션

        EditorUtility.SetDirty(enemyData);
        AssetDatabase.SaveAssets();

        return $"Success! Sliced 32 frames and assigned {activeAttackSprites.Count} active attack frames (8-23, 16 FPS) to '{enemyData.name}'.";
    }

    private static List<Sprite> SliceGrid(string texturePath, int cols, int rows, int fw, int fh, string prefix)
    {
        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("Error: TextureImporter not found for " + texturePath);
            return null;
        }

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

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var editCapability = dataProvider.GetDataProvider<ISpriteFrameEditCapability>();
        if (editCapability == null || !editCapability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite))
        {
            Debug.LogError("Error: CreateAndDeleteSprite capability not supported by importer.");
            return null;
        }

        int totalH = rows * fh;
        var rects = new List<SpriteRect>();
        var namePairs = new List<SpriteNameFileIdPair>();

        for (int r = 0; r < rows; r++)
        {
            int unityY = totalH - (r + 1) * fh;
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                int unityX = c * fw;
                string spriteName = $"{prefix}_{idx}";
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
        return sprites;
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
