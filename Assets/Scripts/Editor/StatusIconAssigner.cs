using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// Assets/Art/UI/StatusIcons의 PNG를 Sprite로 임포트하고,
// 같은 이름의 StatusDefinition(Assets/Data/Status)에 자동으로 연결한다.
//
// 파일명 규칙: 아이콘 PNG 이름 == StatusDefinition 에셋 이름 (예: Burn.png -> Burn.asset).
// 아이콘 원본 SVG와 생성 스크립트는 저장소 루트의 ArtSource/StatusIcons에 있다.
public static class StatusIconAssigner
{
    private const string IconFolder   = "Assets/Art/UI/StatusIcons";
    private const string StatusFolder = "Assets/Data/Status";

    [MenuItem("Tools/Vertex/Assign Status Icons")]
    public static void AssignIcons()
    {
        var sprites = ImportAndCollectSprites();
        if (sprites.Count == 0)
        {
            Debug.LogWarning($"[StatusIconAssigner] {IconFolder}에서 아이콘을 찾지 못했습니다.");
            return;
        }

        int assigned = 0;
        var missing = new List<string>();

        foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(StatusDefinition)}", new[] { StatusFolder }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var definition = AssetDatabase.LoadAssetAtPath<StatusDefinition>(assetPath);
            if (definition == null) continue;

            if (!sprites.TryGetValue(definition.name, out var sprite))
            {
                missing.Add(definition.name);
                continue;
            }

            // icon은 private [SerializeField]라 SerializedObject로 설정한다.
            var so = new SerializedObject(definition);
            var iconProperty = so.FindProperty("icon");
            if (iconProperty == null) continue;

            if (iconProperty.objectReferenceValue != sprite)
            {
                iconProperty.objectReferenceValue = sprite;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
                assigned++;
            }
        }

        AssetDatabase.SaveAssets();

        var report = new StringBuilder();
        report.AppendLine($"아이콘 {sprites.Count}개 임포트 완료.");
        report.AppendLine($"StatusDefinition {assigned}개에 새로 연결.");
        if (missing.Count > 0)
            report.AppendLine($"\n대응 아이콘 없음 ({missing.Count}): {string.Join(", ", missing)}");

        // 모달 대화상자 대신 콘솔에 남긴다. CLI/배치로 반복 실행해도 멈추지 않는다.
        Debug.Log($"[StatusIconAssigner] {report}");
    }

    // 아이콘 원본(1254px 안팎)은 칩(40px대)보다 훨씬 커서, 밉맵 없이 줄이면 검은 외곽선이 샘플링에서
    // 빠져 흰 덩어리로 뭉개진다. 밉맵을 켜고, 텍스처는 256으로 제한한다(칩 크기의 5~6배면 충분).
    private const int MaxTextureSize = 256;
    // 원본마다 투명 여백이 10~50%라 그대로 쓰면 칩 안의 실제 그림이 절반 크기가 된다.
    // 스프라이트 영역을 그림이 있는 부분(알파 > 임계값)의 정사각형으로 잘라낸다. 원본 PNG는 건드리지 않는다.
    private const byte AlphaThreshold = 8;
    private const float TrimPadding = 0.03f; // 잘라낸 영역 둘레에 남길 여백(한 변 대비)

    // PNG를 UI용 Sprite로 임포트 설정한 뒤 이름 -> Sprite 사전을 만든다.
    private static Dictionary<string, Sprite> ImportAndCollectSprites()
    {
        var result = new Dictionary<string, Sprite>();

        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { IconFolder }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.EndsWith(".png")) continue;

            if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple; // 영역을 잘라내려면 Multiple이어야 한다
                importer.alphaIsTransparency = true;
                // 작은 아이콘이라 압축 아티팩트가 그대로 보인다. 무압축 유지.
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Trilinear;
                importer.maxTextureSize = MaxTextureSize;
                importer.SaveAndReimport();

                TrimToContent(importer, assetPath);
            }

            foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath))
                if (asset is Sprite sprite) result[sprite.name] = sprite;
        }

        return result;
    }

    // 스프라이트 영역(원본 해상도 좌표)을 그림이 있는 부분의 정사각형으로 맞춘다.
    // 스프라이트 이름은 파일명과 같게 두어 StatusDefinition 이름과 매칭되게 한다.
    private static void TrimToContent(TextureImporter importer, string assetPath)
    {
        // 임포트된 텍스처는 256으로 줄어 있으므로, 원본 파일을 직접 읽어 원본 좌표로 계산한다.
        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!source.LoadImage(System.IO.File.ReadAllBytes(assetPath)))
        {
            Object.DestroyImmediate(source);
            return;
        }

        RectInt content = FindOpaqueBounds(source);
        int width = source.width, height = source.height;
        Object.DestroyImmediate(source);
        if (content.width <= 0 || content.height <= 0) return;

        int side = Mathf.CeilToInt(Mathf.Max(content.width, content.height) * (1f + TrimPadding * 2f));
        side = Mathf.Min(side, Mathf.Min(width, height));
        int x = Mathf.Clamp(Mathf.RoundToInt(content.center.x - side * 0.5f), 0, width - side);
        int y = Mathf.Clamp(Mathf.RoundToInt(content.center.y - side * 0.5f), 0, height - side);

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        if (provider == null) return;
        provider.InitSpriteEditorDataProvider();

        var editCapability = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (editCapability == null ||
            !editCapability.GetEditCapability().HasCapability(EEditCapability.EditSpriteRect) ||
            !editCapability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite))
        {
            Debug.LogWarning($"[StatusIconAssigner] {assetPath}: 임포터가 스프라이트 영역 편집을 지원하지 않아 트림을 건너뜀.");
            return;
        }

        var rects = provider.GetSpriteRects();
        var rect = rects.Length > 0 ? rects[0] : new SpriteRect { spriteID = GUID.Generate() };
        rect.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        rect.rect = new Rect(x, y, side, side);
        rect.alignment = SpriteAlignment.Center;
        rect.pivot = new Vector2(0.5f, 0.5f);
        provider.SetSpriteRects(new[] { rect }); // 한 장만 남긴다

        // 이름 ↔ 파일 ID 매핑도 맞춰야 이후 재임포트에서 참조가 유지된다
        var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameIds?.SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(rect.name, rect.spriteID) });

        provider.Apply();
        importer.SaveAndReimport();
    }

    // 알파가 임계값을 넘는 픽셀의 경계 상자 (텍스처 좌표: 왼쪽 아래 원점 — 스프라이트 rect와 같은 기준)
    private static RectInt FindOpaqueBounds(Texture2D texture)
    {
        Color32[] pixels = texture.GetPixels32();
        int w = texture.width, h = texture.height;
        int minX = w, minY = h, maxX = -1, maxY = -1;
        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
            {
                if (pixels[row + x].a <= AlphaThreshold) continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }
        return maxX < 0 ? new RectInt() : new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}
