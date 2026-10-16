using UnityEngine;

// 로비 BGM. 인트로를 한 번 틀고, 끝나는 샘플에 맞춰 본문을 이어 붙여 그 뒤로 본문만 반복한다(PlayScheduled로 이음매 없음).
// 씬이 열리면 서서히 커지고, 씬 전환 막이 닫히기 시작하면(출정·훈련) 서서히 줄어든다.
// 클립: Assets/Audio/BGM/bgm_lobby_intro.wav(4마디) + bgm_lobby_loop.wav(24마디, 반복 이음매·잔향 꼬리 처리 완료)
//   원곡 MIDI·생성 스크립트: ArtDirection/Music (REAPER + BBC Symphony Orchestra로 렌더)
public class LobbyBgm : MonoBehaviour
{
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip loopClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
    [SerializeField, Min(0f)] private float fadeIn = 2f;
    [SerializeField, Min(0f)] private float fadeOut = 0.6f;

    public static float MusicVolume = 1f; // 설정 화면이 생기면 연결

    private AudioSource _intro, _loop;
    private float _level; // 0 → 1 페이드 배율

    private void Awake()
    {
        _intro = NewSource(introClip, false);
        _loop = NewSource(loopClip, true);
    }

    private AudioSource NewSource(AudioClip clip, bool loop)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.clip = clip;
        s.loop = loop;
        s.playOnAwake = false;
        s.spatialBlend = 0f;
        s.volume = 0f;
        return s;
    }

    private void Start()
    {
        double start = AudioSettings.dspTime + 0.1;
        if (introClip != null)
        {
            _intro.PlayScheduled(start);
            start += (double)introClip.samples / introClip.frequency;
        }
        if (loopClip != null) _loop.PlayScheduled(start);
    }

    private void Update()
    {
        // 전환 막이 닫히는 동안은 줄이고, 그 외에는 키운다
        bool leaving = SceneTransition.IsTransitioning;
        float speed = leaving ? (fadeOut > 0f ? 1f / fadeOut : 999f) : (fadeIn > 0f ? 1f / fadeIn : 999f);
        _level = Mathf.MoveTowards(_level, leaving ? 0f : 1f, speed * Time.unscaledDeltaTime);
        float v = volume * MusicVolume * _level;
        _intro.volume = v;
        _loop.volume = v;
    }
}
