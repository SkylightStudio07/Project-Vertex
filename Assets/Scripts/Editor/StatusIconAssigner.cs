using System.Collections.Generic;
using System.Text;
using UnityEditor;
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
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    changed = true;
                }
                // 34px 칩에 들어가는 작은 아이콘이라 압축 아티팩트가 그대로 보인다. 무압축 유지.
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

                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null) result[sprite.name] = sprite;
        }

        return result;
    }
}
