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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"我是{gameObject.name}，我被删除了，挤占我的是{Instance.gameObject.name}");
            Destroy(gameObject); // 防止重复
            return;
        }

        Instance = this;
    }

    void Start()
    {
        // 初始化 slider 值，直接从 AudioManager 获取
        masterSlider.value = AudioManager.Instance.GetMasterVolume();
        musicSlider.value = AudioManager.Instance.GetMusicVolume();
        sfxSlider.value = AudioManager.Instance.GetSFXVolume();

        masterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }

#if UNITY_EDITOR
    public void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("MasterVolume", 1);
        PlayerPrefs.SetFloat("MusicVolume", 1);
        PlayerPrefs.SetFloat("SFXVolume", 1);
        PlayerPrefs.Save();
    }
#endif
}
