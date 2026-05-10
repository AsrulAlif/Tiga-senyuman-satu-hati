using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Video;

// =========================================================
// ENUM Heroine (DIHAPUS DARI SINI: Sudah ada di MoodManager.cs)
// =========================================================
// C# akan menggunakan definisi dari file MoodManager.cs
// Jika Anda mendapatkan Error, pastikan kedua file ada di folder 'Scripts' yang sama
// atau tambahkan 'using MoodSystemNamespace;' jika Anda menggunakan Namespace.

// =========================================================
// STRUKTUR DATA DIALOGUE
// =========================================================

[System.Serializable]
public class DialogueChoice
{
    [Header("Pilihan Dialog")]
    public string choiceText;
    [Tooltip("ID Dialog selanjutnya yang akan dipanggil.")]
    public string nextDialogueID;

    [Header("Perubahan Nilai Mood")]
    [Tooltip("Karakter mana yang mood-nya akan diubah (None = tidak berubah).")]
    public Heroine moodTargetHeroine = Heroine.None; 
    [Range(-50f, 50f)] public float moodChangeAmount; // Menggantikan moodChange lama
    [Range(-80000f, 80000f)] public float moneyChange;

    [Header("Aksi Khusus")]
    [Tooltip("Jika True, akan memicu transisi bulanan (Month) setelah pilihan ini.")]
    public bool endOfMonth;

    [Header("Transisi Normal")]
    [Tooltip("Jika True, akan memicu transisi fade dengan teks tertentu.")]
    public bool triggerNormalTransition = false;
    [TextArea(1, 3)] public string normalTransitionText = "Beberapa saat kemudian...";

    [Header("Persyaratan Mood")]
    [Tooltip("Karakter mana yang mood-nya akan diperiksa (None = tidak ada persyaratan mood).")]
    public Heroine requiredMoodHeroine = Heroine.None; // Karakter yang menjadi persyaratan
    [Tooltip("Persentase mood minimum (0-100) yang diperlukan untuk memilih opsi ini.")]
    [Range(0f, 100f)] public float requiredMoodPercent = 0f;
    [Tooltip("Pesan yang ditampilkan jika mood tidak mencukupi.")]
    [TextArea(1, 3)] public string moodInsufficientMessage = "Mood kamu kurang untuk melakukan ini.";

    [Header("MiniGame")]
    public bool loadMiniGameScene = false;

    [Tooltip("Nama Scene MiniGame yang akan di-load (harus sesuai di Build Settings).")]
    public string miniGameSceneName; // 🔥 BARU

    [Tooltip("ID Dialog yang akan dipanggil setelah kembali dari MiniGame.")]
    public string returnDialogueAfterMiniGame;
}

[System.Serializable]
public class DialogueStep
{
    [Header("Informasi Kalimat")]
    [TextArea(2, 5)] public string sentenceText;
    public AudioClip voiceClip;
    
    [Header("Tampilan Karakter & Nama")]
    [Tooltip("Kunci (CharacterNameKey) dari karakter yang sedang berbicara (untuk Nama & Icon).")]
    public string speakingCharacterKey = ""; 
    
    [Tooltip("Kunci (CharacterNameKey) dari karakter yang akan ditampilkan di layar (bisa lebih dari satu, pisahkan dengan koma).")]
    public string activeCharacterKeys = ""; 
    
    public Sprite iconSprite;
    public Sprite backgroundImage;
    [Tooltip("Jika true, background akan diubah jika berbeda dari saat ini.")]
    public bool changeBackground = false; 
}

[System.Serializable]
public class CharacterDisplayData
{
    [Tooltip("Kunci Unik. Contoh: 'MC', 'NPC_A', 'Narrator'.")]
    public string characterNameKey; 
    
    [Tooltip("Nama yang akan ditampilkan di UI. Contoh: 'Pemain', 'Alia', 'Narator'.")]
    public string displayName;
    
    public GameObject characterObject; 
    [HideInInspector] public CanvasGroup canvasGroup; 
}

[System.Serializable]
public class DialogueLine
{
    [Tooltip("ID unik untuk dialog ini. Contoh: start, chapter2, scene_a.")]
    public string dialogueID;
    
