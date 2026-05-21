using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("🎵 BGM Settings")]
    public AudioSource bgmSource;
    public float defaultVolume = 0.8f;

    [Header("⚙️ UI Settings (Optional)")]
    public GameObject optionsPanel;
    public Slider bgmSlider;
    public Button resumeButton;

    [Header("Button SFX")]
    [Tooltip("Isi dengan suara klik tombol. Kosongkan kalau tidak ingin memakai efek tombol.")]
    public AudioClip buttonClickSound;
    [Range(0f, 1f)]
    public float buttonClickVolume = 1f;

    private bool isPaused = false;
    private AudioSource sfxSource;

    private void Awake()
    {
        // Pastikan hanya ada satu AudioManager di seluruh game
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupButtonSfxSource();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Setup BGM looping
        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.volume = PlayerPrefs.GetFloat("BGMVolume", defaultVolume);
            bgmSource.Play();
        }

        // Setup UI jika ada
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (bgmSlider != null)
        {
            bgmSlider.value = bgmSource.volume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        AddButtonSfxToSceneButtons();
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        // Tekan ESC untuk membuka/tutup menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptions();
        }
    }

    public void ToggleOptions()
    {
        if (optionsPanel == null) return;

        isPaused = !isPaused;
        optionsPanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        if (optionsPanel == null) return;

        isPaused = false;
        optionsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
            PlayerPrefs.SetFloat("BGMVolume", volume);
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickSound == null)
            return;

        if (sfxSource == null)
            SetupButtonSfxSource();

        sfxSource.PlayOneShot(buttonClickSound, buttonClickVolume);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddButtonSfxToSceneButtons();
    }

    private void SetupButtonSfxSource()
    {
        sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
    }

    private void AddButtonSfxToSceneButtons()
    {
        if (IsMiniGameScene())
            return;

        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            if (button != null && button.GetComponent<ButtonClickSfx>() == null)
                button.gameObject.AddComponent<ButtonClickSfx>();
        }
    }

    private bool IsMiniGameScene()
    {
        string sceneName = SceneManager.GetActiveScene().name.Replace(" ", "");
        return sceneName.IndexOf("MiniGame", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
