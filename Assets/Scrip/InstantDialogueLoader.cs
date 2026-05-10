using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InstantDialogueLoader : MonoBehaviour
{
    public DialogueSystem dialogueSystem;

    public void LoadMonth2Start()
    {
        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem belum di assign!");
            return;
        }

        // 🔥 BUAT DIALOG BARU
        DialogueLine newLine = new DialogueLine
        {
            dialogueID = "Bulan 2 yumi",

            dialogueSteps = new DialogueStep[]
            {
                new DialogueStep { sentenceText = "Hari kedua di SMA 3 Gerberas dimulai dengan suasana yang masih terasa asing...", speakingCharacterKey = "Narrator", activeCharacterKeys = "" },
                new DialogueStep { sentenceText = "Namun entah kenapa, aku merasa hari ini akan sedikit berbeda.", speakingCharacterKey = "MC", activeCharacterKeys = "MC" },

                new DialogueStep { sentenceText = "Woy! Pagi bro!", speakingCharacterKey = "Basrul", activeCharacterKeys = "MC,Basrul" },
                new DialogueStep { sentenceText = "Gimana hari pertama lo kemarin?", speakingCharacterKey = "Basrul", activeCharacterKeys = "MC,Basrul" },

                new DialogueStep { sentenceText = "Lumayan... walaupun masih agak canggung.", speakingCharacterKey = "MC", activeCharacterKeys = "MC,Basrul" },
                new DialogueStep { sentenceText = "Hahaha wajar lah! Tapi tenang, di sini banyak cewek menarik loh~", speakingCharacterKey = "Basrul", activeCharacterKeys = "MC,Basrul" },

                new DialogueStep { sentenceText = "Kemarin gue udah bilang kan, ada tiga cewek yang paling menonjol di sekolah ini.", speakingCharacterKey = "Basrul", activeCharacterKeys = "MC,Basrul" },

                new DialogueStep { sentenceText = "Yang pertama... si Yumi.", speakingCharacterKey = "Basrul", activeCharacterKeys = "MC,Basrul" },
                new DialogueStep { sentenceText = "Galak, dingin, jutek... tapi entah kenapa banyak yang suka.", speakingCharacterKey = "Basrul", activeCharacterKeys = "MC,Basrul" },

                new DialogueStep { sentenceText = "Yang kedua Miyu, anak perpustakaan. Pendiem banget.", speakingCharacterKey = "Basrul", activeCharacterKeys = "MC,Basrul" },
                new DialogueStep { sentenceText = "Yang terakhir Hikari... itu sih udah kayak matahari berjalan.", speakingCharacterKey = "Basrul", activeCharacterKeys = "MC,Basrul" },

                new DialogueStep { sentenceText = "Terus... lo tertarik yang mana?", speakingCharacterKey = "Basrul", activeCharacterKeys = "MC,Basrul" },

                new DialogueStep { sentenceText = "Aku terdiam sejenak, memikirkan kata-katanya.", speakingCharacterKey = "Narrator", activeCharacterKeys = "" }
            },

            choices = new DialogueChoice[]
            {
                new DialogueChoice { choiceText = "Yumi...", nextDialogueID = "month2_yumi_first", moodTargetHeroine = Heroine.Yumi, moodChangeAmount = 5 },
                new DialogueChoice { choiceText = "Miyu...", nextDialogueID = "month2_miyu_first", moodTargetHeroine = Heroine.Miyu, moodChangeAmount = 5 },
                new DialogueChoice { choiceText = "Hikari...", nextDialogueID = "month2_hikari_first", moodTargetHeroine = Heroine.Hikari, moodChangeAmount = 5 }
            }
        };

        // 🔥 AMBIL LIST LAMA
        List<DialogueLine> tempList = dialogueSystem.dialogueLines.ToList();

        // 🔥 HAPUS JIKA SUDAH ADA (biar gak duplicate)
        tempList.RemoveAll(d => d.dialogueID == newLine.dialogueID);

        // 🔥 TAMBAH DIALOG BARU
        tempList.Add(newLine);

        // 🔥 MASUKKAN KEMBALI KE SYSTEM
        dialogueSystem.dialogueLines = tempList.ToArray();

        // 🚀 JALANKAN
        dialogueSystem.ShowDialogueByID("month2_start");
    }
}