    [Header("Langkah Dialog")]
    public DialogueStep[] dialogueSteps;
    
    [Header("Pilihan")]
    public DialogueChoice[] choices;
}

// =========================================================
// Kelas Dialogue System UTAMA (REVISI)
// =========================================================

public class DialogueSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public Text characterNameText;
    public Text dialogueText;
    [Tooltip("Daftar semua karakter di scene, harus diisi agar nama dan gambar bisa muncul.")]
    public CharacterDisplayData[] allCharacters; 
    public Image iconImageUI;
    public Image backgroundImageUI;
    public Button[] choiceButtons;
    public Button nextButton;
    public Text notificationText;

public string mainMenuSceneName = "MainMenu"; // nama scene menu
private bool isEndingTriggered = false; 

    [Header("SYSTEM REFERENCES")] // BARU: Tambahkan referensi ke MoodManager
    [Tooltip("Seret script MoodManager di sini.")]
    public MoodManager moodManager; 

    [Header("Dialogue Data (Array In-Scene)")]
    [Tooltip("Daftar semua dialog yang akan di-load langsung di Inspector.")]
    public DialogueLine[] dialogueLines;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Audio")]
    public AudioSource voiceAudioSource;
    public AudioSource bgmAudioSource;
    [Tooltip("Daftar BGM berdasarkan indeks bulan.")]
    public AudioClip[] monthBGMs;

    [Header("Money System")]
    public Text moneyText;
    public float currentMoney = 20000f;
    public float maxMoney = 999999f;
    public float moneyLerpSpeed = 5f;

    [Header("Calendar System")]
    public Text monthText;
    public int currentMonth = 9;

    [Header("Transition Settings")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Transition Text Settings")]
    public Text transitionText;
    public string[] monthTransitionTexts;
    public float textFadeSpeed = 1f;
    public Vector3 textPosition = new Vector3(0, 0, 0);

    [Header("Money Popup Settings")]
    public float popupScale = 1.3f;
    public float popupDuration = 0.25f;

    // Internal
    private DialogueLine currentDialogue;
    private int currentSentenceIndex = 0;
    private bool showingChoices = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Coroutine moneyCoroutine;
    private Coroutine popupCoroutine;
    private Coroutine notificationCoroutine; 
    private Vector3 defaultMoneyScale;
    private Sprite currentBackground; 

    private bool isChoiceProcessing = false;

    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string LAST_DIALOGUE_KEY = "LastDialogueID";
    private const string LAST_MONEY_KEY = "LastMoney";
    private const string LAST_MONTH_KEY = "LastMonth";
    
    private Dictionary<string, CharacterDisplayData> characterMap = new Dictionary<string, CharacterDisplayData>();
    private Color defaultChoiceTextColor = Color.black;
    private float t;

    void Awake()
    {
        if (allCharacters == null || allCharacters.Length == 0)
        {
            Debug.LogError("ERROR: 'All Characters' array di Inspector belum diisi! Tidak ada karakter yang bisa ditampilkan.");
            return;
        }

        foreach (var charData in allCharacters)
        {
            if (charData.characterObject != null)
            {
                charData.canvasGroup = charData.characterObject.GetComponent<CanvasGroup>();
                if (charData.canvasGroup == null)
                {
                    charData.canvasGroup = charData.characterObject.AddComponent<CanvasGroup>();
                }
                charData.canvasGroup.alpha = 0f; 
                charData.characterObject.SetActive(true); 
                characterMap[charData.characterNameKey.Trim()] = charData; 
            }
            else
            {
                Debug.LogWarning($"PERINGATAN: Character Object untuk '{charData.characterNameKey}' tidak diisi di Inspector.");
            }
        }
        Debug.Log($"INFO: Berhasil mendaftarkan {characterMap.Count} karakter ke dalam system.");

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }

        if (choiceButtons != null && choiceButtons.Length > 0 && choiceButtons[0] != null)
        {
            Text btnText = choiceButtons[0].GetComponentInChildren<Text>();
            if (btnText != null)
            {
                defaultChoiceTextColor = btnText.color; 
            }
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                 if (choiceButtons[i] != null) choiceButtons[i].onClick.RemoveAllListeners();
            }
        }
    }

    void Start()
    {
        if (nextButton == null)
        {
            Debug.LogError("ERROR: Next Button UI tidak terhubung di Inspector!");
            return;
        }

        // Cek MoodManager (REVISI)
        if (moodManager == null)
        {
            moodManager = FindObjectOfType<MoodManager>();
            if (moodManager == null)
            {
                Debug.LogError("ERROR: MoodManager tidak terhubung di Inspector atau tidak ditemukan di Scene! Sistem Mood Karakter tidak akan berfungsi.");
            } else {
                // Pastikan mood dimuat saat DialogueSystem dimulai
                moodManager.LoadMoods(); 
                Debug.Log("INFO: MoodManager ditemukan dan Mood Heroine dimuat.");
            }
        }

        StartCoroutine(BounceButton(nextButton));
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(ShowNextSentence);

        string startID = "start";
        string targetID = startID;
        
        // Logika loading progress
        if (PlayerPrefs.HasKey("ReturnFromMiniGame") && PlayerPrefs.GetInt("ReturnFromMiniGame") == 1)
        {
            // Kembali dari Mini Game
            PlayerPrefs.SetInt("ReturnFromMiniGame", 0);
            targetID = PlayerPrefs.GetString("ReturnAfterMiniGameID", startID);
            Debug.Log($"INFO: Kembali dari MiniGame, melanjutkan ke ID: '{targetID}'");

            int miniScore = PlayerPrefs.GetInt("LastMiniGameScore", 0);
            if (miniScore > 0)
            {
                currentMoney = Mathf.Clamp(currentMoney + miniScore * 100, 0f, maxMoney);
                ShowNotification($"Anda mendapatkan Rp. {miniScore * 100:n0} dari MiniGame!", 3f);
            }
            PlayerPrefs.DeleteKey("LastMiniGameScore");
            PlayerPrefs.DeleteKey("ReturnAfterMiniGameID");
        }
        else if (PlayerPrefs.HasKey(LAST_DIALOGUE_KEY))
        {
            // Melanjutkan dialog terakhir
            targetID = PlayerPrefs.GetString(LAST_DIALOGUE_KEY, startID);
            Debug.Log($"INFO: Melanjutkan dari save terakhir, ID: '{targetID}'");
        }
        else
        {
            Debug.Log($"INFO: Memulai dialog baru dari awal, ID: '{targetID}'");
        }

        // Load data dari PlayerPrefs
        currentMoney = PlayerPrefs.GetFloat(LAST_MONEY_KEY, currentMoney);
        currentMonth = PlayerPrefs.GetInt(LAST_MONTH_KEY, currentMonth);

        ShowDialogueByID(targetID);

        if (moneyText != null)
            defaultMoneyScale = moneyText.transform.localScale;

        UpdateMoneyUIInstant();
        UpdateMonthUI();

        if (fadeImage != null)
        {
            Color initialColor = fadeImage.color;
            initialColor.a = 1f;
            fadeImage.color = initialColor;
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(Fade(1f, 0f, fadeDuration));
        }
        else
        {
            Debug.LogError("ERROR: Fade Image tidak diassign di Inspector!");
        }

        PlayMonthBGM(currentMonth);

        if (transitionText != null)
        {
            transitionText.gameObject.SetActive(false);
            transitionText.color = new Color(1, 1, 1, 0);
            transitionText.rectTransform.localPosition = textPosition;
        }
    }

    private string ReplacePlayerName(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        string playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "Pemain"); 
        if (string.IsNullOrEmpty(playerName)) playerName = "Pemain";

        input = input.Replace("Pemain", playerName)
                     .Replace("{player}", playerName)
                     .Replace("{Player}", playerName)
                     .Replace("{PLAYER}", playerName);
        return input;
    }

    private void PlayMonthBGM(int monthIndex)
    {
        if (bgmAudioSource == null || monthBGMs == null || monthBGMs.Length == 0) return;
        
        int index = Mathf.Clamp(monthIndex, 0, monthBGMs.Length - 1);
        AudioClip clip = monthBGMs[index];
        
        if (clip != null && bgmAudioSource.clip != clip)
        {
            bgmAudioSource.Stop();
            bgmAudioSource.clip = clip;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }
    }

    public void ShowDialogueByID(string id)
    {
        isChoiceProcessing = false;
        Debug.Log($"ACTION: Mencoba menampilkan dialog dengan ID: '{id}'");
        currentDialogue = FindDialogueByID(id);
        if (currentDialogue == null)
        {
            Debug.LogError($"ERROR: Dialogue ID '{id}' TIDAK DITEMUKAN! Pastikan ID ada di array Dialogue Lines di Inspector.");
            EndDialogue();
            return;
        }
        if (currentDialogue.dialogueSteps == null || currentDialogue.dialogueSteps.Length == 0)
        {
            Debug.LogWarning($"WARNING: Dialogue ID '{id}' ditemukan, tapi tidak memiliki langkah dialog (Dialogue Steps). Memanggil EndDialogue.");
            EndDialogue();
            return;
        }

        // Simpan progress
        PlayerPrefs.SetString(LAST_DIALOGUE_KEY, id);
        PlayerPrefs.SetFloat(LAST_MONEY_KEY, currentMoney);
        PlayerPrefs.SetInt(LAST_MONTH_KEY, currentMonth);
        PlayerPrefs.Save();
        
        if (moodManager != null) moodManager.SaveMoods(); // BARU: Simpan mood multi-karakter

        currentSentenceIndex = 0;
        showingChoices = false;

        if (dialogueText != null) dialogueText.gameObject.SetActive(true);
        if (characterNameText != null) characterNameText.gameObject.SetActive(true);

        if (choiceButtons != null)
        {
            foreach (var btn in choiceButtons)
                if (btn != null) btn.gameObject.SetActive(false);
        }

        PlaySentence(currentSentenceIndex); 
        
        UpdateUI(); 
        UpdateMoneyUIInstant();
    }

    private DialogueLine FindDialogueByID(string id)
    {
        foreach (var line in dialogueLines)
        {
            if (line.dialogueID.Equals(id, System.StringComparison.OrdinalIgnoreCase)) return line;
        }
        return null;
    }

    public void ShowNextSentence()
    {
        if (showingChoices) return;

        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            voiceAudioSource.Stop();

        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (currentDialogue != null && currentDialogue.dialogueSteps.Length > currentSentenceIndex)
            {
                string full = ReplacePlayerName(currentDialogue.dialogueSteps[currentSentenceIndex].sentenceText);
                if (dialogueText != null) dialogueText.text = full;
            }
            isTyping = false;
            UpdateUI(); 
            return;
        }

        if (currentDialogue == null) return;

        if (currentSentenceIndex < currentDialogue.dialogueSteps.Length - 1)
        {
            currentSentenceIndex++;
            PlaySentence(currentSentenceIndex);
        }
        else
        {
            if (currentDialogue.choices != null && currentDialogue.choices.Length > 0)
                ShowChoices();
            else
                EndDialogue();
        }

        UpdateUI();
    }
    
    // FUNGSI BANTU BARU: Mengambil mood karakter yang spesifik
    private float GetHeroineMood(Heroine heroine) 
    {
        if (moodManager == null || heroine == Heroine.None) return 100f; 
        
        switch (heroine)
        {
            case Heroine.Hikari: return moodManager.GetHikariMood();
            case Heroine.Yumi: return moodManager.GetYumiMood();
            case Heroine.Miyu: return moodManager.GetMiyuMood();
            default: return 100f; 
        }
    }


    private void ShowChoices()
    {
        showingChoices = true;
        ClearCharacterDisplay();

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (currentDialogue.choices != null && i < currentDialogue.choices.Length)
            {
                int choiceIndex = i;
                DialogueChoice choice = currentDialogue.choices[choiceIndex]; 
                Button btn = choiceButtons[i];
                if (btn == null) continue;

                btn.gameObject.SetActive(true);

                // LOGIKA MOOD: Persyaratan
                float heroineCurrentMood = GetHeroineMood(choice.requiredMoodHeroine);
                bool moodIsSufficient = choice.requiredMoodHeroine == Heroine.None || heroineCurrentMood >= choice.requiredMoodPercent;

                string replacedChoiceText = ReplacePlayerName(choice.choiceText);
                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    if (!moodIsSufficient)
                    {
                        string moodHeroineName = choice.requiredMoodHeroine.ToString();
                        // Tampilkan pesan peringatan mood di tombol
                        btnText.text = $"{choiceIndex + 1}. [Mood {moodHeroineName} Kurang ({heroineCurrentMood:F0}%) | Min: {choice.requiredMoodPercent:F0}%] {replacedChoiceText}"; 
                        btnText.color = Color.gray; 
                    }
                    else
                    {
                        btnText.text = $"{choiceIndex + 1}. {replacedChoiceText}";
                        btnText.color = defaultChoiceTextColor; 
                    }
                }
                
                btn.interactable = moodIsSufficient;
                
                btn.onClick.RemoveAllListeners();

                if (moodIsSufficient)
                {
                    void ExecuteChoiceAction(DialogueChoice choiceData)
                    {
                        // LOGIKA MOOD: Panggil MoodManager.ChangeMood
                        if (choiceData.moodTargetHeroine != Heroine.None && moodManager != null)
                        {
                            Debug.Log($"ACTION: Pilihan ini mengubah mood {choiceData.moodTargetHeroine} sebesar {choiceData.moodChangeAmount}.");
                            moodManager.ChangeMood(choiceData.moodTargetHeroine, choiceData.moodChangeAmount); 
                        } else if (choiceData.moodTargetHeroine != Heroine.None)
                        {
                            Debug.LogWarning($"PERINGATAN: MoodManager null, tidak bisa mengubah mood {choiceData.moodTargetHeroine}.");
                        }

                        ApplyMoneyChange(choiceData.moneyChange);

                        if (choiceData.endOfMonth)
                        {
                            StartCoroutine(FadeTransition(choiceData.nextDialogueID));
                            return; 
                        }

                        if (choiceData.triggerNormalTransition)
                        {
                            StartCoroutine(NormalTransition(choiceData.nextDialogueID, choiceData.normalTransitionText));
                            return; 
                        }

if (choiceData.loadMiniGameScene)
{
    if (string.IsNullOrEmpty(choiceData.miniGameSceneName))
    {
        Debug.LogError("ERROR: MiniGame Scene Name belum diisi!");
        return;
    }

    PlayerPrefs.SetInt("ReturnFromMiniGame", 1);
    PlayerPrefs.SetString(LAST_DIALOGUE_KEY, currentDialogue.dialogueID);
    PlayerPrefs.SetFloat(LAST_MONEY_KEY, currentMoney);
    PlayerPrefs.SetInt(LAST_MONTH_KEY, currentMonth);
    PlayerPrefs.SetString("ReturnAfterMiniGameID", choiceData.returnDialogueAfterMiniGame);

    moodManager.SaveMoods();
    PlayerPrefs.Save();

    Debug.Log($"INFO: Load MiniGame Scene: {choiceData.miniGameSceneName}");

    SceneManager.LoadScene(choiceData.miniGameSceneName); // 🔥 DINAMIS
    return;
}
                        
                        ShowDialogueByID(choiceData.nextDialogueID);
                    }

                    btn.onClick.AddListener(() =>
{
    if (isChoiceProcessing) return; // 🔥 cegah spam

    isChoiceProcessing = true;

    // 🔥 disable semua tombol choice
    foreach (var b in choiceButtons)
    {
        if (b != null)
            b.interactable = false;
    }

    ExecuteChoiceAction(choice);
});
                }
                else
                {
                    string notification = ReplacePlayerName(choice.moodInsufficientMessage);
                    btn.onClick.AddListener(() => ShowNotification(notification));
                }

                StartCoroutine(BounceButton(btn));
            }
            else
            {
                if (choiceButtons[i] != null) choiceButtons[i].gameObject.SetActive(false);
            }
        }

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
    }
    
    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationText == null) return;
        
        if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
        
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        notificationCoroutine = StartCoroutine(HideNotificationAfterTime(duration));
    }

    private IEnumerator HideNotificationAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }


