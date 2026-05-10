using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    [Header("Panel UI")]
    [Tooltip("Panel opsi yang berisi slider volume dan tombol resume.")]
    public GameObject optionPanel;

    [Header("Audio Settings")]
    [Tooltip("AudioSource untuk BGM (format mp3 didukung).")]
    public AudioSource bgmAudioSource;

    [Tooltip("Slider UI untuk mengatur volume BGM.")]
    public Slider bgmSlider;

    // --- Variabel internal ---
    private bool isPaused = false; // Status pause game
    private const string BGM_PREF_KEY = "BGMVolume"; // Kunci penyimpanan PlayerPrefs

    void Start()
    {
        // Pastikan panel opsi dan slider dalam keadaan tertutup saat mulai
        if (optionPanel != null)
            optionPanel.SetActive(false);
        if (bgmSlider != null)
            bgmSlider.gameObject.SetActive(false);

        // Inisialisasi nilai volume
        if (bgmAudioSource != null && bgmSlider != null)
        {
            // Ambil volume tersimpan, jika belum ada gunakan volume default AudioSource
            float savedVolume = PlayerPrefs.GetFloat(BGM_PREF_KEY, bgmAudioSource.volume);

            // Terapkan volume ke AudioSource dan Slider
            bgmAudioSource.volume = savedVolume;
            bgmSlider.value = savedVolume;

            // Tambahkan listener supaya saat slider digerakkan, fungsi SetBGMVolume dipanggil
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        else
        {
            Debug.LogWarning("⚠️ Harap isi bgmAudioSource dan bgmSlider di Inspector!");
        }
    }

    void Update()
    {
        // Tekan tombol "O" atau "Escape" untuk membuka/tutup panel opsi
        if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // -------------------------------
    // Fungsi untuk toggle pause/resume
    // -------------------------------
    public void TogglePause()
    {
        // Balikkan status pause
        isPaused = !isPaused;

        // Jika sedang pause, buka panel opsi
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    // -------------------------------
    // Fungsi untuk menghentikan game
    // -------------------------------
    private void PauseGame()
    {
        if (optionPanel != null)
            optionPanel.SetActive(true); // Tampilkan panel opsi
        if (bgmSlider != null)
            bgmSlider.gameObject.SetActive(true); // Tampilkan slider

        Time.timeScale = 0f; // Hentikan waktu dalam game (pause)
        AudioListener.pause = false; // Pastikan audio tetap bisa diatur
        Debug.Log("⏸️ Game di-pause. Panel opsi dan slider muncul.");
    }

    // -------------------------------
    // Fungsi untuk melanjutkan game
    // -------------------------------
    public void ResumeGame()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false); // Sembunyikan panel opsi
        if (bgmSlider != null)
            bgmSlider.gameObject.SetActive(true); // Sembunyikan slider

        Time.timeScale = 1f; // Lanjutkan waktu dalam game
        isPaused = false;
        Debug.Log("▶️ Game dilanjutkan. Panel opsi dan slider ditutup.");
    }

    // -------------------------------
    // Fungsi untuk mengatur volume BGM
    // -------------------------------
    public void SetBGMVolume(float volume)
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = volume;

            // Simpan nilai ke PlayerPrefs agar tetap tersimpan saat game dibuka lagi
            PlayerPrefs.SetFloat(BGM_PREF_KEY, volume);
            PlayerPrefs.Save();
        }
    }
}
