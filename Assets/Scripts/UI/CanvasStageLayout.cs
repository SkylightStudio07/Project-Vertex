using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 1920×1080 기준으로 배치한 화면을 16:9가 아닌 해상도에 맞춘다.
// 캔버스는 Expand 모드라 1920×1080 무대가 항상 다 보이고, 남는 여백만큼 캔버스가 넓어진다.
// 무대 좌표로 놓인 UI는 가운데 무대를 따라가고, HUD는 표에 적힌 화면 가장자리에 붙으며, 배경은 여백까지 덮는다.
// 16:9에서는 아무것도 바꾸지 않는다.
public sealed class CanvasStageLayout : MonoBehaviour
{
    private const float DesignWidth = 1920f, DesignHeight = 1080f;

    private enum Pin { Start, Center, End } // x는 왼쪽·가운데·오른쪽, y는 아래·가운데·위

    // 이 경로와 그 아래 UI가 붙을 화면 가장자리. 적혀 있지 않으면 가운데 무대를 따라간다.
    private static readonly Dictionary<string, (Pin x, Pin y)> Pins = new()
    {
        { "TopBar_v3", (Pin.Center, Pin.End) },
        { "EssentialUI", (Pin.End, Pin.End) },
        { "EndTurnButton", (Pin.End, Pin.Start) },
        { "BattleUI/Resource_v3", (Pin.End, Pin.Start) },
        { "BattleUI/PlayerHUDView", (Pin.End, Pin.Start) },
        { "PlayerStatusBar", (Pin.Center, Pin.Start) },
        { "DialogUI", (Pin.Center, Pin.Start) },
        { "NpcDialogueOverlay", (Pin.Center, Pin.Start) },
    };

    // 캐릭터처럼 캔버스 비율로 직접 자리를 잡거나, 스스로 비율에 대응하는 UI.
    private static readonly HashSet<string> Skip = new()
    {
        "Background", "BattleAmbience", "BlessingView",
        "HandArea", "EnemyArea", "PartyViewer",
        "PlayerShadow", "PlayerDraft", "CoopDraft",
        "CoopTransform1", "CoopTransform2", "CoopTransform3",
        "CardObjAnimTransform", "CardObjAnimTransform2",
        "UnderBaseLayoutUI (1)", "UnderBaseLayoutUI (2)",
    };

    // 여백까지 덮어야 하는 배경.
    private static readonly string[] CoverPaths =
    {
        "Background",
        "BattleAmbience",
        "Nodes/RestView/Background",
        "Nodes/ShopView/ShopKit/Background",
        "CutsceneView/Image",
        "MainView/LobbyKit/Scene_WithCharacter",
        "ShopView/ShopKit/Background",
    };

    private readonly struct Anchored
    {
        public readonly RectTransform rect;
        public readonly Vector2 anchor;
        public readonly Pin x, y;

        public Anchored(RectTransform rect, Pin x, Pin y)
        {
            this.rect = rect;
            anchor = rect.anchorMin;
            this.x = x;
            this.y = y;
        }
    }

    private readonly struct Cover
    {
        public readonly RectTransform rect;
        public readonly Vector2 size, center;

        public Cover(RectTransform rect, Vector2 size, Vector2 center)
        {
            this.rect = rect;
            this.size = size;
            this.center = center;
        }
    }

    private readonly List<Anchored> _anchored = new();
    private readonly List<Cover> _covers = new();
    private Vector2Int _appliedScreen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook() => SceneManager.sceneLoaded += OnSceneLoaded;

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponent<CanvasStageLayout>() != null) continue;
            if (!root.TryGetComponent(out CanvasScaler scaler)) continue;
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) continue;
            if (scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.Expand) continue;
            root.AddComponent<CanvasStageLayout>();
        }
    }

    private void Awake()
    {
        Collect(transform, "", Pin.Center, Pin.Center);
        foreach (string path in CoverPaths)
        {
            RectTransform rect = FindTrimmed(path);
            if (rect != null) _covers.Add(new Cover(rect, DesignSize(rect), DesignCenter(rect)));
        }
        Apply();
    }

    private void Update()
    {
        if (_appliedScreen.x != Screen.width || _appliedScreen.y != Screen.height) Apply();
    }

    private void Collect(Transform parent, string parentPath, Pin inheritedX, Pin inheritedY)
    {
        foreach (Transform child in parent)
        {
            if (child is not RectTransform rect) continue;

            string path = parentPath.Length == 0 ? child.name.Trim() : parentPath + "/" + child.name.Trim();
            if (Skip.Contains(path)) continue;

            Pin x = inheritedX, y = inheritedY;
            if (Pins.TryGetValue(path, out (Pin x, Pin y) pin)) (x, y) = pin;

            if (rect.anchorMin == rect.anchorMax)
                _anchored.Add(new Anchored(rect, x, y));
            else if (rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one)
                Collect(child, path, x, y);
        }
    }

    private void Apply()
    {
        _appliedScreen = new Vector2Int(Screen.width, Screen.height);
        if (Screen.width <= 0 || Screen.height <= 0) return;

        float scale = Mathf.Min(Screen.width / DesignWidth, Screen.height / DesignHeight);
        float width = Screen.width / scale, height = Screen.height / scale;

        foreach (Anchored item in _anchored)
        {
            Vector2 anchor = new(Map(item.anchor.x, DesignWidth / width, item.x), Map(item.anchor.y, DesignHeight / height, item.y));
            item.rect.anchorMin = anchor;
            item.rect.anchorMax = anchor;
        }

        float cover = Mathf.Max(width / DesignWidth, height / DesignHeight);
        foreach (Cover item in _covers)
        {
            item.rect.anchorMin = item.rect.anchorMax = new Vector2(0.5f, 0.5f);
            item.rect.pivot = new Vector2(0.5f, 0.5f);
            item.rect.sizeDelta = item.size * cover;
            item.rect.anchoredPosition = item.center * cover;
        }
    }

    // 무대 안의 비율 위치를 넓어진 캔버스의 비율 위치로 바꾼다.
    private static float Map(float anchor, float ratio, Pin pin) => pin switch
    {
        Pin.Start => anchor * ratio,
        Pin.End => 1f - (1f - anchor) * ratio,
        _ => 0.5f + (anchor - 0.5f) * ratio,
    };

    private RectTransform FindTrimmed(string path)
    {
        Transform current = transform;
        foreach (string segment in path.Split('/'))
        {
            Transform next = null;
            foreach (Transform child in current)
            {
                if (child.name.Trim() != segment) continue;
                next = child;
                break;
            }
            if (next == null) return null;
            current = next;
        }
        return current as RectTransform;
    }

    private Vector2 DesignSize(RectTransform rect)
    {
        if (rect == transform) return new Vector2(DesignWidth, DesignHeight);
        if (rect.anchorMin == rect.anchorMax) return rect.sizeDelta;
        return Vector2.Scale(DesignSize((RectTransform)rect.parent), rect.anchorMax - rect.anchorMin) + rect.sizeDelta;
    }

    // 부모 가운데를 원점으로 한 기준 해상도에서의 중심 위치.
    private Vector2 DesignCenter(RectTransform rect)
    {
        Vector2 parentSize = DesignSize((RectTransform)rect.parent);
        Vector2 size = DesignSize(rect);
        Vector2 anchor = (rect.anchorMin + rect.anchorMax) * 0.5f;
        return Vector2.Scale(anchor - new Vector2(0.5f, 0.5f), parentSize)
             + rect.anchoredPosition
             + Vector2.Scale(new Vector2(0.5f, 0.5f) - rect.pivot, size);
    }
}
