using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class PhoneAnimator : MonoBehaviour
{
    [Header("Referensi")]
    // Seret GameObject yang memiliki MoodManager ke sini
    public MoodManager moodManager; 
    // Seret GameObject 'moodPanel' (RectTransform) dari MoodManager ke sini
    public RectTransform phoneRectTransform; 

    [Header("Pengaturan Animasi")]
    public float animationDuration = 0.5f;   // Durasi animasi (detik)
    public float startRotationZ = 30f;       // Kemiringan awal (derajat Z)
    public float startOffset = 500f;         // Jarak awal dari posisi akhir (pixel/unit Canvas)
    
    // Kurva untuk mengontrol kecepatan (Easing). Atur di Inspector (misalnya EaseOutQuad)
    public AnimationCurve positionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); 
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Posisi dan Rotasi akhir (diambil dari posisi RectTransform di Scene)
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private bool isAnimating = false;
    
    // =========================================================
    // UNITY MONOBEHAVIOUR METHODS
    // =========================================================

    void Awake()
    {
        // Mengambil posisi dan rotasi target (posisi akhir panel)
        if (phoneRectTransform != null)
        {
            // Menggunakan AnchoredPosition3D untuk UI (Panel)
            targetPosition = phoneRectTransform.anchoredPosition3D; 
            targetRotation = phoneRectTransform.localRotation;
        }
        else
        {
            Debug.LogError("Phone RectTransform belum diset di PhoneAnimator.");
        }
    }

    // =========================================================
    // FUNGSI UTAMA TOGGLE
    // =========================================================

    // Dipanggil dari tombol UI
    public void AnimateAndTogglePhone()
    {
        if (isAnimating) return; 
        
        if (moodManager != null && moodManager.moodPanel != null)
        {
            if (!moodManager.moodPanel.activeSelf)
            {
                // Animasi Masuk: Dari kiri miring ke tengah lurus
                StartCoroutine(AnimateInCoroutine());
            }
            else 
            {
                // Animasi Keluar: Dari tengah lurus ke kanan bawah
                StartCoroutine(AnimateOutCoroutine());
            }
        }
    }

    // =========================================================
    // FUNGSI ANIMASI MASUK (Miring lalu Lurus)
    // =========================================================

    private IEnumerator AnimateInCoroutine()
    {
        isAnimating = true;

        // Tentukan Posisi dan Rotasi Awal
        Vector3 startPosition = targetPosition + Vector3.left * startOffset;
        Quaternion startRot = Quaternion.Euler(0, 0, startRotationZ);

        // Terapkan kondisi awal
        phoneRectTransform.localRotation = startRot;
        phoneRectTransform.anchoredPosition3D = startPosition;

        // Aktifkan panel dan inisialisasi UI
        moodManager.ToggleMoodPanel(); 

        float startTime = Time.time;
        float elapsed = 0f;

        // Loop Animasi
        while (elapsed < animationDuration)
        {
            elapsed = Time.time - startTime;
            float t = Mathf.Clamp01(elapsed / animationDuration); // Nilai 0.0 sampai 1.0

            // Apply Easing (Kurva)
            float posCurveValue = positionCurve.Evaluate(t);
            float rotCurveValue = rotationCurve.Evaluate(t);

            // Pergerakan Posisi
            phoneRectTransform.anchoredPosition3D = Vector3.Lerp(startPosition, targetPosition, posCurveValue); 
            
            // Pergerakan Rotasi
            phoneRectTransform.localRotation = Quaternion.Lerp(startRot, targetRotation, rotCurveValue);

            yield return null;
        }
        
        // Pastikan posisi dan rotasi berakhir tepat di target
        phoneRectTransform.anchoredPosition3D = targetPosition;
        phoneRectTransform.localRotation = targetRotation;

        isAnimating = false;
    }
    
    // =========================================================
    // FUNGSI ANIMASI KELUAR
    // =========================================================
    
    private IEnumerator AnimateOutCoroutine()
    {
        isAnimating = true;
        float duration = animationDuration * 0.7f; // Animasi keluar lebih cepat
        
        Vector3 currentPosition = phoneRectTransform.anchoredPosition3D;
        Quaternion currentRotation = phoneRectTransform.localRotation;

        // Posisi Akhir Animasi Keluar (ke kanan bawah)
        Vector3 endPosition = targetPosition + (Vector3.right + Vector3.down) * startOffset / 2f;
        Quaternion endRot = Quaternion.Euler(0, 0, -15f); // Miring ke arah berlawanan

        float startTime = Time.time;
        float elapsed = 0f;

        // Loop Animasi
        while (elapsed < duration)
        {
            elapsed = Time.time - startTime;
            float t = Mathf.Clamp01(elapsed / duration); // Nilai 0.0 sampai 1.0

            // Apply Easing/Curve
            float curveValue = positionCurve.Evaluate(t); 

            // Lerp Posisi
            phoneRectTransform.anchoredPosition3D = Vector3.Lerp(currentPosition, endPosition, curveValue); 
            
            // Lerp Rotasi
            phoneRectTransform.localRotation = Quaternion.Lerp(currentRotation, endRot, rotationCurve.Evaluate(t));

            yield return null;
        }
        
        // Matikan panel HP setelah animasi keluar
        moodManager.ToggleMoodPanel();
        
        // Reset posisi dan rotasi agar siap untuk animasi masuk berikutnya
        phoneRectTransform.anchoredPosition3D = targetPosition;
        phoneRectTransform.localRotation = targetRotation;
        
        isAnimating = false;
    }
}