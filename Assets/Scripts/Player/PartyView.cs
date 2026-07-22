using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 전투 화면에 플레이어 스프라이트와 합류한 협력자들을 보여준다.
// 애니메이션 없이 정적 스프라이트만 표시 (연출은 추후 과제).
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

    [Header("합류 캐릭터 — 스프라이트를 생성해 넣을 프리팹과 위치 마커(빈 오브젝트, 최대 3명)")]
    [SerializeField] private Image companionPrefab;
    [SerializeField] private Transform[] companionSlots = new Transform[3];

    private void Start()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnBattleStarted += Refresh;

        // 이 오브젝트가 활성화되기 전에 이미 전투가 시작돼 있었을 경우(에디터에서 오브젝트를
        // 나중에 켜서 테스트하는 경우 등)를 대비해 최초 1회는 바로 그려본다.
        Refresh();
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnBattleStarted -= Refresh;
    }

    private void Refresh()
    {
        BindPlayer();
        BindCompanions();
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

        Sprite sprite = playerCharData != null ? playerCharData.standingSprite : null;
        if (playerCharData != null && sprite == null)
            Debug.LogWarning($"[PartyView] '{playerCharData.name}'의 standingSprite가 비어있음.");

        playerImage.sprite  = sprite;
        playerImage.enabled = sprite != null;
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

        for (int i = 0; i < companionSlots.Length; i++)
        {
            Transform slot = companionSlots[i];
            if (slot == null || i >= joined.Count || joined[i].charData == null)
                continue; // 미합류 슬롯은 빈 위치로 남김

            var image = Instantiate(companionPrefab, slot);

            // 슬롯(빈 오브젝트) 기준 정중앙에 앉히도록 강제 정렬.
            // 프리팹에 남아있는 앵커/피벗이 무엇이든 상관없이 "슬롯 위치 = 카드 중심"을 보장한다
            // (CardInteractionView.SetRestingPose에서 겪은 것과 같은 종류의 문제 예방).
            RectTransform rt = image.rectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition3D = Vector3.zero;
            }
            else
            {
                image.transform.localPosition = Vector3.zero;
            }

            image.sprite  = joined[i].charData.standingSprite;
            image.enabled = image.sprite != null;
            if (image.sprite == null)
                Debug.LogWarning($"[PartyView] '{joined[i].charData.name}'의 standingSprite가 비어있음.");
        }
    }
}
