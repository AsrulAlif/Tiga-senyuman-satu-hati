using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// Enum untuk mengidentifikasi karakter (Sangat Penting)
// Perbaikan: Ganti Heroin menjadi Heroine
public enum Heroine
{
    None, 
    Hikari,
    Yumi,
    Miyu
}

public class MoodManager : MonoBehaviour
{
    // =========================================================
    // VARIABEL UNTUK SIMPAN/MUAT DATA (PlayerPrefs Keys)
    // PERBAIKAN: Diubah dari private menjadi public agar MainMenu/DialogueSystem bisa mengaksesnya
    // =========================================================
    public const string HIKARI_MOOD_KEY = "HikariMood";
    public const string YUMI_MOOD_KEY = "YumiMood";
    public const string MIYU_MOOD_KEY = "MiyuMood";
    private const float DEFAULT_MOOD = 0f; // Mood awal default 0%

    // =========================================================
    // NILAI MOOD SAAT INI (0 - 100)
    // =========================================================
    [Header("Current Mood Values (0 - 100)")]
    [Range(0f, 100f)] [SerializeField] private float hikariMood = DEFAULT_MOOD;
    [Range(0f, 100f)] [SerializeField] private float yumiMood = DEFAULT_MOOD;
    [Range(0f, 100f)] [SerializeField] private float miyuMood = DEFAULT_MOOD;
    
    // Properti publik untuk diakses oleh DialogueSystem
    public float GetHikariMood() => hikariMood;
    public float GetYumiMood() => yumiMood;
    public float GetMiyuMood() => miyuMood;

    // =========================================================
    // REFERENSI UI UNTUK LAYAR HP (REVISI BARU)
    // =========================================================
    [Header("HP Main Panel & Navigasi")]
    public GameObject moodPanel; 
    public GameObject screen1Container;
    public GameObject screen2Container;
    
    [Header("Wallpaper & Background")]
    public Image hpWallpaperImage; 
    public Sprite defaultWallpaper; 
    public Sprite secondaryWallpaper; 

    private int currentScreenIndex = 0; 

    // =========================================================
    // REFERENSI UI KARAKTER
    // =========================================================
    [Header("Hikari UI Display")]
    public Text hikariNameText;
    public Image hikariFillImage;
    public Text hikariValueText;
    public Text hikariStatusText;

    [Header("Yumi UI Display")]
    public Text yumiNameText;
    public Image yumiFillImage;
    public Text yumiValueText;
    public Text yumiStatusText;

    [Header("Miyu UI Display")]
    public Text miyuNameText;
    public Image miyuFillImage;
    public Text miyuValueText;
    public Text miyuStatusText;

    // =========================================================
    // UNITY MONOBEHAVIOUR METHODS
    // =========================================================
    void Awake()
    {
        if (moodPanel != null)
        {
            // PENTING: Matikan moodPanel di awal agar PhoneAnimator bisa mengaktifkannya
            moodPanel.SetActive(false); 
        }
        
        LoadMoods();
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (hikariNameText != null) hikariNameText.text = "Hikari";
        if (yumiNameText != null) yumiNameText.text = "Yumi";
        if (miyuNameText != null) miyuNameText.text = "Miyu";

        UpdateAllMoodUI();
        UpdateScreenDisplay(0); 
    }

    // =========================================================
    // FUNGSI NAVIGASI LAYAR HP
    // =========================================================

    public void ToggleScreen()
    {
        currentScreenIndex = (currentScreenIndex == 0) ? 1 : 0;
        UpdateScreenDisplay(currentScreenIndex);
    }
    
    private void UpdateScreenDisplay(int index)
    {
        if (screen1Container != null)
        {
            screen1Container.SetActive(index == 0);
        }
        if (screen2Container != null)
        {
            screen2Container.SetActive(index == 1);
        }

        if (hpWallpaperImage != null)
        {
            Sprite targetWallpaper = (index == 0) ? defaultWallpaper : secondaryWallpaper;
            if (hpWallpaperImage.sprite != targetWallpaper && targetWallpaper != null)
            {
                hpWallpaperImage.sprite = targetWallpaper;
            }
        }
        
        if (index == 0)
        {
            UpdateAllMoodUI();
        }
    }
    