private IEnumerator FadeTransition(string nextDialogueID)
{
    StopAllDialogueAudioAndTyping();
    yield return StartCoroutine(Fade(0f, 1f, fadeDuration)); 

    ClearCharacterDisplay();

    // Kurangi bulan dulu sebelum cek status END
    if (currentMonth > 0) 
    {
        currentMonth--;
    }

    UpdateMonthUI();

    // CEK APAKAH SUDAH END
    if (currentMonth <= 0 && !isEndingTriggered)
    {
        StartCoroutine(TriggerEnding());
        yield break; // Berhenti di sini, jangan lanjut ke dialog berikutnya
    }

    // Jika belum END, lanjut tampilkan teks transisi bulan
    if (transitionText != null)
    {
        transitionText.gameObject.SetActive(true);
        string mText = (currentMonth < monthTransitionTexts.Length && currentMonth >= 0)
            ? monthTransitionTexts[currentMonth]
            : "Waktu Berlalu...";
        transitionText.text = ReplacePlayerName(mText);

        yield return StartCoroutine(FadeText(0f, 1f));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(FadeText(1f, 0f));
        transitionText.gameObject.SetActive(false);
    }

    PlayMonthBGM(currentMonth);
    ShowDialogueByID(nextDialogueID);

    yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
}

