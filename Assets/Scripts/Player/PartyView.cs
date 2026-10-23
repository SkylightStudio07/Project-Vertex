using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 전투 화면에 플레이어 스프라이트와 합류한 협력자들을 보여준다.
// 기본은 정적 스프라이트, 플레이어가 공격 카드를 내면 PoseSequencePlayer로 키프레임 홀드 연출 재생
// (BattleManager.OnCardPlayed 구독) — 이건 아직 플레이어만 적용(협력자 쪽 공격 포즈는 없음).
// 방어도 획득 이펙트(HandleBlockGained)는 반대로 파티 전원에게 재생한다 — 협력자는 자기
// HP/방어도가 없는 장식 캐릭터라 "누구 방어도인가"를 따질 필요가 없고, 파티가 함께 막아내는
// 연출로 보여주는 쪽이 더 자연스럽기 때문.
// 협력자는 최대 3명 고정. companionSlots는 시각 요소 없는 빈 오브젝트(위치 마커)로,
// 인스펙터에서 원하는 좌표에 미리 배치해두면 그 자리에 companionPrefab을 실제로 생성해 채운다.
// 합류 인원이 3명 미만이면 남는 슬롯은 그대로 비워둔다.
// 파티 구성은 전투 시작 시점마다 다시 그린다 — 합류는 성소(휴식)에서만 일어나고
// 전투 중엔 바뀌지 않는다.
// 이 프로젝트는 씬을 갈아끼우지 않고 화면(맵/전투/성소 등)을 SetActive로 여닫는 구조인데,
// 전투 화면 자체는 처음부터 끝까지 계속 켜진 채로 맵/성소 패널이 위에 덮였다 걷히는 방식이라
// Start()는 물론 OnEnable()도 최초 1회만 실행되고 이후 전투 진입 때는 다시 안 불린다.
// EnemyZoneView/HandView가 BattleManager 이벤트 구독으로 이 문제를 푼 것과 동일하게,
// BattleManager.OnBattleStarted(StartBattle() 끝에서 발화)를 구독해 매 전투 진입을 감지한다.
public class PartyView : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private PlayerCharData playerCharData;
    [SerializeField] private Image playerImage;
    // playerImage와 같은 오브젝트에 붙는 PoseSequencePlayer. 공격 카드 사용 시 연출 재생용(없어도 정상 작동).
    [SerializeField] private PoseSequencePlayer playerPoseSequencer;
    [SerializeField] private UISpriteSheetAnimator playerSheetAnimator;

    [Header("합류 캐릭터 — 스프라이트를 생성해 넣을 프리팹과 위치 마커(빈 오브젝트, 최대 3명)")]
    [SerializeField] private GameObject companionPrefab; // 루트 밑에 "CharacterSprite"(Image) 자식 필요 — 발밑 그림자(Shadow)도 같이 딸려옴
    [SerializeField] private Transform[] companionSlots = new Transform[3];

    [Header("진형 — 인원에 따라 자리 (1: 가운데, 2: 위아래, 3: 삼각형, 4: 마름모)")]
    [Tooltip("끄면 예전처럼 씬에 놓인 고정 위치")]
    [SerializeField] private bool useFormation = true;
    [Tooltip("진형 중심 = 처음 주인공 발 위치 + 이 값 (1920×1080 화면 기준 px)")]
    [SerializeField] private Vector2 formationCenterOffset = new(-60f, 0f);
    [Tooltip("켜면 진형 중심의 세로(발 높이)를 적들이 서 있는 바닥선에 맞춘다. 적이 없으면 주인공 처음 자리")]
    [SerializeField] private bool matchEnemyGround = true;
    [Tooltip("적 바닥선에서 더 내리거나(−) 올릴(+) 값 (1920×1080 화면 기준 px)")]
    [SerializeField] private float enemyGroundOffsetY = 0f;
    [Tooltip("진형 반지름 (가로, 세로 · 1920×1080 화면 기준 px). 세로는 원근감으로 납작하게")]
    [SerializeField] private Vector2 formationSpread = new(170f, 72f);

    // 진형 자리 (반지름 단위). 0번 = 주인공, 적 쪽(+x)이 앞. 아래(-y)일수록 화면 앞에 그린다
    private static readonly Vector2[][] Formations =
    {
        new[] { Vector2.zero },
        new[] { new Vector2(0.2f, -0.55f), new Vector2(-0.2f, 0.55f) },
        new[] { new Vector2(0.85f, 0f), new Vector2(-0.6f, 1f), new Vector2(-0.6f, -1f) },
        new[] { new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(0f, -1f), new Vector2(-1f, 0f) },
    };
    private bool _hasHomeFeet;
    private Vector2 _homeFeet;

    [Header("방어도 획득 이펙트 — 파티 전원에게 재생")]
    // 협력자(CoopCharState)는 자기 HP/방어도가 없는 순수 장식 캐릭터라(호감도/카드 언락만 가짐),
    // 방어도는 여전히 PlayerCombatant 하나의 값이다. 그래도 "파티 전체가 함께 막아낸다"는
    // 연출로 보여주는 게 자연스러워서, 방어 카드를 낼 때 플레이어 + 합류 중인 협력자 전원의
    // 스프라이트 위치에 이 이펙트를 재생한다. 프리팹 자체는 PlayerHUDView가 쓰던 것과 동일 —
    // 사운드만 HUD 쪽에서 한 번 재생하고(파티원 수만큼 중복 재생 안 함), 시각 연출은 여기서.
    [SerializeField] private GameObject blockGainShieldPrefab;
    [SerializeField] private GameObject blockGainParticlePrefab;

    // PlayerCombatant는 전투마다 새 인스턴스라, 매 전투 시작(Refresh)마다 재구독해야 한다.
    // PartyView는 매 프레임 폴링하지 않는 완전 이벤트 기반 구조라, PlayerHUDView처럼 Update()에서
    // 비교하는 대신 Refresh()(OnBattleStarted 구독)에서 갈아탄다 — BattleManager.StartBattle()이
    // OnBattleStarted를 발화하기 전에 이미 새 PlayerCombatant를 State.Player에 대입해두므로 안전하다.
    private PlayerCombatant _subscribedPlayer;

    private readonly Dictionary<string, (PoseSequencePlayer sequencer, CoopCharData data)> _companionVisuals = new();

    private void Start()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted += Refresh;
            BattleManager.Instance.OnCardPlayed += OnCardPlayed;
            BattleManager.Instance.OnWeaponChanged += OnWeaponChanged;
            BattleManager.Instance.OnCompanionSkillUsed += OnCompanionSkillUsed;
        }

        // 이 오브젝트가 활성화되기 전에 이미 전투가 시작돼 있었을 경우(에디터에서 오브젝트를
        // 나중에 켜서 테스트하는 경우 등)를 대비해 최초 1회는 바로 그려본다.
        Refresh();
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= Refresh;
            BattleManager.Instance.OnCardPlayed -= OnCardPlayed;
            BattleManager.Instance.OnWeaponChanged -= OnWeaponChanged;
            BattleManager.Instance.OnCompanionSkillUsed -= OnCompanionSkillUsed;
        }

        if (_subscribedPlayer != null)
        {
            _subscribedPlayer.OnBlockGained -= HandleBlockGained;
            _subscribedPlayer.OnDamaged -= HandlePlayerDamaged;
        }
    }

    private void OnWeaponChanged(WeaponData weapon)
    {
        BindPlayer();
    }

    private void OnCompanionSkillUsed(CompanionSkillSlot slot)
    {
        if (slot?.Owner == null) return;
        PlayCompanionAttack(slot.Owner.charID);
    }

    private void PlayCompanionAttack(string charId)
    {
        if (string.IsNullOrEmpty(charId)) return;
        if (_companionVisuals.TryGetValue(charId, out var visual) && visual.sequencer != null && visual.data != null)
        {
            if (visual.data.attackFrames != null && visual.data.attackFrames.Length > 0)
            {
                visual.sequencer.Play(visual.data.attackFrames, visual.data.attackFrameRate);
            }
            else if (visual.data.attackSequence != null && visual.data.attackSequence.Length > 0)
            {
                visual.sequencer.Play(visual.data.attackSequence);
            }
        }
    }

    // OnCardPlayed는 공격 카드를 낼 때마다(전투 중 빈번히) 불리므로, 설정 미비 경고는
    // 매번 다시 찍지 않고 한 번만 띄운다 — 안 그러면 콘솔이 같은 경고로 도배돼서 다른 로그를 가린다.
    private bool warnedMissingPoseSequencer;
    private bool warnedMissingPlayerCharData;
    private bool warnedEmptyAttackSequence;

    // 공격 카드를 냈을 때 플레이어/협력자 스탠딩에 연출을 재생한다. 데미지 적용을 기다리지 않는
    // "구경용" 재생 — 결과와 무관하게 그냥 재생만 하고 끝난다.
    private void OnCardPlayed(CardData card)
    {
        if (card == null || card.Type != CardData.CardType.Attack) return;

        // 동료 소속 카드면 그 동료가 공격 모션 (소속은 CoopCharData.cardOwner)
        if (card.Owner != CardData.CardOwner.Player && card.Owner != CardData.CardOwner.Non_Color)
        {
            foreach (var (sequencer, data) in _companionVisuals.Values)
                if (data != null && data.cardOwner == card.Owner) { PlayCompanionAttack(data.charID); return; }
        }

        if (playerPoseSequencer == null)
        {
            if (!warnedMissingPoseSequencer)
            {
                Debug.LogWarning("[PartyView] playerPoseSequencer가 비어있음. Inspector 연결 확인 필요.");
                warnedMissingPoseSequencer = true;
            }
            return;
        }
        if (playerCharData == null)
        {
            if (!warnedMissingPlayerCharData)
            {
                Debug.LogWarning("[PartyView] playerCharData가 비어있음. Inspector 연결 확인 필요.");
                warnedMissingPlayerCharData = true;
            }
            return;
        }
        WeaponData currentWeapon = BattleManager.Instance?.State?.CurrentWeapon;

        // 1. 스프라이트 시트 기반 공격 애니메이션 우선 재생
        var attackFrames = playerCharData.GetAttackFrames(currentWeapon, out float attackFps);
        if (attackFrames != null && attackFrames.Length > 0)
        {
            playerPoseSequencer.Play(attackFrames, attackFps);
            return;
        }

        // 2. 키프레임 공격 시퀀스 재생
        var sequence = playerCharData.GetAttackSequence(currentWeapon);
        if (sequence != null && sequence.Length > 0)
        {
            playerPoseSequencer.Play(sequence);
            return;
        }

        if (!warnedEmptyAttackSequence)
        {
            Debug.LogWarning($"[PartyView] '{playerCharData.name}'의 attackFrames 및 attackSequence가 비어있음.");
            warnedEmptyAttackSequence = true;
        }
    }

    private void Refresh()
    {
        BindPlayer();
        BindCompanions();
        ApplyFormation();
        UpdateBlockGainSubscription();
        // 적 그림은 같은 이벤트 안에서 뒤늦게 자리 잡으므로, 한 프레임 뒤에 적 바닥선에 다시 맞춘다
        if (matchEnemyGround && isActiveAndEnabled) StartCoroutine(ReapplyFormationNextFrame());
    }

    private System.Collections.IEnumerator ReapplyFormationNextFrame()
    {
        yield return null;
        yield return null;
        ApplyFormation();
    }

    private void UpdateBlockGainSubscription()
    {
        PlayerCombatant current = BattleManager.Instance?.State?.Player;
        if (current == _subscribedPlayer) return;

        if (_subscribedPlayer != null)
        {
            _subscribedPlayer.OnBlockGained -= HandleBlockGained;
            _subscribedPlayer.OnDamaged -= HandlePlayerDamaged;
        }
        _subscribedPlayer = current;
        if (_subscribedPlayer != null)
        {
            _subscribedPlayer.OnBlockGained += HandleBlockGained;
            _subscribedPlayer.OnDamaged += HandlePlayerDamaged;
        }
    }

    // 피격 시 파티원 전원이 붉게 물들었다 돌아온다 — 방어도 연출과 같은 이유로 파티 전체가 함께 맞는 것으로 표현.
    // amount가 0이면 방어도로 전부 막은 것이라 연출하지 않는다.
    private void HandlePlayerDamaged(int amount)
    {
        if (amount <= 0) return;
        foreach (Transform anchor in GetActivePartyAnchors())
            HitFlash.Play(anchor.GetComponent<Graphic>());
    }

    // 방어 카드로 방어도를 얻었을 때 — 지금 화면에 있는 파티원 전원(플레이어 + 합류 중인
    // 협력자)의 스프라이트 위치에 방패/파티클을 재생한다.
    private void HandleBlockGained(int amount)
    {
        if (blockGainShieldPrefab == null && blockGainParticlePrefab == null) return;

        foreach (Transform anchor in GetActivePartyAnchors())
        {
            HitEffectSpawner.Spawn(blockGainShieldPrefab, anchor);
            HitEffectSpawner.Spawn(blockGainParticlePrefab, anchor);
        }
    }

    // 플레이어 스프라이트 + 합류 중인 협력자 슬롯의 CharacterSprite를 모은다.
    // "CharacterSprite"는 슬롯의 직속 자식이 아니라 손자다 — 슬롯 밑에 companionPrefab
    // 인스턴스가 하나 생기고, 그 인스턴스 밑에 CharacterSprite가 있다(BindCompanions 참고).
    // Transform.Find는 재귀 탐색이 아니라서 슬롯에 바로 Find를 걸면 못 찾는다.
    // 미합류 슬롯은 BindCompanions()가 자식을 안 만들어두므로 childCount==0이라 자동으로 빠진다.
    private IEnumerable<Transform> GetActivePartyAnchors()
    {
        if (playerImage != null) yield return playerImage.transform;

        if (companionSlots == null) yield break;
        foreach (var slot in companionSlots)
        {
            if (slot == null || slot.childCount == 0) continue;
            Transform sprite = slot.GetChild(0).Find("CharacterSprite");
            if (sprite != null) yield return sprite;
        }
    }

    private void BindPlayer()
    {
        if (playerImage == null)
        {
            Debug.LogWarning("[PartyView] playerImage가 비어있음. Inspector 연결 확인 필요.");
            return;
        }
        if (playerCharData == null)
            Debug.LogWarning("[PartyView] playerCharData가 비어있음. Inspector 연결 확인 필요.");

        if (playerSheetAnimator == null && playerImage != null)
            playerSheetAnimator = playerImage.GetComponent<UISpriteSheetAnimator>();

        WeaponData currentWeapon = BattleManager.Instance?.State?.CurrentWeapon;
        Sprite sprite = playerCharData != null
            ? playerCharData.GetStandingSprite(currentWeapon)
            : null;

        if (playerCharData != null && sprite == null)
            Debug.LogWarning($"[PartyView] '{playerCharData.name}'의 standingSprite가 비어있음.");

        playerImage.sprite  = sprite;
        playerImage.enabled = sprite != null;
        BattleUnitLook.Apply(playerImage, BattleUnitLook.Side.Ally);

        // 스프라이트 시트 기반 대기(Idle) 애니메이션 제어
        if (playerSheetAnimator != null && playerCharData != null)
        {
            var frames = playerCharData.GetIdleFrames(currentWeapon, out float fps);
            if (frames != null && frames.Length > 0)
            {
                playerSheetAnimator.enabled = true;
                playerSheetAnimator.Configure(frames, fps, true);
            }
            else
            {
                playerSheetAnimator.Stop();
                playerSheetAnimator.enabled = false;
                playerImage.sprite = sprite;
            }
        }
    }

    private void BindCompanions()
    {
        if (companionPrefab == null)
        {
            Debug.LogWarning("[PartyView] companionPrefab이 비어있음. Inspector 연결 확인 필요.");
            return;
        }
        if (companionSlots == null || companionSlots.Length == 0)
        {
            Debug.LogWarning("[PartyView] companionSlots가 비어있음. Inspector 연결 확인 필요.");
            return;
        }
        for (int i = 0; i < companionSlots.Length; i++)
        {
            if (companionSlots[i] == null)
                Debug.LogWarning($"[PartyView] companionSlots[{i}]가 비어있음. Inspector 연결 확인 필요.");
        }

        // 슬롯(위치 마커) 밑에 이미 생성된 게 있으면 지우고 다시 만든다 (재호출 대비 안전장치).
        foreach (var slot in companionSlots)
        {
            if (slot == null) continue;
            foreach (Transform child in slot)
                Destroy(child.gameObject);
        }

        if (CooperationManager.Instance == null)
        {
            Debug.LogWarning("[PartyView] CooperationManager.Instance가 없음. 씬(또는 부트 씬)에 CooperationManager가 있는지 확인 필요.");
            return;
        }

        // GetJoinedInRunCharStates()는 charID 기준 정렬 — 같은 파티 구성이면 항상 같은 슬롯에 앉는다.
        List<CoopCharState> joined = CooperationManager.Instance.GetJoinedInRunCharStates();
        if (joined.Count == 0)
            Debug.LogWarning("[PartyView] 합류한 협력자가 0명임 — 성소에서 SelectChar()가 호출됐는지 확인 필요.");

        _companionVisuals.Clear();

        for (int i = 0; i < companionSlots.Length; i++)
        {
            Transform slot = companionSlots[i];
            if (slot == null || i >= joined.Count || joined[i].charData == null)
                continue; // 미합류 슬롯은 빈 위치로 남김

            GameObject instance = Instantiate(companionPrefab, slot);

            // 슬롯(빈 오브젝트) 기준 정중앙에 앉히도록 강제 정렬.
            // 프리팹에 남아있는 앵커/피벗이 무엇이든 상관없이 "슬롯 위치 = 카드 중심"을 보장한다
            // (CardInteractionView.SetRestingPose에서 겪은 것과 같은 종류의 문제 예방).
            RectTransform rt = instance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition3D = Vector3.zero;
            }
            else
            {
                instance.transform.localPosition = Vector3.zero;
            }

            Transform spriteChild = instance.transform.Find("CharacterSprite");
            if (spriteChild == null || !spriteChild.TryGetComponent(out Image image))
            {
                Debug.LogWarning("[PartyView] companionPrefab 밑에서 'CharacterSprite'(Image)를 못 찾음. 프리팹 구조 확인 필요.");
                continue;
            }

            var coopData = joined[i].charData;
            image.sprite  = coopData.standingSprite;
            image.enabled = image.sprite != null;
            if (image.sprite == null)
                Debug.LogWarning($"[PartyView] '{coopData.name}'의 standingSprite가 비어있음.");

            ApplyStandingPose(instance.transform, image.rectTransform, coopData);
            // 떠다니는 동료는 프리팹 그림자(크기만 숨 쉬듯 변함)를 그대로 쓴다
            BattleUnitLook.Apply(image, BattleUnitLook.Side.Ally, shadow: coopData.floatAmplitude <= 0f);

            // 대기(Idle) 스프라이트 애니메이션이 등록되어 있으면 재생
            if (coopData.idleFrames != null && coopData.idleFrames.Length > 0)
            {
                var sheetAnimator = spriteChild.GetComponent<UISpriteSheetAnimator>();
                if (sheetAnimator == null) sheetAnimator = spriteChild.gameObject.AddComponent<UISpriteSheetAnimator>();
                sheetAnimator.enabled = true;
                sheetAnimator.Configure(coopData.idleFrames, coopData.idleFrameRate, true);
            }

            // 공격 포즈 재생기(PoseSequencePlayer) 준비
            var compSequencer = spriteChild.GetComponent<PoseSequencePlayer>();
            if (compSequencer == null) compSequencer = spriteChild.gameObject.AddComponent<PoseSequencePlayer>();

            if (!string.IsNullOrEmpty(coopData.charID))
            {
                _companionVisuals[coopData.charID] = (compSequencer, coopData);
            }
        }
    }

    // 주인공 + 합류 동료를 인원수 진형에 세운다. 각자 보이는 그림의 발끝을 진형 자리에 맞춘다(그림마다 여백이 달라서).
    private void ApplyFormation()
    {
        if (!useFormation || playerImage == null) return;
        var parent = playerImage.rectTransform.parent as RectTransform;
        if (parent == null) return;

        // (움직일 트랜스폼, 발을 잴 그림)
        var units = new List<(RectTransform move, Image image)> { (playerImage.rectTransform, playerImage) };
        if (companionSlots != null)
            foreach (var slot in companionSlots)
            {
                if (slot == null || slot.childCount == 0 || slot.parent != parent) continue;
                var sprite = slot.GetChild(0).Find("CharacterSprite");
                if (sprite != null && sprite.TryGetComponent(out Image img) && img.enabled) units.Add(((RectTransform)slot, img));
            }

        Vector2 Feet(Image img) => BattleUnitLook.TryVisibleFeetWorld(img, out Vector3 w) ? (Vector2)parent.InverseTransformPoint(w) : (Vector2)img.rectTransform.localPosition;
        if (!_hasHomeFeet) { _homeFeet = Feet(playerImage); _hasHomeFeet = true; }

        var layout = Formations[Mathf.Clamp(units.Count, 1, Formations.Length) - 1];
        // 값은 1920 화면 기준 px → 이 캔버스 단위로 (캔버스 기준 해상도가 달라도 같은 화면 간격)
        float k = parent.rect.width / 1920f;
        Vector2 center = _homeFeet + formationCenterOffset * k;
        if (matchEnemyGround && TryEnemyGroundY(parent, out float groundY)) center.y = groundY + enemyGroundOffsetY * k;
        var order = new List<(Transform t, float y)>();
        for (int i = 0; i < units.Count && i < layout.Length; i++)
        {
            Vector2 target = center + Vector2.Scale(layout[i], formationSpread * k);
            var (move, img) = units[i];
            Vector2 delta = target - Feet(img);
            move.localPosition += (Vector3)delta;
            order.Add((move, target.y));
        }

        // 아래(앞) 유닛이 위(뒤) 유닛을 가리게: 같은 자리 번호 묶음 안에서 위쪽부터 그린다
        var indices = new List<int>();
        foreach (var o in order) indices.Add(o.t.GetSiblingIndex());
        indices.Sort();
        order.Sort((a, b) => b.y.CompareTo(a.y));
        for (int i = 0; i < order.Count; i++) order[i].t.SetSiblingIndex(indices[i]);
    }

    // 살아 있는 적들의 발끝 높이 평균 (parent 로컬 좌표)
    private static bool TryEnemyGroundY(RectTransform parent, out float y)
    {
        y = 0f;
        float sum = 0f; int n = 0;
        foreach (var ev in FindObjectsByType<EnemyView>(FindObjectsSortMode.None))
        {
            if (!ev.isActiveAndEnabled || !ev.SpriteRect.TryGetComponent(out Image img) || !img.enabled) continue;
            if (!BattleUnitLook.TryVisibleFeetWorld(img, out Vector3 w)) continue;
            sum += parent.InverseTransformPoint(w).y; n++;
        }
        if (n == 0) return false;
        y = sum / n;
        return true;
    }

    // 동료별 크기·위치 보정과, 애니메이션이 없는 동료의 위아래 떠다니기.
    // 그림자는 크기를 따라가고, 떠오를 때 살짝 작아졌다가 내려올 때 커진다.
    private static void ApplyStandingPose(Transform root, RectTransform sprite, CoopCharData data)
    {
        float scale = data.battleScale > 0f ? data.battleScale : 1f;
        sprite.localScale = Vector3.one * scale;
        sprite.anchoredPosition += data.battleOffset;

        Transform shadow = root.Find("Shadow");
        if (shadow != null) shadow.localScale = Vector3.one * Mathf.Lerp(1f, scale, 0.7f);

        if (data.floatAmplitude <= 0f) return;
        float half = Mathf.Max(0.2f, data.floatPeriod * 0.5f);
        // 동료마다 박자가 겹치지 않게 시작 위상을 조금 흩뜨린다
        float phase = Random.Range(0f, half);
        sprite.DOAnchorPosY(sprite.anchoredPosition.y + data.floatAmplitude, half)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(sprite.gameObject).Goto(phase, true);
        if (shadow != null)
            shadow.DOScale(shadow.localScale * 0.85f, half)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(shadow.gameObject).Goto(phase, true);
    }
}
