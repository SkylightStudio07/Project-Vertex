using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SanctuaryCandidatePrefabSetup
{
    private const string BackgroundPath =
        "Assets/Art/UI/Sanctuary/Sanctuary_Background_Clean_v4.png";
    private const string FramePath =
        "Assets/Art/UI/Sanctuary/Sanctuary_CandidateFrame_Clean_v4.png";
    private const string ActionButtonPath =
        "Assets/Art/UI/Sanctuary/Sanctuary_ActionButton_Clean_v4.png";
    private const string PrefabPath =
        "Assets/Prefabs/UI/Sanctuary/CompanionCandidateFrame.prefab";

    [MenuItem("Tools/Vertex/Setup Editorial Sanctuary Candidate Prefab")]
    public static string Apply()
    {
        if (EditorApplication.isPlaying) return "Edit Mode에서만 실행할 수 있습니다.";

        SelectCoopCharUI ui = UnityEngine.Object.FindAnyObjectByType<SelectCoopCharUI>(
            FindObjectsInactive.Include);
        if (ui == null) return "SelectCoopCharUI not found";

        ConfigureSprite(BackgroundPath, false);
        ConfigureSprite(FramePath, true);
        ConfigureSprite(ActionButtonPath, true);
        Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        Sprite frame = AssetDatabase.LoadAssetAtPath<Sprite>(FramePath);
        Sprite actionButton = AssetDatabase.LoadAssetAtPath<Sprite>(ActionButtonPath);
        if (background == null || frame == null || actionButton == null)
            return "Generated sanctuary sprites could not be loaded";

        SerializedObject uiSerialized = new SerializedObject(ui);
        uiSerialized.FindProperty("sanctuaryBackground").objectReferenceValue = background;
        uiSerialized.FindProperty("candidateFrameSprite").objectReferenceValue = frame;
        uiSerialized.FindProperty("candidateActionButtonSprite").objectReferenceValue = actionButton;
        uiSerialized.ApplyModifiedPropertiesWithoutUndo();

        foreach (SelectCoopCharBtn stray in UnityEngine.Object.FindObjectsByType<SelectCoopCharBtn>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (stray.gameObject.scene == ui.gameObject.scene &&
                !stray.transform.IsChildOf(ui.transform) &&
                (stray.name == "Character Choice 2" || stray.name == "Character Choice 3"))
                Undo.DestroyObjectImmediate(stray.gameObject);
        }

        List<SelectCoopCharBtn> existing = ui.GetComponentsInChildren<SelectCoopCharBtn>(true).ToList();
        if (existing.Count == 0) return "No candidate button exists to use as prefab source";
        SetButtonList(ui, existing);
        ApplyLayout(ui, 3);

        SelectCoopCharBtn first = existing[0];
        Transform parent = first.transform.parent;
        Button firstButton = first.GetComponent<Button>();
        if (firstButton != null && firstButton.onClick.GetPersistentEventCount() == 0)
            UnityEventTools.AddPersistentListener(firstButton.onClick, first.OnClickBtn);

        Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
        GameObject prefabAssetRoot = PrefabUtility.SaveAsPrefabAssetAndConnect(
            first.gameObject, PrefabPath, InteractionMode.AutomatedAction);
        if (prefabAssetRoot == null) return "Failed to create candidate frame prefab";

        SelectCoopCharBtn prefab = AssetDatabase.LoadAssetAtPath<SelectCoopCharBtn>(PrefabPath);
        var sceneButtons = new List<SelectCoopCharBtn> { first };
        for (int i = 1; i < 3; i++)
        {
            if (i < existing.Count && existing[i] != null)
                Undo.DestroyObjectImmediate(existing[i].gameObject);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, parent);
            Undo.RegisterCreatedObjectUndo(instance, "Create sanctuary candidate prefab instance");
            instance.name = $"Character Choice {i + 1}";
            instance.transform.SetSiblingIndex(sceneButtons[0].transform.GetSiblingIndex() + i);
            sceneButtons.Add(instance.GetComponent<SelectCoopCharBtn>());
        }

        for (int i = existing.Count - 1; i >= 3; i--)
            if (existing[i] != null) Undo.DestroyObjectImmediate(existing[i].gameObject);

        uiSerialized.Update();
        uiSerialized.FindProperty("candidateFramePrefab").objectReferenceValue = prefab;
        uiSerialized.ApplyModifiedPropertiesWithoutUndo();
        SetButtonList(ui, sceneButtons);
        ApplyLayout(ui, 3);

        foreach (Component component in ui.GetComponentsInChildren<Component>(true))
            if (component != null) EditorUtility.SetDirty(component);
        EditorUtility.SetDirty(ui);
        EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
        AssetDatabase.SaveAssets();
        return $"Created {PrefabPath} and wired 3 fixed sanctuary candidates";
    }

    private static void ConfigureSprite(string path, bool alpha)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException($"TextureImporter missing: {path}");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = alpha;
        importer.mipmapEnabled = false;
        importer.sRGBTexture = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static void SetButtonList(SelectCoopCharUI ui, List<SelectCoopCharBtn> buttons)
    {
        FieldInfo field = typeof(SelectCoopCharUI).GetField(
            "selectCoopCharBtns", BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(ui, buttons);
    }

    private static void ApplyLayout(SelectCoopCharUI ui, int count)
    {
        MethodInfo method = typeof(SelectCoopCharUI).GetMethod(
            "ApplySanctuaryLayout", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(ui, new object[] { count });
    }
}
