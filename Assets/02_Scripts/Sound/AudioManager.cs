using DG.Tweening;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.5f;

    [Header("BGM Duck")]
    [SerializeField] [Range(0f, 1f)] private float duckRatio  = 0.3f;
    [SerializeField] private float duckDuration    = 0.4f;
    [SerializeField] private float restoreDuration = 0.6f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource.loop   = true;
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
    }

    private void Start()
    {
        PlayBGM(bgmClip);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---- BGM ----

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.DOKill();
        bgmSource.volume = bgmVolume;
    }

    // 패널 등장 시 BGM 볼륨을 duckRatio 배율로 줄임 (unscaled — timeScale=0에서도 동작)
    public void DuckBGM()
    {
        bgmSource.DOKill();
        bgmSource.DOFade(bgmVolume * duckRatio, duckDuration).SetUpdate(true);
    }

    // 패널 종료 시 BGM 볼륨 복원
    public void RestoreBGM()
    {
        bgmSource.DOKill();
        bgmSource.DOFade(bgmVolume, restoreDuration).SetUpdate(true);
    }

    // ---- SFX ----

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
}
