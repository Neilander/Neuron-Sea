using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource whiteNoiseSource;
    public AudioSource bgmSource;
    public AudioSource[] sfxSources;

    private int sfxIndex = 0;

    [Header("Audio Clips")]
    public List<BGMEntry> bgmClips;
    public List<WhiteNoiseEntry> whiteNoiseClips;
    public List<SFXEntry> sfxClips;

    private Dictionary<BGMClip, AudioClip> bgmDict;
    private Dictionary<WhiteNoiseClip, AudioClip> whiteNoiseDict;
    private Dictionary<SFXClip, SFXEntry> sfxDict;

    [System.Serializable]
    public class BGMEntry { public BGMClip key; public AudioClip clip; }
    [System.Serializable]
    public class WhiteNoiseEntry { public WhiteNoiseClip key; public AudioClip clip; }
    [System.Serializable]
    public class SFXEntry
    {
        public SFXClip key;
        public AudioClip clip;
        public bool loop;
    }

    private Dictionary<BGMClip, AudioSource> bgmSourceDict = new();
    private Dictionary<WhiteNoiseClip, AudioSource> whiteNoiseSourceDict = new();
    private Dictionary<SFXClip, AudioSource> sfxSourceDict = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmDict = bgmClips.ToDictionary(e => e.key, e => e.clip);
        whiteNoiseDict = whiteNoiseClips.ToDictionary(e => e.key, e => e.clip);
        sfxDict = sfxClips.ToDictionary(e => e.key, e => e);
    }

    public void Play(BGMClip clipKey, float volume = 1f)
    {
        if (!bgmDict.TryGetValue(clipKey, out var clip) || clip == null)
            return;

        if (bgmSourceDict.TryGetValue(clipKey, out var oldSource))
        {
            oldSource.Stop();
            Destroy(oldSource);
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.loop = true;
        newSource.volume = volume;
        newSource.Play();

        bgmSourceDict[clipKey] = newSource;
    }

    public void Play(WhiteNoiseClip clipKey, float volume = 1f)
    {
        if (!whiteNoiseDict.TryGetValue(clipKey, out var clip) || clip == null)
            return;

        if (whiteNoiseSourceDict.TryGetValue(clipKey, out var oldSource))
        {
            oldSource.Stop();
            Destroy(oldSource);
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.loop = true;
        newSource.volume = volume;
        newSource.Play();

        whiteNoiseSourceDict[clipKey] = newSource;
    }

    public void Play(SFXClip clipKey, float volume = 1f)
    {
        if (!sfxDict.TryGetValue(clipKey, out var entry) || entry.clip == null)
        {
            Debug.LogWarning($"SFXClip {clipKey} 未找到或为空");
            return;
        }

        // 如果已经有旧的 source，在播放新的前先销毁
        if (sfxSourceDict.TryGetValue(clipKey, out var oldSource))
        {
            oldSource.Stop();
            Destroy(oldSource);
        }

        // 创建新的 AudioSource 并播放
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = entry.clip;
        newSource.loop = entry.loop;
        newSource.volume = volume;
        newSource.Play();

        // 存入字典
        sfxSourceDict[clipKey] = newSource;

        // 非 loop 的自动销毁 + 移除字典
        if (!entry.loop)
            StartCoroutine(DestroyWhenDone(newSource, clipKey));
    }

    public void Stop(BGMClip key)
    {
        if (bgmSourceDict.TryGetValue(key, out var source))
        {
            source.Stop();
            Destroy(source);
            bgmSourceDict.Remove(key);
        }
    }

    public void Stop(WhiteNoiseClip key)
    {
        if (whiteNoiseSourceDict.TryGetValue(key, out var source))
        {
            source.Stop();
            Destroy(source);
            whiteNoiseSourceDict.Remove(key);
        }
    }

    public void Stop(SFXClip key)
    {
        if (sfxSourceDict.TryGetValue(key, out var source))
        {
            source.Stop();
            Destroy(source);
            sfxSourceDict.Remove(key);
        }
    }

    private IEnumerator DestroyWhenDone(AudioSource source, SFXClip key)
    {
        yield return new WaitForSeconds(source.clip.length);

        if (sfxSourceDict.TryGetValue(key, out var currentSource) && currentSource == source)
        {
            sfxSourceDict.Remove(key);
        }

        Destroy(source);
    }
}

public enum BGMClip
{
    Title,
    Level1,
    BossFight
}

public enum WhiteNoiseClip
{
    Wind,
    Rain,
    Ocean
}

public enum SFXClip
{
    Walk,
}