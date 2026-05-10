using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class OpeningAnimation : MonoBehaviour
{
    [Header("Gambar Jatuh")]
    public RectTransform[] fallingImages;   // 4 gambar jatuh
    public float fallDuration = 1f;         // durasi jatuh
    public float bounceHeight = 30f;        // tinggi mantul
    public float bounceDuration = 0.5f;     // durasi mantul

    [Header("Gambar Pop Up")]
    public RectTransform[] popUpImages;     // 2 gambar pop up
    public float popUpDuration = 0.5f;      // durasi pop up

    [Header("Scene Transition")]
    public float delayBeforeLoad = 4f;      // jeda sebelum pindah scene
    public string nextSceneName = "MainMenu"; // nama scene berikutnya (optional)

    [Header("Audio")]
    public AudioClip backgroundMusic;       // file audio background
    private AudioSource audioSource;        // komponen audio

    [Header("Testing / Debug")]
    public bool forcePlayAnimation = false; // jika true, animasi akan selalu diputar

    void Start()
    {
        // 🔊 Siapkan audio source untuk musik background
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;            // biar suaranya nge-loop
        audioSource.playOnAwake = false;    // tidak langsung main sebelum diatur
        audioSource.volume = 0.5f;          // volume bisa diatur (0–1)

        if (backgroundMusic != null)
            audioSource.Play();

        // 🧩 Reset PlayerPrefs jika force play diaktifkan
        if (forcePlayAnimation)
        {
            PlayerPrefs.DeleteKey("OpeningPlayed");
            Debug.Log("Force Play diaktifkan — animasi akan diputar ulang.");
        }

        // 🔁 Cek apakah animasi sudah pernah dimainkan
        if (PlayerPrefs.GetInt("OpeningPlayed", 0) == 1)
        {
            Debug.Log("Opening sudah pernah dimainkan — lewati animasi.");
            return;
        }

        // 🚫 Sembunyikan semua gambar di awal
        foreach (RectTransform img in fallingImages)
            if (img != null)
                img.gameObject.SetActive(false);

        foreach (RectTransform img in popUpImages)
            if (img != null)
                img.gameObject.SetActive(false);

        // ▶️ Mainkan animasi pertama kali
        StartCoroutine(PlayOpening());
    }

    IEnumerator PlayOpening()
    {
        Debug.Log("Memulai animasi opening...");

        // 1️⃣ Gambar jatuh satu per satu
        foreach (RectTransform img in fallingImages)
        {
            if (img == null) continue;

            img.gameObject.SetActive(true);

            Vector3 targetPos = img.anchoredPosition;
            Vector3 startPos = targetPos + new Vector3(0, Screen.height + 200, 0);
            img.anchoredPosition = startPos;

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / fallDuration;
                img.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            Vector3 bouncePos = targetPos + new Vector3(0, bounceHeight, 0);
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / bounceDuration;
                img.anchoredPosition = Vector3.Lerp(targetPos, bouncePos, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            img.anchoredPosition = targetPos;
            yield return new WaitForSeconds(0.2f);
        }

        // 2️⃣ Gambar pop-up
        foreach (RectTransform img in popUpImages)
        {
            if (img == null) continue;

            img.gameObject.SetActive(true);
            img.localScale = Vector3.zero;

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / popUpDuration;
                img.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }

            img.localScale = Vector3.one;
            yield return new WaitForSeconds(0.3f);
        }

        // 3️⃣ Tandai bahwa animasi sudah pernah dimainkan
        PlayerPrefs.SetInt("OpeningPlayed", 1);
        PlayerPrefs.Save();

        // 4️⃣ Tunggu sebentar lalu (opsional) pindah scene berikutnya
        yield return new WaitForSeconds(delayBeforeLoad);

        // 🔇 Hentikan musik saat berpindah scene (jika diinginkan)
        if (audioSource.isPlaying)
            audioSource.Stop();

        // Pindah scene otomatis (opsional)
        // SceneManager.LoadScene(nextSceneName);
    }
}
