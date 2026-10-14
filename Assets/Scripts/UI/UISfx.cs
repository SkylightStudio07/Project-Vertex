using System.Collections.Generic;
using UnityEngine;

// UI 효과음 재생기. UISfx.Play("ui_click")처럼 이름으로 부른다.
// 클립: Resources/Audio/UI/<이름>.wav (명일방주식 작전 단말기 톤 — 짧은 전자음 클릭·블립·가벼운 스윕)
//   ui_hover · ui_click · ui_back · ui_panel_open · ui_deploy · ui_lobby_enter
// 씬을 넘어 살아 있는 2D AudioSource 하나로 겹쳐 재생한다(PlayOneShot). 같은 소리가 한 프레임에 몰려도
// 짧은 간격 안에서는 한 번만 낸다(호버를 훑을 때 따닥거림 방지). 음량은 Volume(설정 화면이 생기면 연결).
public static class UISfx
{
    private const string Folder = "Audio/UI/";
    private const float MinInterval = 0.05f;

    public static float Volume = 1f;

    private static AudioSource s_source;
    private static readonly Dictionary<string, AudioClip> s_clips = new();
    private static readonly Dictionary<string, float> s_lastPlayed = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_source = null;
        s_clips.Clear();
        s_lastPlayed.Clear();
    }

    public static void Play(string name, float volumeScale = 1f)
    {
        if (string.IsNullOrEmpty(name) || !Application.isPlaying) return;
        var clip = Clip(name);
        if (clip == null) return;

        float now = Time.unscaledTime;
        if (s_lastPlayed.TryGetValue(name, out float last) && now - last < MinInterval) return;
        s_lastPlayed[name] = now;

        Source().PlayOneShot(clip, Mathf.Clamp01(Volume * volumeScale));
    }

    private static AudioClip Clip(string name)
    {
        if (!s_clips.TryGetValue(name, out var clip))
        {
            clip = Resources.Load<AudioClip>(Folder + name);
            if (clip == null) Debug.LogWarning($"[UISfx] 효과음 없음: Resources/{Folder}{name}");
            s_clips[name] = clip;
        }
        return clip;
    }

    private static AudioSource Source()
    {
        if (s_source != null) return s_source;
        var go = new GameObject("[UISfx]");
        Object.DontDestroyOnLoad(go);
        s_source = go.AddComponent<AudioSource>();
        s_source.playOnAwake = false;
        s_source.spatialBlend = 0f;   // UI는 2D
        s_source.ignoreListenerPause = true;
        return s_source;
    }
}
