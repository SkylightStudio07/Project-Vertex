using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 방어도 획득 이펙트(방패 팝업 스프라이트 + 오라 파티클) 프리팹을 자동 구성하고
// PartyView(파티원 전원 스폰 담당)에 배선한다. 사운드는 PlayerHUDView가 별도로 재생하므로
// 여기서는 건드리지 않는다.
//
// 파티클(BlockGainAura)의 Shape/Emission/Size/Color 값은 "합리적인 시작점"일 뿐이다.
// 실시간 프리뷰를 보며 튜닝하는 건 이 스크립트가 아니라 에디터에서 사람이 할 일 —
// Inspector에서 프리팹을 열고 파티클 프리뷰(재생 버튼)로 보면서 조정하면 된다.
// 어느 모듈을 만지면 되는지는 이 스크립트의 각 주석에 표시해 뒀다.
public static class BlockGainVfxSetup
{
    private const string ShieldSpritePath = "Assets/Art/VFX/BlockGainShield.png";
    private const string ShieldPrefabPath = "Assets/Data/VFX/BlockGainShield.prefab";
    private const string AuraPrefabPath   = "Assets/Data/VFX/BlockGainAura.prefab";
    private const string EnemyPrefabPath  = "Assets/Data/Enemy/Prefabs/EnemyView.prefab";

