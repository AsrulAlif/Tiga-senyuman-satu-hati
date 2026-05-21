using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class ResponsiveCanvasScaler : MonoBehaviour
{
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;
    private const float ReferenceAspect = ReferenceWidth / ReferenceHeight;

    private int lastScreenWidth;
    private int lastScreenHeight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (FindObjectOfType<ResponsiveCanvasScaler>() != null)
            return;

        GameObject scaler = new GameObject(nameof(ResponsiveCanvasScaler));
        DontDestroyOnLoad(scaler);
        scaler.AddComponent<ResponsiveCanvasScaler>();
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Apply();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
            return;

        Apply();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply();
    }

    private void Apply()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float currentAspect = (float)Screen.width / Screen.height;
        float match = currentAspect >= ReferenceAspect ? 1f : 0f;

        foreach (CanvasScaler scaler in FindObjectsOfType<CanvasScaler>(true))
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = match;
        }
    }
}
