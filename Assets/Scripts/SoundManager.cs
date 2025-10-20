using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sfx
{
    //general
    ButtonClick = 0,

    //Arrival
    DoorCreak,
    Knock,

    //Recipe
    BookFlip,
    StartCraft,

    //Crafting
    StepFail,
    StepSuccess,
    Stirring,
    PourWater,
    PourSugar,
    PourEssence,
    WaterBoil,
    CandyDone,
    TimeTicking,
    BurnAlert,

    //result
    CraftingDone,

    //ending
    RemoteControl

}

public enum Bgm
{
    Title = 0,
    Battle,
    Shop,
    Boss,
    FinalStage,
    Ending,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] sfxClips;
    
    [Header("Players")]
    private AudioSource bgmPlayer;
    private AudioSource[] sfxPlayers;

    [Header("Settings")]
    [SerializeField] private int sfxChannels = 20;
    [SerializeField] private float bgmVolume = 0.6f;
    [SerializeField] private bool fadeOnBgmChange = false; // set true if you want a tiny fade
    [SerializeField] private float bgmFadeTime = 0.4f;

    private Coroutine bgmFadeRoutine;
    private Bgm currentBgm;
    private int nextSfxIndex;

    [SerializeField] private Sfx boilingSfxEnum = Sfx.WaterBoil; // assign your actual boiling clip enum
    private AudioSource ambienceLoopPlayer;

    private readonly Dictionary<Sfx, AudioSource> _loopPlayers = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // BGM source on this object
        bgmPlayer = GetComponent<AudioSource>();
        if (!bgmPlayer) bgmPlayer = gameObject.AddComponent<AudioSource>();
        bgmPlayer.loop = true;
        bgmPlayer.playOnAwake = false;
        bgmPlayer.volume = bgmVolume;

        // ambience loop source
        ambienceLoopPlayer = gameObject.AddComponent<AudioSource>();
        ambienceLoopPlayer.playOnAwake = false;
        ambienceLoopPlayer.loop = true;
        ambienceLoopPlayer.volume = 0.9f;

        // SFX pool as children
        sfxPlayers = new AudioSource[sfxChannels];
        var sfxRoot = new GameObject("SFX_Pool");
        sfxRoot.transform.SetParent(transform, false);

        for (int i = 0; i < sfxChannels; i++)
        {
            var src = sfxRoot.AddComponent<AudioSource>();
            src.playOnAwake = false;
            sfxPlayers[i] = src;
        }

        PlayBGM(Bgm.Title);
    }

    // --- BGM ---

    public void PlayBGM(Bgm bgm, bool loop = true, float volume = -1f)
    {
        currentBgm = bgm;
        if (volume >= 0f) bgmVolume = Mathf.Clamp01(volume);

        var clip = bgmClips != null && (int)bgm < bgmClips.Length ? bgmClips[(int)bgm] : null;
        if (!clip) { Debug.LogWarning($"[SoundManager] Missing BGM clip for {bgm}"); return; }

        if (fadeOnBgmChange)
        {
            if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);
            bgmFadeRoutine = StartCoroutine(FadeSwapBgm(clip, loop));
        }
        else
        {
            bgmPlayer.Stop();
            bgmPlayer.clip = clip;
            bgmPlayer.loop = loop;
            bgmPlayer.volume = bgmVolume;
            bgmPlayer.Play();
        }
    }

    public void StopBGM(bool fade = false)
    {
        if (fade && fadeOnBgmChange)
        {
            if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);
            bgmFadeRoutine = StartCoroutine(FadeOutBgm());
        }
        else
        {
            bgmPlayer.Stop();
        }
    }

    private IEnumerator FadeSwapBgm(AudioClip newClip, bool loop)
    {
        // fade out
        float t = 0f;
        float startVol = bgmPlayer.volume;
        while (t < bgmFadeTime && bgmPlayer.isPlaying)
        {
            t += Time.unscaledDeltaTime;
            bgmPlayer.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeTime);
            yield return null;
        }

        // swap
        bgmPlayer.Stop();
        bgmPlayer.clip = newClip;
        bgmPlayer.loop = loop;
        bgmPlayer.volume = 0f;
        bgmPlayer.Play();

        // fade in
        t = 0f;
        while (t < bgmFadeTime)
        {
            t += Time.unscaledDeltaTime;
            bgmPlayer.volume = Mathf.Lerp(0f, bgmVolume, t / bgmFadeTime);
            yield return null;
        }
        bgmFadeRoutine = null;
    }

    private IEnumerator FadeOutBgm()
    {
        float t = 0f;
        float startVol = bgmPlayer.volume;
        while (t < bgmFadeTime && bgmPlayer.isPlaying)
        {
            t += Time.unscaledDeltaTime;
            bgmPlayer.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeTime);
            yield return null;
        }
        bgmPlayer.Stop();
        bgmPlayer.volume = bgmVolume;
        bgmFadeRoutine = null;
    }

    public Bgm GetCurrentBGM() => currentBgm;

    // --- SFX ---

    public void PlaySfx(Sfx sfx, bool loop = false, float volume = 1f)
    {
        var clip = sfxClips != null && (int)sfx < sfxClips.Length ? sfxClips[(int)sfx] : null;
        if (!clip) { Debug.LogWarning($"[SoundManager] Missing SFX clip for {sfx}"); return; }

        // find a free channel (round-robin)
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int idx = (nextSfxIndex + i) % sfxPlayers.Length;
            if (!sfxPlayers[idx].isPlaying)
            {
                nextSfxIndex = (idx + 1) % sfxPlayers.Length;
                var src = sfxPlayers[idx];
                src.clip = clip;
                src.loop = loop;
                src.volume = Mathf.Clamp01(volume);
                src.Play();
                return;
            }
        }

        // if all busy, steal next slot
        var steal = sfxPlayers[nextSfxIndex];
        nextSfxIndex = (nextSfxIndex + 1) % sfxPlayers.Length;
        steal.Stop();
        steal.clip = clip;
        steal.loop = loop;
        steal.volume = Mathf.Clamp01(volume);
        steal.Play();
    }

    public void StopSfx(Sfx sfx)
    {
        var clip = (int)sfx < sfxClips.Length ? sfxClips[(int)sfx] : null;
        if (!clip) return;

        foreach (var src in sfxPlayers)
        {
            if (src.isPlaying && src.clip == clip)
            {
                src.Stop();
                src.loop = false;
            }
        }
    }

    public void OnButtonPress()
    {
        PlaySfx(Sfx.ButtonClick);
    }

    public void OnRecipeNextPress()
    {
        PlaySfx(Sfx.BookFlip);
    }

    public void StartBoilingLoop(float volume = 1f)
    {
        var clip = (int)boilingSfxEnum < sfxClips.Length ? sfxClips[(int)boilingSfxEnum] : null;
        if (!clip) { Debug.LogWarning("[SoundManager] Boiling SFX clip missing"); return; }

        ambienceLoopPlayer.clip = clip;
        ambienceLoopPlayer.volume = Mathf.Clamp01(volume);
        if (!ambienceLoopPlayer.isPlaying) ambienceLoopPlayer.Play();
    }

    public void StopBoilingLoop()
    {
        if (ambienceLoopPlayer.isPlaying) ambienceLoopPlayer.Stop();
    }

    public void StartLoopSfx(Sfx sfx, float volume = 1f)
    {
        if (_loopPlayers.ContainsKey(sfx) && _loopPlayers[sfx] && _loopPlayers[sfx].isPlaying)
            return;

        var clip = (int)sfx < sfxClips.Length ? sfxClips[(int)sfx] : null;
        if (!clip)
        {
            Debug.LogWarning($"[SoundManager] Missing loop SFX clip for {sfx}");
            return;
        }

        var go = new GameObject($"Loop_{sfx}");
        go.transform.SetParent(transform, false);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.volume = Mathf.Clamp01(volume);
        src.clip = clip;
        src.Play();

        _loopPlayers[sfx] = src;
    }

    public void StopLoopSfx(Sfx sfx, float fadeTime = 0f)
    {
        if (!_loopPlayers.TryGetValue(sfx, out var src) || !src) return;

        if (fadeTime <= 0f)
        {
            src.Stop();
            Destroy(src.gameObject);
            _loopPlayers.Remove(sfx);
        }
        else
        {
            StartCoroutine(FadeAndDestroyLoop(src, sfx, fadeTime));
        }
    }

    private IEnumerator FadeAndDestroyLoop(AudioSource src, Sfx sfx, float time)
    {
        float t = 0f;
        float start = src.volume;
        while (t < time && src)
        {
            t += Time.unscaledDeltaTime;
            if (src) src.volume = Mathf.Lerp(start, 0f, t / time);
            yield return null;
        }
        if (src)
        {
            src.Stop();
            Destroy(src.gameObject);
        }
        _loopPlayers.Remove(sfx);
    }

}
