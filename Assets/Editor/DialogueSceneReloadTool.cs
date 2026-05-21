#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public static class DialogueSceneReloadTool
{
    private const string ScenePath = "Assets/Scenes/IN GAME.unity";
    private const int ExpectedDialogueLines = 238;

    [MenuItem("Tools/Dialogue/FORCE Reload IN GAME dialogue from disk")]
    public static void ForceReloadInGameSceneFromDisk()
    {
        string fullPath = Path.GetFullPath(ScenePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError("IN GAME.unity tidak ditemukan di: " + fullPath);
            return;
        }

        string sceneText = File.ReadAllText(fullPath);
        int countOnDisk = Regex.Matches(sceneText, @"^\s*- dialogueID:", RegexOptions.Multiline).Count;

        if (countOnDisk < ExpectedDialogueLines)
        {
            Debug.LogError($"Scene di disk belum lengkap. dialogueID di disk: {countOnDisk}, target: {ExpectedDialogueLines}. Tidak membuka ulang scene.");
            return;
        }

        Debug.Log($"Memaksa reload {ScenePath} dari disk. dialogueID di disk: {countOnDisk}. Jangan save scene lama yang masih 121.");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var dialogueSystem = UnityEngine.Object.FindObjectOfType<DialogueSystem>();
        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem tidak ditemukan setelah reload scene.");
            return;
        }

        int inspectorCount = dialogueSystem.dialogueLines == null ? 0 : dialogueSystem.dialogueLines.Length;
        if (inspectorCount == ExpectedDialogueLines)
        {
            Debug.Log($"BERHASIL: Dialogue Lines di Inspector sekarang {inspectorCount}.");
        }
        else
        {
            Debug.LogError($"Masih belum cocok. Inspector: {inspectorCount}, disk: {countOnDisk}. Tutup Unity tanpa save, lalu buka ulang project.");
        }
    }

    [MenuItem("Tools/Dialogue/SAFE Fix Indonesian Dialogue Text")]
    public static void SafeFixIndonesianDialogueText()
    {
        string fullPath = Path.GetFullPath(ScenePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError("IN GAME.unity tidak ditemukan di: " + fullPath);
            return;
        }

        string sceneText = File.ReadAllText(fullPath);
        int countOnDisk = Regex.Matches(sceneText, @"^\s*- dialogueID:", RegexOptions.Multiline).Count;
        if (countOnDisk != ExpectedDialogueLines)
        {
            Debug.LogError($"Batal koreksi. File disk harus {ExpectedDialogueLines} dialogueID, tapi sekarang {countOnDisk}.");
            return;
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var dialogueSystem = UnityEngine.Object.FindObjectOfType<DialogueSystem>();
        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem tidak ditemukan. Batal koreksi.");
            return;
        }

        int beforeCount = dialogueSystem.dialogueLines == null ? 0 : dialogueSystem.dialogueLines.Length;
        if (beforeCount != ExpectedDialogueLines)
        {
            Debug.LogError($"Batal koreksi. Inspector masih membaca {beforeCount}, bukan {ExpectedDialogueLines}. Jangan save scene ini.");
            return;
        }

        string backupPath = fullPath + ".before-indonesian-text-fix.bak";
        File.Copy(fullPath, backupPath, true);

        int changed = 0;
        foreach (DialogueLine line in dialogueSystem.dialogueLines)
        {
            if (line == null)
            {
                continue;
            }

            if (line.dialogueSteps != null)
            {
                foreach (DialogueStep step in line.dialogueSteps)
                {
                    if (step == null)
                    {
                        continue;
                    }

                    string fixedSentenceText = FixIndonesianText(step.sentenceText);
                    if (fixedSentenceText != step.sentenceText)
                    {
                        step.sentenceText = fixedSentenceText;
                        changed++;
                    }
                }
            }

            if (line.choices != null)
            {
                foreach (DialogueChoice choice in line.choices)
                {
                    if (choice == null)
                    {
                        continue;
                    }

                    string fixedChoiceText = FixIndonesianText(choice.choiceText);
                    if (fixedChoiceText != choice.choiceText)
                    {
                        choice.choiceText = fixedChoiceText;
                        changed++;
                    }

                    string fixedNormalTransitionText = FixIndonesianText(choice.normalTransitionText);
                    if (fixedNormalTransitionText != choice.normalTransitionText)
                    {
                        choice.normalTransitionText = fixedNormalTransitionText;
                        changed++;
                    }

                    string fixedMoodInsufficientMessage = FixIndonesianText(choice.moodInsufficientMessage);
                    if (fixedMoodInsufficientMessage != choice.moodInsufficientMessage)
                    {
                        choice.moodInsufficientMessage = fixedMoodInsufficientMessage;
                        changed++;
                    }
                }
            }
        }

        int afterCount = dialogueSystem.dialogueLines == null ? 0 : dialogueSystem.dialogueLines.Length;
        if (afterCount != ExpectedDialogueLines)
        {
            Debug.LogError($"Batal save. Jumlah Dialogue Lines berubah dari {beforeCount} ke {afterCount}. Backup ada di: {backupPath}");
            return;
        }

        EditorUtility.SetDirty(dialogueSystem);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

        Debug.Log($"BERHASIL koreksi teks Indonesia. Dialogue Lines tetap {afterCount}. Field teks berubah: {changed}. Backup: {backupPath}");
    }

    private static string FixIndonesianText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        string result = text;
        bool wrappedInQuotes = result.Length >= 2 && result[0] == '"' && result[result.Length - 1] == '"';
        if (wrappedInQuotes)
        {
            result = result.Substring(1, result.Length - 2);
        }

        result = result.Replace("\r", " ").Replace("\n", " ");
        result = result.Replace("â€¦", "...").Replace("\u2026", "...");
        result = result.Replace("â€œ", "\"").Replace("â€", "\"");
        result = result.Replace("â€˜", "'").Replace("â€™", "'");

        result = Regex.Replace(result, @"\s+", " ").Trim();

        result = ApplyWordFixes(result);
        result = ApplyPhraseFixes(result);

        result = Regex.Replace(result, @"\s+([,.;:!?])", "$1");
        result = Regex.Replace(result, @",(?=\S)", ", ");
        result = Regex.Replace(result, @"([!?])\.", "$1");
        result = Regex.Replace(result, @"\.{4,}", "...");
        result = Regex.Replace(result, @"\s+", " ").Trim();

        result = CapitalizeSentenceStart(result);

        if (wrappedInQuotes && !IsStageDirection(result))
        {
            result = "\"" + result + "\"";
        }

        return result;
    }

    private static string ApplyWordFixes(string text)
    {
        var replacements = new Dictionary<string, string>
        {
            { "Kalo", "Kalau" }, { "kalo", "kalau" },
            { "Ga", "Tidak" }, { "ga", "tidak" },
            { "Gak", "Tidak" }, { "gak", "tidak" },
            { "Nggak", "Tidak" }, { "nggak", "tidak" },
            { "Engga", "Tidak" }, { "engga", "tidak" },
            { "Enggak", "Tidak" }, { "enggak", "tidak" },
            { "Tau", "Tahu" }, { "tau", "tahu" },
            { "Ko", "Kok" }, { "ko", "kok" },
            { "Cuman", "Hanya" }, { "cuman", "hanya" },
            { "Bakalan", "Akan" }, { "bakalan", "akan" },
            { "Ngantuk", "Mengantuk" }, { "ngantuk", "mengantuk" },
            { "Nunggu", "Menunggu" }, { "nunggu", "menunggu" },
            { "Bener", "Benar" }, { "bener", "benar" },
            { "Udah", "Sudah" }, { "udah", "sudah" },
            { "Dapet", "Dapat" }, { "dapet", "dapat" },
            { "Liat", "Lihat" }, { "liat", "lihat" },
            { "Club", "Klub" }, { "club", "klub" },
            { "Cafe", "Kafe" }, { "cafe", "kafe" },
            { "Mall", "Mal" }, { "mall", "mal" }
        };

        foreach (var pair in replacements)
        {
            text = Regex.Replace(text, $@"\b{Regex.Escape(pair.Key)}\b", pair.Value);
        }

        return text;
    }

    private static string ApplyPhraseFixes(string text)
    {
        string[,] replacements =
        {
            { @"\bSamapai\b", "Sampai" },
            { @"\bsamapai\b", "sampai" },
            { @"\bTerimakasih\b", "Terima kasih" },
            { @"\bterimakasih\b", "terima kasih" },
            { @"\btrimakasih\b", "terima kasih" },
            { @"\btampa\b", "tanpa" },
            { @"\bTampa\b", "Tanpa" },
            { @"\bremcana\b", "rencana" },
            { @"\bSekaolah\b", "Sekolah" },
            { @"\bsekaolah\b", "sekolah" },
            { @"\bmemgeluarkan\b", "mengeluarkan" },
            { @"\bkebetualn\b", "kebetulan" },
            { @"\bdunya\b", "dunia" },
            { @"\bfaforit\b", "favorit" },
            { @"\bmeyenangkan\b", "menyenangkan" },
            { @"\bPemian\b", "Pemain" },
            { @"\bkanapa\b", "kenapa" },
            { @"\bterlau\b", "terlalu" },
            { @"\blenih\b", "lebih" },
            { @"\btiak\b", "tidak" },
            { @"\bsesewatu\b", "sesuatu" },
            { @"\bsese\s+watu\b", "sesuatu" },
            { @"\bsese\s+orang\b", "seseorang" },
            { @"\bsebener\s+nya\b", "sebenarnya" },
            { @"\bsebenar\s+nya\b", "sebenarnya" },
            { @"\bseperti\s+nya\b", "sepertinya" },
            { @"\bbiasa\s+nya\b", "biasanya" },
            { @"\bseharus\s+nya\b", "seharusnya" },
            { @"\bharus\s+nya\b", "harusnya" },
            { @"\bmenanya\s+kan\b", "menanyakan" },
            { @"\bApa\s+kah\b", "Apakah" },
            { @"\balasan\s+nya\b", "alasannya" },
            { @"\bngobrol\s+nya\b", "mengobrol" },
            { @"\bsebelum\s+nya\b", "sebelumnya" },
            { @"\bBukan\s+nya\b", "Bukannya" },
            { @"\btentang\s+mu\b", "tentangmu" },
            { @"\bmemanggil\s+ku\b", "memanggilku" },
            { @"\bRambut\s+mu\b", "Rambutmu" },
            { @"\bnama\s+ku\b", "namaku" },
            { @"\bnama\s+mu\b", "namamu" },
            { @"\brumah\s+ku\b", "rumahku" },
            { @"\bwaktu\s+mu\b", "waktumu" },
            { @"\bmata\s+mu\b", "matamu" },
            { @"\bdiri\s+mu\b", "dirimu" },
            { @"\bmimpi\s+mu\b", "mimpimu" },
            { @"\bdi\s+kasih\b", "dikasih" },
            { @"\bdi\s+dapat\b", "didapat" },
            { @"\bdi\s+rayakan\b", "dirayakan" },
            { @"\bdi\s+bentak\b", "dibentak" },
            { @"\bdi\s+panggil\b", "dipanggil" },
            { @"\bdi\s+putus\s+kan\b", "diputuskan" },
            { @"\bdisini\b", "di sini" },
            { @"\bDisini\b", "Di sini" },
            { @"\bdimana\b", "di mana" },
            { @"\bDimana\b", "Di mana" },
            { @"\bkemana\b", "ke mana" },
            { @"\bKemana\b", "Ke mana" },
            { @"\bkesini\b", "ke sini" },
            { @"\bKesini\b", "Ke sini" },
            { @"\bkerumah\b", "ke rumah" },
            { @"\bkekamar\b", "ke kamar" },
            { @"\bkekantin\b", "ke kantin" },
            { @"\bTidak papa\b", "Tidak apa-apa" },
            { @"\btidak papa\b", "tidak apa-apa" },
            { @"\bTidak apa apa\b", "Tidak apa-apa" },
            { @"\btidak apa apa\b", "tidak apa-apa" },
            { @"\bburu buru\b", "buru-buru" },
            { @"\bcepat cepat\b", "cepat-cepat" },
            { @"\bjalan jalan\b", "jalan-jalan" },
            { @"\bpelan pelan\b", "pelan-pelan" },
            { @"\bsia sia\b", "sia-sia" },
            { @"\bteman teman\b", "teman-teman" }
        };

        for (int i = 0; i < replacements.GetLength(0); i++)
        {
            text = Regex.Replace(text, replacements[i, 0], replacements[i, 1]);
        }

        return text;
    }

    private static string CapitalizeSentenceStart(string text)
    {
        return Regex.Replace(text, @"(^|[.!?]\s+)([a-z])", match => match.Groups[1].Value + match.Groups[2].Value.ToUpperInvariant());
    }

    private static bool IsStageDirection(string text)
    {
        return text.StartsWith("(") || text.StartsWith("[");
    }
}
#endif
