using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AutoDialogueController : MonoBehaviour
{
    [Header("Reference")]
    public DialogueSystem dialogueSystem;
    public Button autoButton;

    [Header("Auto Settings")]
    public float autoDelay = 0.8f; // jeda antar dialog
    public float autoTypingSpeed = 0.015f; // lebih cepat dari normal

    private bool autoMode = false;
    private float normalTypingSpeed;

    void Start()
    {
        if (dialogueSystem != null)
        {
            normalTypingSpeed = dialogueSystem.typingSpeed;
        }

        autoButton.onClick.AddListener(ToggleAutoMode);
    }

    void ToggleAutoMode()
    {
        autoMode = !autoMode;

        if (autoMode)
        {
            dialogueSystem.typingSpeed = autoTypingSpeed;
            StartCoroutine(AutoRun());
        }
        else
        {
            dialogueSystem.typingSpeed = normalTypingSpeed;
            StopAllCoroutines();
        }
    }

    IEnumerator AutoRun()
    {
        while (autoMode)
        {
            yield return new WaitForSeconds(autoDelay);

            // cek apakah pilihan dialog muncul
            if (IsChoiceActive())
            {
                StopAuto();
                yield break;
            }

            // klik tombol next otomatis
            if (dialogueSystem.nextButton.gameObject.activeInHierarchy)
            {
                dialogueSystem.nextButton.onClick.Invoke();
            }
        }
    }

    bool IsChoiceActive()
    {
        foreach (Button btn in dialogueSystem.choiceButtons)
        {
            if (btn.gameObject.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    void StopAuto()
    {
        autoMode = false;
        dialogueSystem.typingSpeed = normalTypingSpeed;
        StopAllCoroutines();
    }
}