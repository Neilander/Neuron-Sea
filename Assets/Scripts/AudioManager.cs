using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private Dictionary<BGMClip, BGMEntry> bgmDict;
    private Dictionary<WhiteNoiseClip, WhiteNoiseEntry> whiteNoiseDict;
    private Dictionary<SFXClip, SFXEntry> sfxDict;

    [System.Serializable]
    public class BGMEntry { public BGMClip key; public AudioClip clip;
        [Range(0, 1)]
        public float volume;
    }
    [System.Serializable]
    public class WhiteNoiseEntry {
        public WhiteNoiseClip key;
        public AudioClip clip;
        [Range(0, 1)]
        public float volume;
    }
    [System.Serializable]
    public class SFXEntry
    {
        public SFXClip key;
        public AudioClip clip;
        [Range(0,1)]
        public float volume;
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
        SceneManager.activeSceneChanged += ChangeBGM;

        bgmDict = bgmClips.ToDictionary(e => e.key, e => e);
        whiteNoiseDict = whiteNoiseClips.ToDictionary(e => e.key, e => e);
        sfxDict = sfxClips.ToDictionary(e => e.key, e => e); // 保持不动
    }
    private void Start()
    {
        ChangeBGM(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
    }

    void ChangeBGM(Scene oldScene, Scene newScene)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == newScene.buildIndex)
            {
                AudioManager.Instance.Play((BGMClip)i);
                AudioManager.Instance.Play((WhiteNoiseClip)i);
            }
            else
            {
                AudioManager.Instance.Stop((BGMClip)i);
                AudioManager.Instance.Stop((WhiteNoiseClip)i);
            }
        }
    }

    public void ClickSound(int soundID)
    {
        AudioManager.Instance.Play((SFXClip)(soundID + 27));
    }

    private BGMClip curClip;
    public void Play(BGMClip clipKey)
    {
        if (!bgmDict.TryGetValue(clipKey, out var entry) || entry.clip == null)
            return;

        if (bgmSourceDict.TryGetValue(clipKey, out var oldSource))
        { 
            oldSource.UnPause();
        }
        else
        {
            curClip = clipKey;
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = entry.clip;
            newSource.loop = true;
            newSource.volume = entry.volume;
            newSource.Play();

            bgmSourceDict[clipKey] = newSource;
        }
    }

    public void Play(WhiteNoiseClip clipKey)
    {
        if (!whiteNoiseDict.TryGetValue(clipKey, out var entry) || entry.clip == null)
            return;

        if (whiteNoiseSourceDict.TryGetValue(clipKey, out var oldSource))
        {
            oldSource.Stop();
            Destroy(oldSource);
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = entry.clip;
        newSource.loop = true;
        newSource.volume = entry.volume;
        newSource.Play();

        whiteNoiseSourceDict[clipKey] = newSource;
    }

    public void Play(SFXClip clipKey, float volumeDebuff = 1f)
    {
        if (!sfxDict.TryGetValue(clipKey, out var entry) || entry.clip == null)
        {
            Debug.LogWarning($"SFXClip {clipKey} 未找到或为空");
            return;
        }

        if (sfxSourceDict.TryGetValue(clipKey, out var oldSource))
        {
            oldSource.Stop();
            Destroy(oldSource);
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = entry.clip;
        newSource.loop = entry.loop;
        newSource.volume = entry.volume * Mathf.Clamp01(volumeDebuff);
        newSource.Play();

        sfxSourceDict[clipKey] = newSource;

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

    public void Pause(BGMClip key)
    {
        if (bgmSourceDict.TryGetValue(key, out var source))
        {
            source.Pause();
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
            if (source.loop)
            {
                source.loop = false;
            }
            else
            {
                source.Stop();
            }
            StartCoroutine(DestroyWhenDone(source, key));
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

    public void PauseBGM()
    {
        Pause(curClip);
    }

    public void ResumeBGM()
    {
        Play(curClip);
    }
}

public enum BGMClip
{
    SceneBegin,
    Scene1,
    Scene2,
    Scene3,
}

public enum WhiteNoiseClip
{
    SceneBegin,
    Scene1,
    Scene2,
    Scene3,
}

public enum SFXClip
{
    Teleport,
    Jump,
    Drop,
    Switch,
    IdleBox,
    BulletTimeIn,
    BulletTimeOut,
    PickUpCollectable,
    ObjSelection,
    BulletContinune,
    PlayerDeath,
    Scene1Spike,
    AutoMoveBox,
    BoomIdle,
    BoomStart,
    BoomExplosion,
    BoomEnd,
    AutoMoveBoxTurnBack,
    Scene1Walk,
    Scene2Walk,
    Scene3Walk,
    Scene3Spike,
    TeleportDoor,
    BoomTouch,
    TouchMoveBox,
    TouchMoveBoxTurnBack,
    Scan,
    Cilck0,
    Cilck1,
    Cilck2,
    Cilck3,
    Cilck4,
    Cilck5,
    Cilck6,
    Cilck7,
    StartVideo,
    EnterLevel,
}