using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseSystem : MonoBehaviour
{
    [Header("UI Pause")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button backToMenuButton;

    [Header("Volume Settings")]
    public Slider bgmSlider;
    public Slider voiceSlider;

    [Header("CHEAT SYSTEM 🔥")]
    public Button cheatToggleButton;     // tombol ON/OFF
    public GameObject cheatPanel;        // panel input cheat
    public InputField dialogueIDInput;   // input text
    public Button goToDialogueButton;    // tombol GO

    private bool isPaused = false;
    private bool cheatActive = false;

    private DialogueSystem dialogueSystem;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (bgmSlider != null) bgmSlider.gameObject.SetActive(false);
        if (voiceSlider != null) voiceSlider.gameObject.SetActive(false);
        if (cheatPanel != null) cheatPanel.SetActive(false);

        dialogueSystem = FindObjectOfType<DialogueSystem>();

        // tombol
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (backToMenuButton != null) backToMenuButton.onClick.AddListener(BackToMenu);

        // cheat
        if (cheatToggleButton != null) cheatToggleButton.onClick.AddListener(ToggleCheat);
        if (goToDialogueButton != null) goToDialogueButton.onClick.AddListener(JumpToDialogue);

        // volume
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            if (dialogueSystem != null && dialogueSystem.bgmAudioSource != null)
                bgmSlider.value = dialogueSystem.bgmAudioSource.volume;
        }

        if (voiceSlider != null)
        {
            voiceSlider.onValueChanged.AddListener(SetVoiceVolume);
            if (dialogueSystem != null && dialogueSystem.voiceAudioSource != null)
                voiceSlider.value = dialogueSystem.voiceAudioSource.volume;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);

        if (bgmSlider != null) bgmSlider.gameObject.SetActive(true);
        if (voiceSlider != null) voiceSlider.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);

        if (bgmSlider != null) bgmSlider.gameObject.SetActive(false);
        if (voiceSlider != null) voiceSlider.gameObject.SetActive(false);

        if (cheatPanel != null) cheatPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // ==================== CHEAT SYSTEM ====================

    void ToggleCheat()
    {
        cheatActive = !cheatActive;

        if (cheatPanel != null)
            cheatPanel.SetActive(cheatActive);

        Debug.Log("Cheat Mode: " + (cheatActive ? "ON" : "OFF"));
    }

    void JumpToDialogue()
    {
        if (!cheatActive) return;

        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem tidak ditemukan!");
            return;
        }

        string id = dialogueIDInput.text;

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Dialogue ID kosong!");
            return;
        }

        Debug.Log("Cheat lompat ke Dialogue ID: " + id);

        Time.timeScale = 1f;
        ResumeGame();

        dialogueSystem.ShowDialogueByID(id);
    }

    // ==================== VOLUME ====================

    public void SetBGMVolume(float value)
    {
        if (dialogueSystem != null && dialogueSystem.bgmAudioSource != null)
            dialogueSystem.bgmAudioSource.volume = value;
    }

    public void SetVoiceVolume(float value)
    {
        if (dialogueSystem != null && dialogueSystem.voiceAudioSource != null)
            dialogueSystem.voiceAudioSource.volume = value;
    }
}