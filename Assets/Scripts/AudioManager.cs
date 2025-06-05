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
    //private Dictionary<SFXClip, AudioSource> sfxSourceDict = new();
    private Dictionary<SFXClip, Dictionary<string, AudioSource>> sfxSourceDict = new();


    private const string MASTER_KEY = "MasterVolume";
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += StopAllVFX;
        SceneManager.activeSceneChanged += ChangeBGM; 

        bgmDict = bgmClips.ToDictionary(e => e.key, e => e);
        whiteNoiseDict = whiteNoiseClips.ToDictionary(e => e.key, e => e);
        sfxDict = sfxClips.ToDictionary(e => e.key, e => e); // 保持不动

        masterVolume = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);
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
        AudioManager.Instance.Play((SFXClip)(soundID + 27),"System");
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
            newSource.volume = entry.volume * GetBGMVolumeMultiplier();
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
        newSource.volume = entry.volume*GetBGMVolumeMultiplier(); ;
        newSource.Play();

        whiteNoiseSourceDict[clipKey] = newSource;
    }

    public void Play(SFXClip clipKey, string id, float volumeDebuff = 1f)
    {
        if (!sfxDict.TryGetValue(clipKey, out var entry) || entry.clip == null)
        {
            Debug.LogWarning($"SFXClip {clipKey} 未找到或为空");
            return;
        }

        if (!sfxSourceDict.TryGetValue(clipKey, out var innerDict))
        {
            innerDict = new Dictionary<string, AudioSource>();
            sfxSourceDict[clipKey] = innerDict;
        }

        if (innerDict.TryGetValue(id, out var existingSource))
        {
            existingSource.Stop();
            Destroy(existingSource);
            innerDict.Remove(id);
        }


        

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = entry.clip;
        newSource.loop = entry.loop;
        newSource.volume = entry.volume * Mathf.Clamp01(volumeDebuff)* GetSFXVolumeMultiplier(); ;
        newSource.Play();

        innerDict[id] = newSource;

        if (!entry.loop)
            StartCoroutine(DestroyWhenDone(newSource, id,clipKey));
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

    public void Stop(SFXClip key, string id, bool forceEndLoop = false)
    {
        if (!sfxSourceDict.TryGetValue(key, out var innerDict)) return;
        if (!innerDict.TryGetValue(id, out var source) || source == null) return;

        if (source.loop)
        {
            source.loop = false;
            if (forceEndLoop)
            {
                source.Stop();
            }
        }
        else
        {
            source.Stop();
        }

        StartCoroutine(DestroyWhenDone(source, id, key));
    }

    private IEnumerator DestroyWhenDone(AudioSource source, string id ,SFXClip key)
    {
        yield return new WaitForSeconds(source.clip.length);

        if (sfxSourceDict.TryGetValue(key, out var innerDict)
            && innerDict.TryGetValue(id, out var currentSource) && currentSource == source)
        {
            innerDict.Remove(id);
            if (innerDict.Count == 0)
            {
                sfxSourceDict.Remove(key);
            }
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

    public void SetBGMVolume(float masterVolume, float bgmVolume)
    {
        foreach (var pair in bgmDict)
        {
            if (bgmSourceDict.TryGetValue(pair.Key, out var source))
            {
                float baseVolume = pair.Value.volume;
                source.volume = baseVolume * bgmVolume * masterVolume;
            }
        }

        foreach (var pair in whiteNoiseDict)
        {
            if (whiteNoiseSourceDict.TryGetValue(pair.Key, out var source))
            {
                float baseVolume = pair.Value.volume;
                source.volume = baseVolume * bgmVolume * masterVolume;
            }
        }
    }

    public void SetSFXVolume(float masterVolume, float sfxVolume)
    {
        foreach (var clipPair in sfxSourceDict)
        {
            var clipKey = clipPair.Key;

            // 获取每个 SFXClip 的基础 volume（配置）
            if (!sfxDict.TryGetValue(clipKey, out var entry)) continue;
            float baseVolume = entry.volume;

            foreach (var instancePair in clipPair.Value)
            {
                var source = instancePair.Value;
                if (source == null) continue;

                source.volume = baseVolume * sfxVolume * masterVolume;
            }
        }
    }

    public void StopAllVFX(Scene oldScene, Scene newScene)
    {
        // 拷贝所有 key 防止迭代中修改字典
        var allKeys = sfxSourceDict.Keys.ToList();

        foreach (var clipKey in allKeys)
        {
            var idDict = sfxSourceDict[clipKey];
            var allIds = idDict.Keys.ToList();

            foreach (var id in allIds)
            {
                Stop(clipKey, id, forceEndLoop: true);
            }
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        PlayerPrefs.SetFloat(MASTER_KEY, value);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public float GetBGMVolumeMultiplier()
    {
        return Mathf.Clamp01(masterVolume * musicVolume);
    }

    public float GetSFXVolumeMultiplier()
    {
        return Mathf.Clamp01(masterVolume * sfxVolume);
    }

    private void ApplyVolumes()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(masterVolume, musicVolume);
            AudioManager.Instance.SetSFXVolume(masterVolume, sfxVolume);
        }
    }

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
}

public enum BGMClip
{
    SceneBegin,
    Scene1,
    Scene2,
    Scene3,
    EndScene
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