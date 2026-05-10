using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System; 

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;     
    public GameObject nameInputPanel;    
    public GameObject creditPanel;       

    [Header("Name Input UI")]
    public InputField nameInputField;    
    public Button startButton;           
    public Button cancelNameButton;      

    [Header("Main Menu Buttons")]
    public Button playButton;            
    public Button continueButton;        
    public Button resetButton;           
    public Button creditButton;          
    public Button quitButton;            

    [Header("Credit Panel UI")]
    public Button closeCreditButton;     // Tombol untuk menutup panel Credit

    [Header("Settings")]
    public string inGameSceneName = "IN GAME"; // Nama scene in game

    // Key PlayerPrefs
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string LAST_DIALOGUE_KEY = "LastDialogueID";
    private const string LAST_MOOD_KEY = "LastMood"; 
    private const string LAST_MONEY_KEY = "LastMoney";
    private const string LAST_MONTH_KEY = "LastMonth";

    void Awake()
    {
        // Pastikan panel dimulai dengan status yang benar
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        if (creditPanel != null) creditPanel.SetActive(false);
        
        // >> PERBAIKAN: Hubungkan semua tombol saat scene dimuat <<
        InitializeButtons();

        // Perbarui status tombol Continue berdasarkan save progress
        UpdateContinueButtonState();
    }

    /// <summary>
    /// Menghubungkan semua tombol UI ke fungsi skripnya.
    /// Tambahan: RemoveAllListeners() untuk mencegah event ganda/rusak.
    /// </summary>
    private void InitializeButtons()
    {
        // Main Menu Buttons
        if (playButton != null) { playButton.onClick.RemoveAllListeners(); playButton.onClick.AddListener(OnPlayClicked); }
        if (continueButton != null) { continueButton.onClick.RemoveAllListeners(); continueButton.onClick.AddListener(OnContinueClicked); }
        if (resetButton != null) { resetButton.onClick.RemoveAllListeners(); resetButton.onClick.AddListener(DeleteProgress); }
        if (creditButton != null) { creditButton.onClick.RemoveAllListeners(); creditButton.onClick.AddListener(ShowCreditPanel); }
        if (quitButton != null) { quitButton.onClick.RemoveAllListeners(); quitButton.onClick.AddListener(QuitGame); }
        
        // Name Input Panel Buttons
        if (startButton != null) { startButton.onClick.RemoveAllListeners(); startButton.onClick.AddListener(StartGameFromInput); }
        if (cancelNameButton != null) { cancelNameButton.onClick.RemoveAllListeners(); cancelNameButton.onClick.AddListener(CancelNameInput); }
        
        // Credit Panel Button
        if (closeCreditButton != null) { closeCreditButton.onClick.RemoveAllListeners(); closeCreditButton.onClick.AddListener(HideCreditPanel); } // Fokus Perbaikan
    }
    
    // ======================================================
    // 🔹 Handler Tombol Utama
    
    public void OnPlayClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true); 
            
            if (nameInputField != null)
            {
                // Bersihkan nama jika ini New Game
                if (!PlayerPrefs.HasKey(LAST_DIALOGUE_KEY))
                {
                    nameInputField.text = ""; 
                }
                else
                {
                    nameInputField.text = PlayerPrefs.GetString(PLAYER_NAME_KEY, "");
                }
                // Pastikan kursor aktif di InputField agar bisa langsung mengetik
                nameInputField.Select();
                nameInputField.ActivateInputField(); 
            }
        }
    }
    
    public void OnContinueClicked()
    {
        StartCoroutine(LoadSceneAsync(inGameSceneName));
    }
    
    public void ShowCreditPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditPanel != null) creditPanel.SetActive(true);
    }
    
    public void HideCreditPanel()
    {
        // PERBAIKAN: Memastikan Credit Panel disembunyikan dan Main Menu ditampilkan
        if (creditPanel != null) creditPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // ======================================================
    // 🔹 Handler Input Nama
    
    public void StartGameFromInput()
    {
        if (nameInputField == null || string.IsNullOrWhiteSpace(nameInputField.text))
        {
            Debug.LogWarning("Nama pemain tidak boleh kosong!");
            return;
        }

        string playerName = nameInputField.text.Trim();
        
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
        
        // Mulai game baru, hapus progress lama
        DeleteProgress();
        
        PlayerPrefs.Save();
        
        StartCoroutine(LoadSceneAsync(inGameSceneName));
    }

    public void CancelNameInput()
    {
        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // ======================================================
    // 🔹 Tombol Reset Game
    
    public void DeleteProgress()
    {
        // Hapus semua data progress kecuali nama pemain
        PlayerPrefs.DeleteKey(LAST_DIALOGUE_KEY);
        PlayerPrefs.DeleteKey(LAST_MOOD_KEY);
        PlayerPrefs.DeleteKey(LAST_MONEY_KEY);
        PlayerPrefs.DeleteKey(LAST_MONTH_KEY);
        PlayerPrefs.DeleteKey("ReturnAfterMiniGameID");
        PlayerPrefs.DeleteKey("LastMiniGameScore");
        PlayerPrefs.DeleteKey("ReturnFromMiniGame");
        
        // Hapus kunci mood spesifik (Membutuhkan MoodManager.cs agar public const string keys dapat diakses)
        try
        {
            PlayerPrefs.DeleteKey(MoodManager.HIKARI_MOOD_KEY); 
            PlayerPrefs.DeleteKey(MoodManager.YUMI_MOOD_KEY); 
            PlayerPrefs.DeleteKey(MoodManager.MIYU_MOOD_KEY); 
        }
        catch (Exception e)
        {
            PlayerPrefs.DeleteKey("HikariMood"); 
            PlayerPrefs.DeleteKey("YumiMood"); 
            PlayerPrefs.DeleteKey("MiyuMood"); 
            Debug.LogError($"Gagal mereset Mood: Error {e.Message}");
        }

        PlayerPrefs.Save();

        UpdateContinueButtonState();
        Debug.Log("✅ Progress pemain berhasil dihapus.");
    }

    // ======================================================
    // 🔹 Cek & ubah status tombol Continue
    private void UpdateContinueButtonState()
    {
        if (continueButton != null)
        {
            bool hasProgress = PlayerPrefs.HasKey(LAST_DIALOGUE_KEY);
            continueButton.interactable = hasProgress;
        }
    }

    // ======================================================
    // 🔹 Keluar Game
    public void QuitGame()
    {
        Debug.Log("Keluar dari game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // ======================================================
    // 🔹 Fungsi Loading Scene Async
    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}