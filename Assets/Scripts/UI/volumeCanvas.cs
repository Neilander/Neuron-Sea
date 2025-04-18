using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class volumeCanvas : MonoBehaviour
{
    public static volumeCanvas Instance { get; private set; }

    [Header("Slider References")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Canvas ControlCanvas;
    public GameObject panel;
    public GameObject blocker;

    private const string MASTER_KEY = "MasterVolume";
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 防止重复
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }

    void Start()
    {
        // 初始化 slider 值（读取存储）
        float masterVolume = PlayerPrefs.GetFloat(MASTER_KEY, 100f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 100f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 100f);

        masterSlider.value = masterVolume;
        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        // 注册监听
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // 也可以主动触发一次设置逻辑（可选）
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    void SetMasterVolume(float value)
    {
        PlayerPrefs.SetFloat(MASTER_KEY, value);
        // TODO: 未来接入 Wwise
        // AkSoundEngine.SetRTPCValue("MasterVolume", value);
        Debug.Log($"[Volume] MasterVolume = {value}");
    }

    void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        // TODO: 未来接入 Wwise
        // AkSoundEngine.SetRTPCValue("MusicVolume", value);
        Debug.Log($"[Volume] MusicVolume = {value}");
    }

    void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat(SFX_KEY, value);
        // TODO: 未来接入 Wwise
        // AkSoundEngine.SetRTPCValue("SFXVolume", value);
        Debug.Log($"[Volume] SFXVolume = {value}");
    }

    public void CloseCanvas()
    {
        ControlCanvas.gameObject.SetActive(false);   
    }

    public void OpenCanvas()
    {
        ControlCanvas.gameObject.SetActive(true);
        panel.SetActive(true);
        blocker.SetActive(true);
    }

    public void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll();
    }
}
