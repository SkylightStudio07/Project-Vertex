using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// filename   : BlessingViewEditorSetup.cs
// description: Canvas 아래의 BlessingView 오브젝트를 찾아서
//              BlessingView 및 PetalFloatingEffect 컴포넌트를 붙이고
//              Petals.png의 4종 꽃잎 스프라이트와 배경을 자동 연결하는 에디터 툴
// ============================================================
public static class BlessingViewEditorSetup
{
    [MenuItem("Tools/Blessing/Setup BlessingView in Scene")]
    public static void SetupBlessingView()
    {
        // 씬에서 BlessingView 오브젝트 탐색
        GameObject blessingViewObj = GameObject.Find("BlessingView");
        if (blessingViewObj == null)
        {
            // Canvas 아래에서 검색
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                Transform t = canvas.transform.Find("BlessingView");
                if (t != null)
                {
                    blessingViewObj = t.gameObject;
                    break;
                }
            }
        }

        if (blessingViewObj == null)
        {
            Debug.LogWarning("[BlessingViewSetup] 씬의 Canvas 아래에서 'BlessingView' 오브젝트를 찾을 수 없습니다.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(blessingViewObj, "Setup BlessingView");

        // 1. PetalFloatingEffect 컴포넌트 추가/가져오기
        PetalFloatingEffect petalEffect = blessingViewObj.GetComponent<PetalFloatingEffect>();
        if (petalEffect == null)
        {
            petalEffect = Undo.AddComponent<PetalFloatingEffect>(blessingViewObj);
        }

        // 2. BlessingView 컴포넌트 추가/가져오기
        BlessingView blessingView = blessingViewObj.GetComponent<BlessingView>();
        if (blessingView == null)
        {
            blessingView = Undo.AddComponent<BlessingView>(blessingViewObj);
        }

        // 3. Petals.png에서 4종 스프라이트 로드 후 SerializedObject로 할당
        string petalPath = "Assets/Art/Blessing/Machina/Effects/Petals.png";
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(petalPath);
        List<Sprite> petalSprites = new List<Sprite>();
        foreach (var sub in subAssets)
        {
            if (sub is Sprite s)
            {
                petalSprites.Add(s);
            }
        }

        if (petalSprites.Count > 0)
        {
            SerializedObject soEffect = new SerializedObject(petalEffect);
            SerializedProperty propSprites = soEffect.FindProperty("petalSprites");
            propSprites.ClearArray();
            for (int i = 0; i < petalSprites.Count; i++)
            {
                propSprites.InsertArrayElementAtIndex(i);
                propSprites.GetArrayElementAtIndex(i).objectReferenceValue = petalSprites[i];
            }
            soEffect.ApplyModifiedProperties();
            Debug.Log($"[BlessingViewSetup] PetalFloatingEffect에 {petalSprites.Count}종의 꽃잎 스프라이트 연결 완료!");
        }

        // 4. Background 자식 오브젝트 확인 및 스프라이트 연결
        Transform bgTransform = blessingViewObj.transform.Find("Background");
        if (bgTransform != null)
        {
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null && bgImage.sprite == null)
            {
                string bgPath = "Assets/Art/Blessing/Machina/Backgrounds.png";
                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
                if (bgSprite != null)
                {
                    Undo.RecordObject(bgImage, "Assign Background Sprite");
                    bgImage.sprite = bgSprite;
                    Debug.Log("[BlessingViewSetup] Background에 Backgrounds.png 스프라이트 연결 완료!");
                }
            }
        }

        // 5. BlessingView 필드 연결
        SerializedObject soView = new SerializedObject(blessingView);
        SerializedProperty propEffect = soView.FindProperty("petalEffect");
        if (propEffect != null) propEffect.objectReferenceValue = petalEffect;

        if (bgTransform != null)
        {
            SerializedProperty propBg = soView.FindProperty("backgroundImage");
            if (propBg != null) propBg.objectReferenceValue = bgTransform.GetComponent<Image>();
        }

        MapUIController mapUI = Object.FindFirstObjectByType<MapUIController>();
        if (mapUI != null)
        {
            SerializedProperty propMap = soView.FindProperty("mapUIController");
            if (propMap != null) propMap.objectReferenceValue = mapUI;
        }

        soView.ApplyModifiedProperties();
        EditorUtility.SetDirty(blessingViewObj);

        Debug.Log("<color=#38BDF8><b>[BlessingViewSetup] BlessingView 세팅이 성공적으로 완료되었습니다!</b></color>");
    }
}
