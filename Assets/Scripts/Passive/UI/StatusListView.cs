using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// StatusContainer의 내용을 칩 목록으로 표시한다. 플레이어/적 양쪽에서 같은 컴포넌트를 쓴다.
//
// 갱신은 폴링으로 처리한다. StatusContainer.OnChanged 이벤트가 있긴 하지만,
// StatusBehavior가 StatusInstance.ReduceMagnitude()/SetMagnitude()를 직접 호출하는 경로에서는
// 컨테이너를 거치지 않아 이벤트가 발생하지 않는다. 표시가 실제 상태와 어긋나면
// 전투 디버깅 도구로서 쓸모가 없어지므로, 확실성을 택해 매 프레임 시그니처를 비교한다.
// (PlayerHUDView도 같은 이유로 폴링을 쓰고 있다.)
//
// 시그니처가 그대로인 프레임에는 아무 것도 만지지 않으므로 GC Alloc이 발생하지 않는다.
public class StatusListView : MonoBehaviour
{
    [SerializeField] private StatusChipView chipPrefab;
    [SerializeField] private RectTransform container; // Grid Layout Group이 붙는 오브젝트
    [SerializeField] private bool hideWhenEmpty = true;

    // 한 줄에 놓을 최대 칩 수. 초과분은 다음 줄로 내려간다(STS와 같은 방식).
    // 기본 6은 칩 34px + 간격 4px 기준 224px로, 플레이어 HP 패널 폭(253px)에 들어가는 최대값이다.
    // 플레이어와 적이 쓸 수 있는 가로 폭이 달라서 인스펙터에서 조절할 수 있게 둔다.
    [SerializeField, Min(1)] private int maxPerRow = 6;

    private StatusContainer _bound;
    private readonly List<StatusChipView> _chips = new();

    // 아직 한 번도 갱신되지 않은 상태를 나타내는 sentinel. 실제 시그니처와 충돌할 확률이 낮은 값.
    private const int Unset = int.MinValue;
    private int _lastSignature = Unset;

    private void Awake() => ApplyRowLimit();

    // 줄바꿈은 GridLayoutGroup의 고정 열 수로 처리한다.
    // 칩이 아이콘 기반이라 폭이 균일해서 그리드가 잘 맞는다.
    private void ApplyRowLimit()
    {
        if (container == null) return;
        var grid = container.GetComponent<GridLayoutGroup>();
        if (grid == null) return;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, maxPerRow);
    }

    public void Bind(StatusContainer statuses)
    {
        _bound = statuses;
        _lastSignature = Unset; // 새로 바인드하면 내용이 같아 보여도 무조건 한 번 그린다.
    }

    public void Unbind()
    {
        _bound = null;
        _lastSignature = Unset;
        ClearAllChips();
        ApplyEmptyVisibility(true);
    }

    private void Update()
    {
        if (_bound == null) return;

        // 인스펙터 연결이 빠진 상태로 매 프레임 경고를 쏟아내지 않도록 조용히 멈춘다.
        // (누락 경고는 OnValidate에서 에디터 시점에 한 번만 알린다.)
        if (chipPrefab == null || container == null) return;

        int signature = ComputeSignature();
        if (signature == _lastSignature) return;

        _lastSignature = signature;
        Refresh();
    }

    // 표시에 영향을 주는 값(정의 + 스택)만 반영한 해시.
    // 스택 수치가 바뀌면 시그니처도 바뀌므로 수치 변화까지 잡아낸다.
    private int ComputeSignature()
    {
        int hash = 17;
        foreach (var entry in _bound.Entries)
        {
            if (entry is not StatusInstance status) continue;
            if (status.Definition == null || status.Stacks == 0) continue;

            hash = hash * 31 + status.Definition.GetInstanceID();
            hash = hash * 31 + status.Stacks;
        }
        return hash;
    }

    private void Refresh()
    {
        int used = 0;

        foreach (var entry in _bound.Entries)
        {
            if (entry is not StatusInstance status) continue;
            if (status.Definition == null || status.Stacks == 0) continue;

            GetOrCreateChip(used).Bind(status);
            used++;
        }

        // 남는 칩은 파괴하지 않고 비활성화해 둔다. 상태는 매 턴 붙었다 떨어지므로
        // 재생성하면 불필요한 할당이 반복된다.
        for (int i = used; i < _chips.Count; i++)
            _chips[i].Clear();

        ApplyEmptyVisibility(used == 0);
    }

    private StatusChipView GetOrCreateChip(int index)
    {
        if (index < _chips.Count) return _chips[index];

        var chip = Instantiate(chipPrefab, container);
        _chips.Add(chip);
        return chip;
    }

    private void ClearAllChips()
    {
        foreach (var chip in _chips) chip.Clear();
    }

#if UNITY_EDITOR
    // 인스펙터 연결 누락은 런타임에 "아무것도 안 보임"으로만 드러나서 원인을 찾기 어렵다.
    // 에디터에서 값이 바뀔 때 미리 알린다.
    private void OnValidate()
    {
        if (chipPrefab == null)
            Debug.LogWarning($"[{nameof(StatusListView)}] '{name}': chipPrefab 미연결.", this);
        if (container == null)
            Debug.LogWarning($"[{nameof(StatusListView)}] '{name}': container 미연결.", this);

        // 인스펙터에서 maxPerRow를 바꾸면 씬 뷰에 바로 반영된다.
        ApplyRowLimit();
    }
#endif

    private void ApplyEmptyVisibility(bool isEmpty)
    {
        if (!hideWhenEmpty || container == null) return;

        // container를 이 오브젝트 자신으로 연결하면 비활성화하는 순간 Update()가 멈춰
        // 다시 켜질 기회가 없어진다(상태가 생겨도 영영 안 보임). 인스펙터에서 실수하기 쉬워서 막아둔다.
        if (container.gameObject == gameObject)
        {
            Debug.LogWarning(
                $"[{nameof(StatusListView)}] '{name}': container가 자기 자신으로 연결되어 있습니다. " +
                $"자식 오브젝트를 지정하세요. 숨김 처리를 건너뜁니다.", this);
            return;
        }

        container.gameObject.SetActive(!isEmpty);
    }
}
