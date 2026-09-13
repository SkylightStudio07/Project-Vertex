using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed partial class CardAuthoringWindow
{
    [SerializeField] private bool ownsDraft;
    [SerializeField] private bool automaticSave = true;
    [SerializeField] private string chosenFolder = ""; // Empty: derive from rarity + card type.
    [SerializeField] private string manualFolder = "Assets";
    [SerializeField] private PlayerRewardPoolSO rewardPool;
    [SerializeField] private int workspacePage;
    [SerializeField] private string catalogSearch = "";
    [SerializeField] private string catalogFolder = "";
    [SerializeField] private int membershipFilter;
    [SerializeField] private Vector2 catalogScroll;
    private bool catalogDirty = true;
    private List<string> playerFolders = new();
    private List<CardData> catalogCards = new();

    private void InitializeCatalog()
    {
        if (rewardPool == null)
            rewardPool = AssetDatabase.LoadAssetAtPath<PlayerRewardPoolSO>(CardAuthoringCatalog.DefaultPoolPath);
        catalogDirty = true;
    }

    private void RefreshCatalogIfNeeded()
    {
        if (!catalogDirty || Event.current.type != EventType.Layout) return;
        playerFolders = CardAuthoringCatalog.PlayerFolders();
        catalogCards = CardAuthoringCatalog.AllPlayerCards();
        catalogDirty = false;
    }

    private void ChooseInitialFolder()
    {
        string path = card != null ? AssetDatabase.GetAssetPath(card) : "";
        string folder = CardAuthoringCatalog.AssetFolder(path);
        chosenFolder = string.IsNullOrEmpty(path) || CardAuthoringCatalog.IsConventionalCardFolder(folder) ? "" : folder;
        if (!string.IsNullOrEmpty(folder)) manualFolder = folder;
    }

    private void CreateCard(bool duplicate)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        serializedCard?.ApplyModifiedProperties();
        // Capture the source before the user optionally saves/discards the current draft.
        var created = duplicate && card != null ? Instantiate(card) : null;
        string sourceFolder = chosenFolder;
        if (!TryLeaveDraft()) { if (created != null) DestroyImmediate(created); return; }
        if (created == null)
        {
            created = CreateInstance<CardData>();
            CardAuthoringUtility.Initialize(created);
        }
        else
        {
            using var so = new SerializedObject(created);
            string name = so.FindProperty("normalState.cardName").stringValue + " 복사본";
            so.FindProperty("normalState.cardName").stringValue = name;
            var upgradedName = so.FindProperty("upgradedState.cardName");
            if (!string.IsNullOrWhiteSpace(upgradedName.stringValue)) upgradedName.stringValue = name + "+";
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        created.hideFlags = HideFlags.HideInHierarchy;
        created.name = "미저장 카드";
        BindCard(created, true);
        if (duplicate) chosenFolder = sourceFolder;
        page = 0;
        workspacePage = 0;
    }

    private bool TryLeaveDraft()
    {
        if (!ownsDraft || card == null) return true;
        int choice = EditorUtility.DisplayDialogComplex("미저장 카드", "작성 중인 카드를 SO로 저장한 뒤 이동할까요?", "저장", "취소", "초안 버리기");
        if (choice == 1) return false;
        if (choice == 0) return SaveCard();
        ReleaseDraft();
        return true;
    }

    private void ReleaseDraft()
    {
        if (!ownsDraft) return;
        serializedCard?.Dispose();
        serializedCard = null;
        if (card != null && !EditorUtility.IsPersistent(card))
        {
            Undo.ClearUndo(card);
            DestroyImmediate(card);
        }
        card = null;
        ownsDraft = false;
        hasUnsavedChanges = false;
    }

    private void OnDestroy() => ReleaseDraft();

    public override void SaveChanges()
    {
        // Unity keeps the window open when saving was canceled and hasUnsavedChanges stays true.
        if (SaveCard()) base.SaveChanges();
    }

    public override void DiscardChanges()
    {
        ReleaseDraft();
        base.DiscardChanges();
    }

    private bool SaveCard()
    {
        if (card == null || EditorApplication.isPlayingOrWillChangePlaymode) return false;
        serializedCard?.ApplyModifiedProperties();
        string name = CardAuthoringCatalog.NormalName(card);
        if (!CardAuthoringCatalog.ValidFileName(name, out string error))
        {
            EditorUtility.DisplayDialog("저장할 수 없음", error, "확인");
            return false;
        }
        string folder = null;
        if (!automaticSave || !CardAuthoringCatalog.TryAutomaticFolder(card, chosenFolder, out folder))
        {
            string absolute = EditorUtility.OpenFolderPanel("카드 SO를 저장할 폴더 선택 (Assets 내부)",
                Path.GetFullPath(AssetDatabase.IsValidFolder(manualFolder) ? manualFolder : "Assets"), "");
            if (string.IsNullOrEmpty(absolute)) return false;
            folder = CardAuthoringCatalog.ProjectFolder(absolute);
            if (folder == null || !AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog("저장 폴더 확인", "현재 Unity 프로젝트의 Assets 내부 폴더를 선택하세요.", "확인");
                return false;
            }
            manualFolder = folder;
        }
        string destination = folder + "/" + name + ".asset";
        string current = AssetDatabase.GetAssetPath(card);
        string existingGuid = AssetDatabase.AssetPathToGUID(destination);
        bool sameAsset = !string.IsNullOrEmpty(current) &&
            string.Equals(AssetDatabase.AssetPathToGUID(current), existingGuid, StringComparison.Ordinal);
        if ((!string.IsNullOrEmpty(existingGuid) || File.Exists(destination)) && !sameAsset)
        {
            EditorUtility.DisplayDialog("같은 파일명이 있습니다", destination + "\n기존 SO는 덮어쓰지 않습니다. 카드 이름이나 저장 폴더를 변경하세요.", "확인");
            return false;
        }
        if (!string.IsNullOrEmpty(current) && !current.StartsWith("Assets/", StringComparison.Ordinal))
        {
            EditorUtility.DisplayDialog("저장할 수 없음", "Assets 외부 에셋입니다. 복제로 새 초안을 만든 뒤 저장하세요.", "확인");
            return false;
        }
        try
        {
            if (string.IsNullOrEmpty(current))
            {
                var flags = card.hideFlags;
                card.hideFlags = HideFlags.None;
                try { AssetDatabase.CreateAsset(card, destination); }
                catch { card.hideFlags = flags; throw; }
                if (!EditorUtility.IsPersistent(card)) throw new InvalidOperationException("카드 에셋 생성에 실패했습니다.");
                ownsDraft = false;
            }
            else if (!string.Equals(current, destination, StringComparison.Ordinal))
            {
                error = AssetDatabase.MoveAsset(current, destination);
                if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
            }
            card.name = name;
            EditorUtility.SetDirty(card);
            AssetDatabase.SaveAssetIfDirty(card);
            hasUnsavedChanges = false;
            manualFolder = folder;
            // Keep explicitly selected / nonstandard folders on subsequent saves.
            if (!string.IsNullOrEmpty(chosenFolder) || !CardAuthoringCatalog.IsDefaultFolder(folder)) chosenFolder = folder;
            serializedCard?.Update();
            catalogDirty = true;
            EditorGUIUtility.PingObject(card);
            ShowNotification(new GUIContent("카드 저장 완료"));
            return true;
        }
        catch (Exception exception)
        {
            EditorUtility.DisplayDialog("카드 저장 실패", exception.Message, "확인");
            return false;
        }
    }

    private void DrawSaveSettings()
    {
        RefreshCatalogIfNeeded();
        using (new EditorGUILayout.HorizontalScope())
        {
            automaticSave = GUILayout.Toggle(automaticSave, new GUIContent("자동 저장 경로", "켜면 저장 버튼으로 정해진 폴더에 저장합니다. 주기적으로 저장하는 기능은 아닙니다."), GUILayout.Width(122));
            var paths = new List<string> { "" };
            paths.AddRange(playerFolders);
            if (!string.IsNullOrEmpty(chosenFolder) && !paths.Contains(chosenFolder)) paths.Add(chosenFolder);
            var labels = new string[paths.Count];
            labels[0] = "기본: 희귀도 / 카드 종류";
            for (int i = 1; i < paths.Count; i++) labels[i] = CardAuthoringCatalog.LocationLabel(paths[i]);
            int index = Math.Max(0, paths.IndexOf(chosenFolder ?? ""));
            using (new EditorGUI.DisabledScope(!automaticSave))
                chosenFolder = paths[EditorGUILayout.Popup(index, labels)];
        }
        // Use the pending serialized values so the destination follows the current Inspector fields.
        serializedCard.ApplyModifiedProperties();
        if (automaticSave && CardAuthoringCatalog.TryAutomaticFolder(card, chosenFolder, out string folder))
            EditorGUILayout.LabelField("저장 예정: " + folder + "/" + CardAuthoringCatalog.NormalName(card) + ".asset", EditorStyles.miniLabel);
        else
            EditorGUILayout.LabelField("저장 시 폴더를 직접 선택합니다. 자동 경로를 찾지 못한 경우에도 동일합니다.", EditorStyles.miniLabel);
    }

    private void DrawPoolSelector()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            rewardPool = (PlayerRewardPoolSO)EditorGUILayout.ObjectField("보상 풀", rewardPool, typeof(PlayerRewardPoolSO), false);
            if (GUILayout.Button("기본 풀", GUILayout.Width(64)))
                rewardPool = AssetDatabase.LoadAssetAtPath<PlayerRewardPoolSO>(CardAuthoringCatalog.DefaultPoolPath);
        }
        if (rewardPool == null) EditorGUILayout.HelpBox("보상 풀이 없습니다. 기본 경로를 확인하거나 풀 에셋을 지정하세요.", MessageType.Warning);
    }

    private void DrawCurrentPool()
    {
        DrawPoolSelector();
        var membership = CardAuthoringCatalog.GetMembership(rewardPool, card);
        bool saved = card != null && EditorUtility.IsPersistent(card);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(!saved ? "SO로 저장한 뒤 보상 풀에 등록할 수 있습니다." : rewardPool == null ? "등록 여부 확인 불가" : membership.Label);
            DrawPoolAction(card);
        }
        if (membership.NeedsAttention)
            EditorGUILayout.HelpBox("카드 희귀도와 등록 목록이 다르거나 중복 등록되어 있습니다. 풀 에셋에서 확인하세요.", MessageType.Warning);
    }

    private bool CanAddToPool(CardData target) => !EditorApplication.isPlayingOrWillChangePlaymode &&
        target != null && EditorUtility.IsPersistent(target) && rewardPool != null && EditorUtility.IsPersistent(rewardPool) &&
        AssetDatabase.GetAssetPath(rewardPool).StartsWith("Assets/", StringComparison.Ordinal) &&
        CardAuthoringCatalog.PoolField(target.Rarity) != null && CardAuthoringCatalog.GetMembership(rewardPool, target).Total == 0;

    private void RegisterCard(CardData target)
    {
        serializedCard?.ApplyModifiedProperties();
        if (!CanAddToPool(target)) return;
        if (CardAuthoringCatalog.AddToPool(rewardPool, target)) ShowNotification(new GUIContent("보상 풀에 등록했습니다."));
    }

    private void DrawPoolAction(CardData target)
    {
        bool registered = CardAuthoringCatalog.GetMembership(rewardPool, target).Total > 0;
        bool canRemove = !EditorApplication.isPlayingOrWillChangePlaymode &&
            target != null && EditorUtility.IsPersistent(target) && rewardPool != null && EditorUtility.IsPersistent(rewardPool) &&
            AssetDatabase.GetAssetPath(rewardPool).StartsWith("Assets/", StringComparison.Ordinal);
        using (new EditorGUI.DisabledScope(registered ? !canRemove : !CanAddToPool(target)))
        {
            if (!GUILayout.Button(registered ? "풀에서 제거" : "풀에 추가", GUILayout.Width(90))) return;
            if (registered)
            {
                if (CardAuthoringCatalog.RemoveFromPool(rewardPool, target))
                    ShowNotification(new GUIContent("보상 풀에서 제거했습니다."));
            }
            else RegisterCard(target);
            Repaint();
            GUIUtility.ExitGUI();
        }
    }

    private void DrawCatalog()
    {
        RefreshCatalogIfNeeded();
        DrawPoolSelector();
        EditorGUILayout.LabelField(CardAuthoringCatalog.PlayerRoot + " · 하위 폴더 전체", EditorStyles.miniLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            catalogSearch = EditorGUILayout.TextField(catalogSearch ?? "", EditorStyles.toolbarSearchField);
            membershipFilter = EditorGUILayout.Popup(membershipFilter, new[] { "전체 등록 상태", "미등록", "등록됨", "등록 확인 필요" }, GUILayout.Width(130));
            if (GUILayout.Button("새로고침", GUILayout.Width(76))) { catalogDirty = true; Repaint(); }
        }
        var folderPaths = new List<string> { "" };
        folderPaths.AddRange(playerFolders);
        var folderLabels = new string[folderPaths.Count];
        folderLabels[0] = "모든 폴더";
        for (int i = 1; i < folderPaths.Count; i++) folderLabels[i] = CardAuthoringCatalog.LocationLabel(folderPaths[i]);
        catalogFolder = folderPaths[EditorGUILayout.Popup("폴더", Math.Max(0, folderPaths.IndexOf(catalogFolder ?? "")), folderLabels)];
        int registered = 0;
        foreach (var entry in catalogCards)
            if (entry != null && CardAuthoringCatalog.GetMembership(rewardPool, entry).Total > 0) registered++;
        EditorGUILayout.LabelField(rewardPool == null ? $"전체 {catalogCards.Count}장 · 풀 미지정" :
            $"전체 {catalogCards.Count}장 · 등록 {registered}장 · 미등록 {catalogCards.Count - registered}장", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("기본·테스트 등 다른 폴더도 모두 포함됩니다. 등록은 실제 카드 희귀도를 사용합니다.", EditorStyles.miniLabel);
        using (var view = new EditorGUILayout.ScrollViewScope(catalogScroll))
        {
            catalogScroll = view.scrollPosition;
            int visible = 0;
            foreach (var entry in catalogCards)
            {
                if (entry == null) continue;
                string path = AssetDatabase.GetAssetPath(entry);
                if (!string.IsNullOrEmpty(catalogFolder) && !path.StartsWith(catalogFolder + "/", StringComparison.Ordinal)) continue;
                string name = CardAuthoringCatalog.NormalName(entry);
                if (string.IsNullOrWhiteSpace(name)) name = entry.name;
                string term = (catalogSearch ?? "").Trim();
                if (term.Length > 0 && name.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0 && path.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0) continue;
                var member = CardAuthoringCatalog.GetMembership(rewardPool, entry);
                if (membershipFilter != 0 && rewardPool == null) continue;
                if (membershipFilter == 1 && member.Total > 0 || membershipFilter == 2 && member.Total == 0 || membershipFilter == 3 && !member.NeedsAttention) continue;
                visible++;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(name, EditorStyles.boldLabel, GUILayout.MinWidth(140));
                        GUILayout.Label(CardAuthoringCatalog.RarityFolder(entry.Rarity) ?? entry.Rarity.ToString(), GUILayout.Width(40));
                        GUILayout.Label(rewardPool == null ? "풀 미지정" : member.Label, GUILayout.MinWidth(155));
                        if (GUILayout.Button("열기", GUILayout.Width(48)))
                        {
                            if (SelectCard(entry)) { workspacePage = 0; page = 0; }
                            GUIUtility.ExitGUI();
                        }
                        DrawPoolAction(entry);
                    }
                    EditorGUILayout.LabelField(CardAuthoringCatalog.LocationLabel(path), EditorStyles.miniLabel);
                }
            }
            if (visible == 0) EditorGUILayout.HelpBox("조건에 맞는 카드가 없습니다.", MessageType.None);
        }
        EditorGUILayout.HelpBox("보상 풀 등록은 SO에 저장됩니다. 진행 중인 런의 카드풀은 런 시작 때 복사되므로 새 런부터 반영됩니다.", MessageType.None);
    }
}
