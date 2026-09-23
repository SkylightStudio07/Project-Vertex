using UnityEngine;

// 피격 이펙트를 Screen Space - Camera Canvas UI 앞에 확실히 보이도록 스폰하는 공용 헬퍼.
// UI 엘리먼트의 world position은 Canvas Plane Distance와 정확히 같은 깊이라, 그대로 스폰하면
// 파티클 자체의 두께/오프셋 때문에 카메라 기준 캔버스보다 더 멀어져 UI 뒤로 렌더링되는 문제가 있다.
// 카메라 쪽으로 살짝 당겨서 스폰하고, 카메라를 바라보도록 회전시켜 막는다
// (Hit Impact Effects 데모의 transform.LookAt(Camera.main.transform) 방식과 동일).
public static class HitEffectSpawner
{
    public static void Spawn(GameObject prefab, Transform anchor, float cameraOffset = 15f)
    {
        if (prefab == null || anchor == null) return;

        Vector3 position = ResolveWorldPosition(anchor);
        Quaternion rotation = Quaternion.identity;

        Camera cam = Camera.main;
        if (cam != null)
        {
            // 깊이만 당긴다. 직교 카메라에서 카메라 위치 쪽으로 대각선 이동하면
            // 화면 가장자리의 적 이펙트가 캐릭터가 아니라 화면 중앙 쪽에 나타난다.
            Vector3 screenPosition = cam.WorldToScreenPoint(position);
            screenPosition.z = Mathf.Max(cam.nearClipPlane + 0.01f, screenPosition.z - cameraOffset);
            position = cam.ScreenToWorldPoint(screenPosition);
            rotation = Quaternion.LookRotation(cam.transform.position - position);
        }

        // 부모 없이 월드 공간에 스폰한다.
        // anchor를 부모로 지정하면 Canvas 계층의 Scale을 상속받아 파티클이 왜곡되거나
        // 보이지 않을 정도로 작아지는 문제가 있다.
        // 단발성 이펙트는 앵커를 따라다닐 필요가 없고, 파티클 시스템의 Stop Action을
        // Destroy로 설정해두면 재생 후 자동 정리된다.
        Object.Instantiate(prefab, position, rotation);
    }

    // RectTransform.position은 피벗의 월드 좌표일 뿐 스프라이트의 시각적 중앙이 아니다.
    // 캐릭터 스탠딩 스프라이트는 흔히 피벗을 발밑(0.5, 0)에 둬서 바닥에 붙는 것처럼 보이게 하는데
    // (예: PartyView.playerImage), 그 anchor.position을 그대로 쓰면 이펙트가 발밑에서 튀어나온다.
    // RectTransform이면 rect의 실제 중앙을 계산해서 쓴다 — 피벗이 이미 중앙(0.5, 0.5)인
    // 오브젝트(대부분의 협력자 스프라이트 등)에는 결과가 그대로 같아 안전하다.
    private static Vector3 ResolveWorldPosition(Transform anchor)
    {
        if (anchor is RectTransform rt)
        {
            Vector2 center = rt.rect.center;
            return rt.TransformPoint(new Vector3(center.x, center.y, 0f));
        }
        return anchor.position;
    }
}