    // 플레이어 캐릭터가 하나뿐이라 배선을 다시 할 일이 사실상 없어서 툴바 메뉴엔 안 올린다.
    // (프리팹 자체는 재실행해도 안 지워짐 — CreateOrUpdate*Prefab의 existing-reuse 가드 참고.)
    // 필요하면 Unity CLI로 직접 호출: unity command eval 'BlockGainVfxSetup.Setup();'
    public static void Setup()
    {
        EnsureSpriteImported(ShieldSpritePath);

        var shieldPrefab = CreateOrUpdateShieldPrefab();
        var auraPrefab   = CreateOrUpdateAuraPrefab();

        string partyResult = WirePartyView(shieldPrefab, auraPrefab);
        string enemyResult = WireEnemyView(shieldPrefab, auraPrefab);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"[BlockGainVfxSetup] 방패 프리팹: {ShieldPrefabPath}\n" +
            $"오라 프리팹: {AuraPrefabPath}\n" +
            $"{partyResult}\n{enemyResult}");
    }

    // ---------- 스프라이트 임포트 ----------

    private static void EnsureSpriteImported(string path)
    {
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }

        if (changed) importer.SaveAndReimport();
    }

    // ---------- 방패 팝업 프리팹 ----------

    private static GameObject CreateOrUpdateShieldPrefab()
    {
        // 이미 있으면 그대로 재사용 — 재실행 시 프리팹을 처음부터 다시 만들면
        // 나중에 여기 손으로 튜닝해둔 값(있다면)이 통째로 날아간다.
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ShieldPrefabPath);
        if (existing != null) return existing;

        EnsureFolder(Path.GetDirectoryName(ShieldPrefabPath));

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ShieldSpritePath);

        var root = new GameObject("BlockGainShield", typeof(SpriteRenderer), typeof(ShieldPopEffect));
        var sr = root.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        // 알파 블렌드 기본 스프라이트 셰이더 — 이미 아트 자체에 글로우가 그려져 있어서
        // 추가 셰이더/틴트 없이도 파란 빛 번짐이 보인다.
        sr.sortingOrder = 100; // 캐릭터/이펙트보다 앞에 그려지도록

        var saved = PrefabUtility.SaveAsPrefabAsset(root, ShieldPrefabPath);
        Object.DestroyImmediate(root);
        return saved;
    }

    // ---------- 오라 파티클 프리팹 ----------

    private static GameObject CreateOrUpdateAuraPrefab()
    {
        // 이미 있으면 그대로 재사용 — 재실행 시 프리팹을 처음부터 다시 만들면
        // 에디터에서 손으로 튜닝해둔 Shape/Emission/Color 값이 통째로 날아간다.
        // 파티클 값을 스크립트 기본값으로 되돌리고 싶으면 프리팹을 직접 지우고 재실행할 것.
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(AuraPrefabPath);
        if (existing != null) return existing;

        EnsureFolder(Path.GetDirectoryName(AuraPrefabPath));

        var root = new GameObject("BlockGainAura", typeof(ParticleSystem));
        var ps = root.GetComponent<ParticleSystem>();
        var renderer = root.GetComponent<ParticleSystemRenderer>();

        // Main — 재생 1회, 짧게. 실제 게임 내 스케일(카메라 앞으로 당겨 스폰되는 거리 기준)에서
        // 너무 작거나 크지 않도록 잡은 시작값. Setup() 실행 후 스크린샷으로 크기를 1차 확인했다.
        var main = ps.main;
        main.duration = 0.8f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
        // 방패 스프라이트(월드 기준 지름 약 2.7유닛, 100 PPU)에 견줘 눈에 띄게 잡은 크기.
        // 처음엔 0.15~0.3으로 잡았다가 실제 스폰해서 캡처해보니 방패 대비 너무 작아서 안 보였다 —
        // 숫자만으로는 스케일 감이 안 잡혀서 실측 후 키운 값.
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        main.startColor = new Color(0.45f, 0.7f, 1f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World; // 앵커가 움직여도 이미 퍼진 입자는 안 끌려가게
        main.maxParticles = 60;

        // Emission — 지속 방출 없이 한 번에 터뜨린다(버스트).
        // 개수를 늘리면 더 풍성해지고, 줄이면 더 산뜻해진다 — 여기서 눈으로 맞추면 됨.
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24, 30, 1, 0f) });

        // Shape — 캐릭터를 중심으로 한 원형 테두리에서 바깥으로 튀어나가는 형태.
        // radius를 키우면 더 넓게 퍼지고, arc/각도로 방향성(예: 위쪽 절반만)을 줄 수도 있다.
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        // 방패 스프라이트의 절반 크기(약 1.35유닛)보다 살짝 크게 잡아서, 방패 실루엣 밖에서부터
        // 시작해 바깥으로 퍼지게 한다 — 안쪽에서 시작하면 방패에 가려 안 보인다.
        shape.radius = 1.1f;
        shape.radiusThickness = 0.3f; // 0=테두리에서만 발생, 1=원 전체에서 발생

        // Color over Lifetime — 끝에서 알파 0으로 완전히 사라지게. 색 자체를 바꾸고 싶으면
        // 그라디언트 컬러 키를 추가하면 된다(예: 밝은 하늘색 → 진한 파랑).
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.6f, 0.8f, 1f), 0f), new GradientColorKey(new Color(0.3f, 0.6f, 1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        // Size over Lifetime — 살짝 커지다 사그라드는 느낌.
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.7f, 1f, 1.3f));

        // Renderer — 항상 카메라를 보도록 Billboard(기존 Impact 이펙트와 동일 컨벤션).
        // 텍스처는 Unity 기본 파티클 텍스처(부드러운 원형 스프라이트)를 사용하고,
        // Additive 셰이더로 겹칠수록 밝아지는 "빛" 느낌을 낸다.
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = GetOrCreateAdditiveMaterial();
        // BlockGainShield(sortingOrder=100)보다 위에 그려지도록 — 같은 sortingOrder면
        // 파티클이 방패 스프라이트 뒤에 완전히 가려져 안 보이는 문제가 실측 중 발견됐다.
        renderer.sortingOrder = 101;

        var saved = PrefabUtility.SaveAsPrefabAsset(root, AuraPrefabPath);
        Object.DestroyImmediate(root);
        return saved;
    }

    private const string AdditiveMaterialPath = "Assets/Data/VFX/BlockGainAuraParticle.mat";

    private static Material GetOrCreateAdditiveMaterial()
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(AdditiveMaterialPath);
        if (existing != null) return existing;

        var shader = Shader.Find("Legacy Shaders/Particles/Additive");
        var mat = new Material(shader);
        mat.SetTexture("_MainTex", AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd"));

        EnsureFolder(Path.GetDirectoryName(AdditiveMaterialPath));
        AssetDatabase.CreateAsset(mat, AdditiveMaterialPath);
        return mat;
    }

    // ---------- PartyView 배선 ----------

    private static string WirePartyView(GameObject shieldPrefab, GameObject auraPrefab)
    {
        var party = Object.FindFirstObjectByType<PartyView>(FindObjectsInactive.Include);
        if (party == null) return "PartyView를 씬에서 찾지 못함.";

        var so = new SerializedObject(party);
        so.FindProperty("blockGainShieldPrefab").objectReferenceValue = shieldPrefab;
        so.FindProperty("blockGainParticlePrefab").objectReferenceValue = auraPrefab;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(party);

        // 앵커는 PartyView가 playerImage/companionSlots에서 매번 직접 찾으므로
        // (파티원이 성소마다 바뀔 수 있어 고정 참조를 두지 않는다) 여기서 따로 연결할 게 없다.
        return $"'{party.name}'에 blockGainShieldPrefab/blockGainParticlePrefab 연결 완료 " +
               "(앵커는 PartyView가 런타임에 playerImage/companionSlots에서 직접 찾음).";
    }

    // ---------- EnemyView 배선 ----------
    // "적이 방어도를 올릴 때도 동일한 이펙트가 나와야 한다"는 요구사항 반영.
    // 적 프리팹은 전투에 등장하는 모든 적이 공유하는 단일 EnemyView.prefab이라
    // 프리팹 하나만 배선하면 모든 적에게 적용된다.
    private static string WireEnemyView(GameObject shieldPrefab, GameObject auraPrefab)
    {
        var enemyPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        if (enemyPrefabAsset == null) return $"적 프리팹을 찾지 못함: {EnemyPrefabPath}";

        var root = PrefabUtility.LoadPrefabContents(EnemyPrefabPath);
        try
        {
            var view = root.GetComponent<EnemyView>();
            if (view == null) return "EnemyView 컴포넌트가 프리팹 루트에 없음.";

            var so = new SerializedObject(view);
            so.FindProperty("blockGainShieldPrefab").objectReferenceValue = shieldPrefab;
            so.FindProperty("blockGainParticlePrefab").objectReferenceValue = auraPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            return $"'{enemyPrefabAsset.name}' 프리팹에 blockGainShieldPrefab/blockGainParticlePrefab 연결 완료.";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (Directory.Exists(path)) return;
        Directory.CreateDirectory(path);
        AssetDatabase.Refresh();
    }
}
