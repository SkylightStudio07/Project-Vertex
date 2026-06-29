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

        Vector3 position = anchor.position;
        Quaternion rotation = Quaternion.identity;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 toCamera = (cam.transform.position - position).normalized;
            position += toCamera * cameraOffset;
            rotation = Quaternion.LookRotation(cam.transform.position - position);
        }

        Object.Instantiate(prefab, position, rotation, anchor);
    }
}
