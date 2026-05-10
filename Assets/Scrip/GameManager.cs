using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BalloonSequence
{
    public string name = "Round";
    public List<string> colors = new List<string>();
    public float previewDelay = 1.0f;
    public float timeForRound = 20f;

    // ✅ TAMBAHAN BARU
    [Header("Visual Effect")]
    public bool randomizePreviewTextColor = false;
}

public class GameManager : MonoBehaviour
{
    [Header("Buttons (assign di Inspector)")]
    public Button redButton;
    public Button yellowButton;
    public Button greenButton;
    public Button blueButton;

    [Header("UI Elements")]
    public Text previewText;
    public Text infoText;
    public Text timerText;
    public Text scoreText;

    [Header("Gameplay Settings")]
    public List<BalloonSequence> sequences = new List<BalloonSequence>();
    public float defaultTimePerRound = 30f;
    public int pointsPerCorrect = 10;

    [Header("SFX Audio")]
    public AudioSource sfxSource;
    public AudioClip previewSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    [Header("BGM (Backsound Music)")]
    public AudioSource bgmSource;
    public AudioClip bgmClip;
    public bool playBgmOnStart = true;

    [Header("Scene Transition")]
    public string inGameSceneName = "IN GAME";

    [Header("Integration Settings")]
    public string returnDialogueID = "festival_after_game";

    // ------------------------
    private int roundIndex = 0;
    private BalloonSequence currentSeq;
    private int playerIndex = 0;
    private float timer = 0f;
    private bool acceptingInput = false;
    private bool roundActive = false;
    private int score = 0;

    void Start()
    {
        redButton.onClick.AddListener(() => OnBalloonPressed("Merah", redButton));
        yellowButton.onClick.AddListener(() => OnBalloonPressed("Kuning", yellowButton));
        greenButton.onClick.AddListener(() => OnBalloonPressed("Hijau", greenButton));
        blueButton.onClick.AddListener(() => OnBalloonPressed("Biru", blueButton));

        UpdateScoreText();

        if (playBgmOnStart)
            PlayBGM();

        StartCoroutine(StartRoundCoroutine());
    }

    void Update()
    {
        if (roundActive && acceptingInput)
        {
            timer -= Time.deltaTime;

            if (timerText != null)
                timerText.text = "Waktu: " + Mathf.Ceil(timer).ToString();

            if (timer <= 0f)
            {
                GameOver("⏰ Waktu habis!");
            }
        }
    }

    // ============================================================
    // 🔹 BGM CONTROL
    void PlayBGM()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    // ============================================================
    IEnumerator StartRoundCoroutine()
    {
        if (roundIndex >= sequences.Count)
        {
            infoText.text = "🎉 Semua ronde selesai! Skor akhir: " + score;
            previewText.text = "";
            DisableAllButtons();

            StopBGM();

            yield return new WaitForSeconds(2.5f);

            PlayerPrefs.SetInt("ReturnFromMiniGame", 1);
            PlayerPrefs.SetInt("LastMiniGameScore", score);
            PlayerPrefs.SetString("ReturnAfterMiniGameID", returnDialogueID);
            PlayerPrefs.Save();

            SceneManager.LoadScene(inGameSceneName);
            yield break;
        }

        currentSeq = sequences[roundIndex];

        if (currentSeq == null || currentSeq.colors.Count == 0)
        {
            infoText.text = "Round kosong, lanjut...";
            roundIndex++;
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(StartRoundCoroutine());
            yield break;
        }

        acceptingInput = false;
        roundActive = false;
        DisableAllButtons();

        infoText.text = "Round " + (roundIndex + 1) + ": Ingat-ingat urutan nya!";
        previewText.text = "";

        foreach (string c in currentSeq.colors)
        {
            previewText.text = c;

            // ✅ RANDOM WARNA TEXT
            if (currentSeq.randomizePreviewTextColor && previewText != null)
            {
                previewText.color = GetRandomColor();
            }
            else if (previewText != null)
            {
                previewText.color = Color.white; // default
            }

            yield return StartCoroutine(BouncePreview());
            PlaySFX(previewSound);
            yield return new WaitForSeconds(Mathf.Max(0.2f, currentSeq.previewDelay));
        }

        previewText.text = "";
        previewText.color = Color.white;

        yield return new WaitForSeconds(0.25f);

        playerIndex = 0;
        timer = (currentSeq.timeForRound > 0f) ? currentSeq.timeForRound : defaultTimePerRound;

        infoText.text = "Tekan warna sesuai urutan!";
        acceptingInput = true;
        roundActive = true;
        EnableAllButtons();
    }

    // ============================================================
    // 🔹 RANDOM COLOR FUNCTION
    Color GetRandomColor()
    {
        int rand = Random.Range(0, 4);

        switch (rand)
        {
            case 0: return Color.red;
            case 1: return Color.yellow;
            case 2: return Color.green;
            case 3: return Color.blue;
        }

        return Color.white;
    }

    // ============================================================
    void OnBalloonPressed(string color, Button btn)
    {
        if (!acceptingInput || currentSeq == null) return;
        if (playerIndex >= currentSeq.colors.Count) return;

        string expected = currentSeq.colors[playerIndex].Trim().ToLower();
        string picked = color.Trim().ToLower();

        StartCoroutine(FlashButton(btn));

        if (picked == expected)
        {
            score += pointsPerCorrect;
            UpdateScoreText();
            PlaySFX(correctSound);

            playerIndex++;

            if (playerIndex >= currentSeq.colors.Count)
            {
                acceptingInput = false;
                roundActive = false;
                infoText.text = "✅ Benar! Round " + (roundIndex + 1) + " selesai.";
                DisableAllButtons();
                roundIndex++;
                StartCoroutine(NextRoundDelay());
            }
            else
            {
                infoText.text = "👍 Benar! Lanjutkan...";
            }
        }
        else
        {
            PlaySFX(wrongSound);
            GameOver("❌ Salah urutan! Game Over.");
        }
    }

    IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(1.0f);
        StartCoroutine(StartRoundCoroutine());
    }

    IEnumerator FlashButton(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            Color orig = img.color;
            img.color = Color.white;
            yield return new WaitForSeconds(0.18f);
            img.color = orig;
        }
    }

    IEnumerator BouncePreview()
    {
        previewText.transform.localScale = Vector3.zero;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            previewText.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        previewText.transform.localScale = Vector3.one;
    }

    // ============================================================
    void GameOver(string message)
    {
        acceptingInput = false;
        roundActive = false;
        DisableAllButtons();
        infoText.text = message;

        StopBGM();

        StartCoroutine(ReturnToInGameAfterDelay());
    }

    IEnumerator ReturnToInGameAfterDelay()
    {
        yield return new WaitForSeconds(2.5f);

        PlayerPrefs.SetInt("ReturnFromMiniGame", 1);
        PlayerPrefs.SetInt("LastMiniGameScore", score);
        PlayerPrefs.SetString("ReturnAfterMiniGameID", returnDialogueID);
        PlayerPrefs.Save();

        SceneManager.LoadScene(inGameSceneName);
    }

    // ============================================================
    void DisableAllButtons()
    {
        redButton.interactable = false;
        yellowButton.interactable = false;
        greenButton.interactable = false;
        blueButton.interactable = false;
    }

    void EnableAllButtons()
    {
        redButton.interactable = true;
        yellowButton.interactable = true;
        greenButton.interactable = true;
        blueButton.interactable = true;
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}