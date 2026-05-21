using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonClickSfx : MonoBehaviour, IPointerClickHandler
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button == null || !button.interactable)
            return;

        if (IsMiniGameScene())
            return;

        if (AudioManager.instance != null)
            AudioManager.instance.PlayButtonClickSound();
    }

    private bool IsMiniGameScene()
    {
        string sceneName = SceneManager.GetActiveScene().name.Replace(" ", "");
        return sceneName.IndexOf("MiniGame", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
