using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// HP 게이지가 채워지지 않는 문제를 고친다.
//
// 원인: 게이지용 Image에 Sprite가 비어 있었다.
// Unity의 Image.OnPopulateMesh()는 sprite가 null이면 type을 무시하고 단순 사각형만 그린다.
// 그래서 코드가 fillAmount를 넣어도 화면에는 항상 꽉 찬 바가 보였다.
// 여기에 더해 type이 Simple, fillMethod가 Radial360으로 되어 있어 세 가지를 모두 바로잡는다.
//
// 플레이어 쪽은 게이지 오브젝트 자체가 없어서 새로 만든다.
// 위치/크기는 배경 아트(UI1 스프라이트)에 그려진 흰 띠 슬롯을 픽셀 측정해 맞췄다.
public static class HPBarFixup
{
    private const string EnemyPrefabPath = "Assets/Data/Enemy/Prefabs/EnemyView.prefab";
    private const string FillSpritePath  = "Assets/Art/UI/UIFillWhite.png";

    // 배경 아트의 흰 띠 슬롯 (253x322 스프라이트 기준 x 27~178, y 178~186 을 중심 기준으로 환산)
    private static readonly Vector2 PlayerFillPos  = new(-24f, -21f);
    private static readonly Vector2 PlayerFillSize = new(152f, 9f);

    [MenuItem("Tools/Vertex/Fix HP Bars")]
    public static void Fix()
    {
        string enemy  = FixEnemy();
        string player = FixPlayer();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log($"[HPBarFixup]\n적: {enemy}\n플레이어: {player}");
    }

    // Sprite가 없으면 fillAmount가 통째로 무시되므로 반드시 하나 넣어야 한다.
    // 빌트인 UISprite는 모서리가 둥글어서, 비균등 스케일이 걸린 적 게이지(x3, y0.23)에서 모서리가 찌그러진다.
    // 단색 8x8 스프라이트를 쓰면 어떤 비율로 늘려도 평평하게 나온다.
    private static Sprite FillSprite()
    {
        EnsureFillSpriteImported();

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(FillSpritePath);
        if (sprite != null) return sprite;

        Debug.LogWarning($"[HPBarFixup] {FillSpritePath}를 찾지 못해 빌트인 스프라이트로 대체합니다.");
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    // 단색 PNG가 Texture2D로 임포트되어 있으면 Sprite로 로드되지 않는다. 임포트 설정을 보장한다.
    private static void EnsureFillSpriteImported()
    {
        if (AssetImporter.GetAtPath(FillSpritePath) is not TextureImporter importer) return;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }
        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            changed = true;
        }
        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            changed = true;
        }

        if (changed) importer.SaveAndReimport();
    }

    private static bool ConfigureFillImage(Image image)
    {
        bool changed = false;

        // null일 때만이 아니라 "의도한 스프라이트가 아닐 때"도 교체한다.
        // 이전 실행에서 다른 스프라이트가 들어갔을 수 있어서 재실행으로 교정 가능해야 한다.
        var wanted = FillSprite();
        if (wanted != null && image.sprite != wanted)
        {
            image.sprite = wanted;
            changed = true;
        }
        if (image.type != Image.Type.Filled)
        {
            image.type = Image.Type.Filled;
            changed = true;
        }
        if (image.fillMethod != Image.FillMethod.Horizontal)
        {
            image.fillMethod = Image.FillMethod.Horizontal;
            changed = true;
        }
        // Horizontal에서 0 = Left. 왼쪽에서 오른쪽으로 차오른다.
        if (image.fillOrigin != (int)Image.OriginHorizontal.Left)
        {
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            changed = true;
        }
        return changed;
    }

    private static string FixEnemy()
    {
        var root = PrefabUtility.LoadPrefabContents(EnemyPrefabPath);
        try
        {
            var view = root.GetComponent<EnemyView>();
            if (view == null) return "EnemyView 컴포넌트 없음.";

            var so = new SerializedObject(view);
            var fillProp = so.FindProperty("hpFill");
            if (fillProp?.objectReferenceValue is not Image fill)
                return "hpFill이 연결되어 있지 않음.";

            bool changed = ConfigureFillImage(fill);
            if (!changed) return "이미 올바르게 설정되어 있음.";

            PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            return $"'{fill.name}' 수정 (Sprite/Filled/Horizontal/Left).";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static string FixPlayer()
    {
        var hud = Object.FindFirstObjectByType<PlayerHUDView>(FindObjectsInactive.Include);
        if (hud == null) return "PlayerHUDView를 씬에서 찾지 못함.";

        var so = new SerializedObject(hud);
        var fillProp = so.FindProperty("hpFill");
        if (fillProp == null) return "hpFill 필드가 없음 (스크립트 재컴파일 필요).";

        // 게이지는 배경(HPBackground) 위에 올라가야 한다. hpText의 부모가 곧 배경 패널이다.
        var textProp = so.FindProperty("hpText");
        if (textProp?.objectReferenceValue is not Component hpText)
            return "hpText가 연결되어 있지 않아 배경 패널을 찾을 수 없음.";

        Transform background = hpText.transform.parent;
        if (background == null) return "hpText의 부모(배경 패널)를 찾지 못함.";

        Undo.RegisterFullObjectHierarchyUndo(background.gameObject, "Fix HP Bars");

        var existing = background.Find("HPFill");
        GameObject fillGo = existing != null
            ? existing.gameObject
            : new GameObject("HPFill", typeof(RectTransform), typeof(Image));

        if (existing == null)
        {
            fillGo.transform.SetParent(background, false);
            // 텍스트보다 뒤에 그려지도록 맨 앞으로 보낸다.
            fillGo.transform.SetAsFirstSibling();
        }

        var rect = (RectTransform)fillGo.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot     = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = PlayerFillPos;
        rect.sizeDelta = PlayerFillSize;

        var image = fillGo.GetComponent<Image>();
        ConfigureFillImage(image);
        image.color = new Color(0.85f, 0.18f, 0.18f, 1f);
        image.raycastTarget = false;

        fillProp.objectReferenceValue = image;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(hud);

        return $"'{background.name}' 아래에 HPFill {(existing != null ? "갱신" : "생성")} 후 연결.";
    }
}
