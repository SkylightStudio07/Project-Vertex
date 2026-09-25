using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 플레이어 사격 스프라이트 시트 위에 총구 섬광을 덧그린다.
// 원본 시트(player_f_attack_handgun)의 섬광 프레임은 그림이 프레임 오른쪽 경계에서 잘려 있어서,
// 그 구간(replaceFrom~replaceTo)은 총을 뻗은 깨끗한 프레임(replacementFrame)으로 바꿔 보여주고
// 총구 위치에 별도 섬광 시트(ArtSource/VFX/gen-muzzleflash.js)를 재생한다.
// 캐릭터 스탠딩 Image(PoseSequencePlayer가 붙은 오브젝트)에 함께 붙인다.
[RequireComponent(typeof(PoseSequencePlayer))]
public class MuzzleFlashOverlay : MonoBehaviour
{
    [Header("대상 공격 시트")]
    [Tooltip("이 텍스처의 프레임이 재생될 때만 동작한다 (다른 무기 모션에는 영향 없음)")]
    [SerializeField] private Texture2D attackSheet;
    // 아래 번호는 재생 배열의 인덱스가 아니라 "시트 안 프레임 번호"(스프라이트 이름 끝 _N)다.
    // 공격 모션은 시트 일부(현재 36~50)만 골라 재생하므로 배열 인덱스로 잡으면 어긋난다.
    [SerializeField] private int replaceFrom = 43;       // 잘린 섬광이 그려진 첫 프레임
    [SerializeField] private int replaceTo = 47;         // 잘린 섬광/파편이 남아 있는 마지막 프레임
    [SerializeField] private int replacementFrame = 48;  // 대신 보여줄 깨끗한 프레임(총을 뻗은 자세)

    [Header("총구 위치 (원본 프레임 픽셀, 왼쪽 위 원점)")]
    [SerializeField] private Vector2 muzzlePixel = new(744f, 290f);
    [SerializeField] private Vector2 sourceFrameSize = new(832f, 1216f);

    [Header("섬광 시트")]
    [SerializeField] private Sprite[] flashFrames;
    [SerializeField] private float flashFps = 24f;
    [Tooltip("섬광 크기 배율. 1이면 원본 공격 프레임과 같은 픽셀 밀도")]
    [SerializeField] private float flashScale = 1f;

    private PoseSequencePlayer _player;
    private RectTransform _rect;
    private Image _image;
    private Image _flash;
    private Coroutine _playing;

    private void Awake()
    {
        _player = GetComponent<PoseSequencePlayer>();
        _rect = (RectTransform)transform;
        _image = GetComponent<Image>();
    }

    private void OnEnable() => _player.OnSpriteFrameShown += HandleFrame;

    private void OnDisable()
    {
        _player.OnSpriteFrameShown -= HandleFrame;
        StopFlash();
    }

    private void HandleFrame(Sprite[] frames, int index)
    {
        Sprite current = frames[index];
        if (attackSheet == null || current == null || current.texture != attackSheet) return;
        int sheetFrame = SheetFrameNumber(current);
        if (sheetFrame < replaceFrom || sheetFrame > replaceTo) return;

        foreach (var f in frames)
            if (f != null && SheetFrameNumber(f) == replacementFrame) { _image.sprite = f; break; }

        if (sheetFrame == replaceFrom) PlayFlash();
    }

    // 슬라이스 스프라이트 이름 끝의 번호 ("..._43" → 43). 번호가 없으면 -1.
    private static int SheetFrameNumber(Sprite sprite)
    {
        string name = sprite.name;
        int underscore = name.LastIndexOf('_');
        return underscore >= 0 && int.TryParse(name.Substring(underscore + 1), out int n) ? n : -1;
    }

    private void PlayFlash()
    {
        if (flashFrames == null || flashFrames.Length == 0) return;
        EnsureFlashImage();
        PlaceAtMuzzle();
        if (_playing != null) StopCoroutine(_playing);
        _playing = StartCoroutine(PlayFlashRoutine());
    }

    private IEnumerator PlayFlashRoutine()
    {
        _flash.enabled = true;
        float delay = 1f / Mathf.Max(1f, flashFps);
        foreach (var frame in flashFrames)
        {
            _flash.sprite = frame;
            yield return new WaitForSeconds(delay);
        }
        StopFlash();
    }

    private void StopFlash()
    {
        if (_playing != null) StopCoroutine(_playing);
        _playing = null;
        if (_flash != null) _flash.enabled = false;
    }

    private void EnsureFlashImage()
    {
        if (_flash != null) return;
        var go = new GameObject("MuzzleFlash", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        _flash = go.GetComponent<Image>();
        _flash.raycastTarget = false;
        _flash.enabled = false;
    }

    // 공격 프레임은 PoseSequencePlayer가 원본 비율로 rect를 맞추므로, 원본 픽셀 → rect 로컬 좌표로 그대로 환산된다.
    private void PlaceAtMuzzle()
    {
        Rect r = _rect.rect;
        float unitsPerPixel = r.height / sourceFrameSize.y;
        Vector2 local = new(
            r.xMin + muzzlePixel.x * unitsPerPixel,
            r.yMax - muzzlePixel.y * unitsPerPixel);

        var fr = _flash.rectTransform;
        Sprite first = flashFrames[0];
        // 섬광 시트의 피벗(총구 원점)에 맞춰 붙인다
        fr.anchorMin = fr.anchorMax = new Vector2(0.5f, 0.5f);
        fr.pivot = new Vector2(first.pivot.x / first.rect.width, first.pivot.y / first.rect.height);
        fr.sizeDelta = first.rect.size * unitsPerPixel * flashScale;
        fr.localPosition = local;
        fr.localRotation = Quaternion.identity;
        fr.localScale = Vector3.one;
    }
}
