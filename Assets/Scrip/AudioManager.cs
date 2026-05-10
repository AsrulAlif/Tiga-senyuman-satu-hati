using UnityEngine;
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

    private bool isPaused = false;

    private void Awake()
    {
        // Pastikan hanya ada satu AudioManager di seluruh game
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
}