    // =========================================================
    // FUNGSI UTAMA MOOD DAN UI
    // =========================================================
    
    public void LoadMoods()
    {
        hikariMood = PlayerPrefs.GetFloat(HIKARI_MOOD_KEY, DEFAULT_MOOD);
        yumiMood = PlayerPrefs.GetFloat(YUMI_MOOD_KEY, DEFAULT_MOOD);
        miyuMood = PlayerPrefs.GetFloat(MIYU_MOOD_KEY, DEFAULT_MOOD);
    }

    public void SaveMoods()
    {
        PlayerPrefs.SetFloat(HIKARI_MOOD_KEY, hikariMood);
        PlayerPrefs.SetFloat(YUMI_MOOD_KEY, yumiMood);
        PlayerPrefs.SetFloat(MIYU_MOOD_KEY, miyuMood);
        PlayerPrefs.Save();
    }

    public void ChangeMood(Heroine heroine, float amount)
    {
        switch (heroine)
        {
            case Heroine.Hikari:
                hikariMood = Mathf.Clamp(hikariMood + amount, 0f, 100f);
                break;
            case Heroine.Yumi:
                yumiMood = Mathf.Clamp(yumiMood + amount, 0f, 100f);
                break;
            case Heroine.Miyu:
                miyuMood = Mathf.Clamp(miyuMood + amount, 0f, 100f);
                break;
            case Heroine.None:
                return; 
        }

        UpdateAllMoodUI(); 
        SaveMoods(); 
    }

    public void UpdateAllMoodUI()
    {
        UpdateMoodUI(Heroine.Hikari, hikariMood, hikariFillImage, hikariValueText, hikariStatusText);
        UpdateMoodUI(Heroine.Yumi, yumiMood, yumiFillImage, yumiValueText, yumiStatusText);
        UpdateMoodUI(Heroine.Miyu, miyuMood, miyuFillImage, miyuValueText, miyuStatusText);
    }

    private void UpdateMoodUI(Heroine heroine, float moodValue, Image fillImage, Text valueText, Text statusText)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = moodValue / 100f;
        }

        if (valueText != null)
        {
            valueText.text = $"{moodValue:F0}%";
        }

        if (statusText != null)
        {            
            statusText.text = GetMoodStatusText(heroine, moodValue);
        }
    }

    private string GetMoodStatusText(Heroine heroine, float moodValue)
    {
        switch (heroine)
        {
            case Heroine.Hikari:
                if (moodValue >= 80f) return "Ceria";
                if (moodValue >= 60f) return "Bahagia";
                if (moodValue >= 40f) return "Biasa";
                if (moodValue >= 20f) return "Khawatir";
                return "Putus Asa";
            
            case Heroine.Yumi:
                if (moodValue >= 80f) return "Penuh Cinta";
                if (moodValue >= 60f) return "Rileks";
                if (moodValue >= 40f) return "Netral";
                if (moodValue >= 20f) return "Dingin";
                return "Jauh";
            
            case Heroine.Miyu:
                if (moodValue >= 80f) return "Bergairah";
                if (moodValue >= 60f) return "Motivasi";
                if (moodValue >= 40f) return "Lesu";
                if (moodValue >= 20f) return "Marah";
                return "Meledak";
            
            default:
                return "Unknown";
        }
    }

    // =========================================================
    // FUNGSI TOGGLE UI (Dipanggil oleh PhoneAnimator)
    // =========================================================

    public void ToggleMoodPanel()
    {
        if (moodPanel != null)
        {
            moodPanel.SetActive(!moodPanel.activeSelf);
            
            if (moodPanel.activeSelf)
            {
                currentScreenIndex = 0;
                UpdateScreenDisplay(currentScreenIndex);
                UpdateAllMoodUI();
            }
        }
    }
}