private IEnumerator TriggerEnding()
{
    isEndingTriggered = true;
    Debug.Log("INFO: Memicu Ending Cutscene...");

    SceneManager.LoadScene("EndingScene"); // 🔥 GANTI KE SCENE VIDEO
    yield break;
}

    private IEnumerator NormalTransition(string nextDialogueID, string transitionTextToShow)
    {
        StopAllDialogueAudioAndTyping();
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration)); 

        ClearCharacterDisplay();

        if (transitionText != null)
        {
            transitionText.gameObject.SetActive(true);
            transitionText.text = string.IsNullOrEmpty(transitionTextToShow) 
                ? "Beberapa saat kemudian..." 
                : ReplacePlayerName(transitionTextToShow); 

            yield return StartCoroutine(FadeText(0f, 1f));
            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(FadeText(1f, 0f));
            transitionText.gameObject.SetActive(false);
        }
        
        ShowDialogueByID(nextDialogueID);

        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
    }


    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null) 
        {
            Debug.LogError("ERROR: Fade Image belum diisi di Inspector!");
            yield break;
        }
        
        fadeImage.gameObject.SetActive(true); 
        
        float time = 0f;
        Color color = fadeImage.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, time / duration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, to);
        
        if (to < 0.01f)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }


    private IEnumerator FadeText(float from, float to)
    {
        if (transitionText == null) yield break;
        float time = 0f;
        Color color = transitionText.color;

        while (time < 1f)
        {
            time += Time.deltaTime * textFadeSpeed;
            float alpha = Mathf.Lerp(from, to, time);
            transitionText.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        transitionText.color = new Color(color.r, color.g, color.b, to);
    }

    private void UpdateMonthUI()
    {
        if (monthText != null)
        {
            monthText.text = (currentMonth > 0) ? $"Month: {currentMonth}" : "END";
        }
    }

    private void ApplyMoneyChange(float delta)
    {
        currentMoney = Mathf.Clamp(currentMoney + delta, 0f, maxMoney);
        UpdateMoneyUISmooth();

        if (popupCoroutine != null) StopCoroutine(popupCoroutine);
        popupCoroutine = StartCoroutine(MoneyPopupEffect());
    }

    private IEnumerator MoneyPopupEffect()
    {
        if (moneyText == null) yield break;
        Vector3 enlarged = defaultMoneyScale * popupScale;
        float time = 0f;

        while (time < popupDuration)
        {
            time += Time.deltaTime;
            moneyText.transform.localScale = Vector3.Lerp(defaultMoneyScale, enlarged, time / popupDuration);
            yield return null;
        }

        time = 0f;
        while (time < popupDuration)
        {
            t += Time.deltaTime;
            moneyText.transform.localScale = Vector3.Lerp(enlarged, defaultMoneyScale, time / popupDuration);
            yield return null;
        }

        moneyText.transform.localScale = defaultMoneyScale;
    }

    private void UpdateMoneyUIInstant()
    {
        if (moneyText != null)
            moneyText.text = $"Rp. {currentMoney:n0}";
    }

    private void UpdateMoneyUISmooth()
    {
        if (moneyCoroutine != null) StopCoroutine(moneyCoroutine);
        moneyCoroutine = StartCoroutine(LerpMoneyText(currentMoney));
    }

    private IEnumerator LerpMoneyText(float targetValue)
    {
        if (moneyText == null) yield break;
        
        float startValue = 0f;
        string existing = moneyText.text.Replace("Rp. ", "").Replace(".", "").Replace(",", "");
        
        if (!float.TryParse(existing, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.InvariantCulture, out startValue))
        {
            startValue = 0f; 
        }

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * moneyLerpSpeed;
            float currentValue = Mathf.Lerp(startValue, targetValue, time);
            if (moneyText != null)
                moneyText.text = $"Rp. {currentValue:n0}";
            yield return null;
        }

        if (moneyText != null)
            moneyText.text = $"Rp. {targetValue:n0}";
    }

    private void UpdateUI()
    {
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(!showingChoices); 
        }
    }

    private void EndDialogue()
    {
        StopAllDialogueAudioAndTyping();

        if (dialogueText != null) dialogueText.text = "";
        if (characterNameText != null) characterNameText.text = "";
        
        ClearCharacterDisplay();

        if (iconImageUI != null) iconImageUI.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(true);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (characterNameText != null) characterNameText.gameObject.SetActive(false);
        
        if (choiceButtons != null)
        {
            foreach (var btn in choiceButtons) if (btn != null) btn.gameObject.SetActive(false);
        }

        Debug.Log("INFO: Dialogue Selesai. Mencapai EndDialogue.");

        UpdateMoneyUIInstant();
    }
    
    private void ClearCharacterDisplay()
    {
        if (characterMap == null) return;
        foreach (var pair in characterMap)
        {
            StartCoroutine(FadeCharacterImage(pair.Value.canvasGroup, 0f));
        }
    }

    private void PlaySentence(int index)
    {
        if (currentDialogue == null || currentDialogue.dialogueSteps == null) return;
        if (index < 0 || index >= currentDialogue.dialogueSteps.Length) return;

        var stepData = currentDialogue.dialogueSteps[index];
        Debug.Log($"--- MEMULAI KALIMAT ---");
        Debug.Log($"INFO: Indeks Kalimat: {index}");

        // 1. Update Character Display dan Nama
        UpdateCharacterDisplay(stepData.activeCharacterKeys, stepData.speakingCharacterKey);

        if (characterNameText != null)
        {
            string speakingNameKey = stepData.speakingCharacterKey.Trim();
            Debug.Log($"INFO: Mencari nama untuk kunci '{speakingNameKey}'");
            
            string displayName = "";
            if (characterMap.ContainsKey(speakingNameKey))
            {
                displayName = characterMap[speakingNameKey].displayName;
                Debug.Log($"SUKSES: Kunci ditemukan. Nama tampilan: '{displayName}'");
            }
            else
            {
                displayName = speakingNameKey; // Fallback
                Debug.LogError($"GAGAL: Kunci '{speakingNameKey}' TIDAK DITEMUKAN di Character Map! Menggunakan kunci sebagai nama. Pastikan 'Character Name Key' di DialogueStep cocok dengan yang di All Characters.");
            }
            
            characterNameText.text = ReplacePlayerName(displayName); 
        }
        else
        {
            Debug.LogError("ERROR: Character Name Text UI tidak diassign di Inspector!");
        }
        
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);
        if (characterNameText != null) characterNameText.gameObject.SetActive(true);

        // 2. Update Icon
        if (iconImageUI != null)
        {
            if (stepData.iconSprite != null)
            {
                iconImageUI.sprite = stepData.iconSprite;
                iconImageUI.gameObject.SetActive(true);
            }
            else iconImageUI.gameObject.SetActive(false);
        }

        // 3. Update Background 
        if (backgroundImageUI != null)
        {
            if (stepData.changeBackground && stepData.backgroundImage != null)
            {
                if (backgroundImageUI.sprite != stepData.backgroundImage)
                {
                    backgroundImageUI.sprite = stepData.backgroundImage;
                    currentBackground = stepData.backgroundImage;
                }
                backgroundImageUI.gameObject.SetActive(true);
            }
            else if(currentBackground != null) 
            {
                backgroundImageUI.sprite = currentBackground;
                backgroundImageUI.gameObject.SetActive(true);
            }
            else 
            {
                backgroundImageUI.gameObject.SetActive(false);
            }
        }

        // 4. Mulai Ketik Kalimat
        string sentenceToShow = ReplacePlayerName(stepData.sentenceText);
        Debug.Log($"INFO: Teks dialog yang akan ditampilkan: '{sentenceToShow}'");
        StartTyping(sentenceToShow);

        // 5. Play Voice Clip
        if (voiceAudioSource != null)
        {
            voiceAudioSource.Stop();
            if (stepData.voiceClip != null)
            {
                voiceAudioSource.clip = stepData.voiceClip;
                voiceAudioSource.Play();
            }
        }
    }

    private void UpdateCharacterDisplay(string activeKeys, string speakingKey)
    {
        if (characterMap == null || characterMap.Count == 0)
        {
            Debug.LogWarning("PERINGATAN: Character Map kosong, tidak bisa mengupdate tampilan karakter.");
            return;
        }

        Debug.Log($"INFO: Memperbarui tampilan karakter. Kunci Aktif: '{activeKeys}'");

        HashSet<string> activeKeySet = new HashSet<string>();
        foreach(string key in activeKeys.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            activeKeySet.Add(key.Trim());
        }

        foreach (var pair in characterMap)
        {
            string key = pair.Key;
            CharacterDisplayData charData = pair.Value;
            float targetAlpha = 0f;

            if (activeKeySet.Contains(key))
            {
                targetAlpha = 1f;
                Debug.Log($"INFO: Karakter dengan kunci '{key}' akan DITAMPILKAN (Alpha: 1).");
            }
            else
            {
                 Debug.Log($"INFO: Karakter dengan kunci '{key}' akan DISEMBUNYIKAN (Alpha: 0).");
            }

            if (charData.canvasGroup != null && Mathf.Abs(charData.canvasGroup.alpha - targetAlpha) > 0.01f)
            {
                StartCoroutine(FadeCharacterImage(charData.canvasGroup, targetAlpha));
            }
        }
    }


    private void StopAllDialogueAudioAndTyping()
    {
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
        }

        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            isTyping = false;
        }
    }

    private void StartTyping(string sentence)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        UpdateUI(); 
    }

    private IEnumerator FadeCharacterImage(CanvasGroup cg, float targetAlpha)
    {
        if (cg == null) yield break;
        float duration = 0.5f; 
        float time = 0f;
        float startAlpha = cg.alpha;

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;
        cg.blocksRaycasts = targetAlpha > 0.01f;
        cg.interactable = targetAlpha > 0.01f;
    }

    private IEnumerator BounceButton(Button btn)
    {
        if (btn == null) yield break;
        Vector3 originalScale = btn.transform.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        float duration = 0.5f;

        while (btn.gameObject.activeSelf)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (btn != null)
                    btn.transform.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
                yield return null;
            }

            t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (btn != null)
                    btn.transform.localScale = Vector3.Lerp(targetScale, originalScale, t / duration);
                yield return null;
            }
        }

        if (btn != null) btn.transform.localScale = originalScale;
    }
    
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(LAST_DIALOGUE_KEY);
        PlayerPrefs.DeleteKey(LAST_MONEY_KEY);
        PlayerPrefs.DeleteKey(LAST_MONTH_KEY);
        PlayerPrefs.DeleteKey("ReturnAfterMiniGameID");
        PlayerPrefs.DeleteKey("LastMiniGameScore");
        PlayerPrefs.DeleteKey(PLAYER_NAME_KEY);
        
        // Panggil MoodManager untuk mereset mood (Delete PlayerPrefs keys)
        PlayerPrefs.DeleteKey(MoodManager.HIKARI_MOOD_KEY); 
        PlayerPrefs.DeleteKey(MoodManager.YUMI_MOOD_KEY); 
        PlayerPrefs.DeleteKey(MoodManager.MIYU_MOOD_KEY); 
        
        PlayerPrefs.Save();

        Debug.Log("INFO: Progress dialogue dihapus (ResetProgress).");
    }

    internal void StartDialogue(DialogueLine newLine)
    {
        throw new NotImplementedException();
    }

    
}