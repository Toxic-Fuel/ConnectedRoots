using System;
using System.IO;
using UnityEngine;

namespace ScoreSystem
{
    [Serializable]
    public class HighscoreData
    {
        public int bestScore;
    }

    public static class HighscoreSystem
    {
        private const string FileName = "highscore.json";

        private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static int GetBestScore()
        {
            HighscoreData data = LoadData();
            return Mathf.Max(0, data.bestScore);
        }

        public static bool TrySubmitScore(int score, out int bestScore)
        {
            int safeScore = Mathf.Max(0, score);
            HighscoreData data = LoadData();

            if (safeScore <= data.bestScore)
            {
                bestScore = data.bestScore;
                return false;
            }

            data.bestScore = safeScore;
            SaveData(data);
            bestScore = data.bestScore;
            return true;
        }

        public static void ResetHighscore()
        {
            SaveData(new HighscoreData());
        }

        private static HighscoreData LoadData()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    return new HighscoreData();
                }

                string json = File.ReadAllText(SavePath);
                if (string.IsNullOrEmpty(json))
                {
                    return new HighscoreData();
                }

                HighscoreData data = JsonUtility.FromJson<HighscoreData>(json);
                return data ?? new HighscoreData();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HighscoreSystem: Failed to load highscore file. {ex.Message}");
                return new HighscoreData();
            }
        }

        private static void SaveData(HighscoreData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HighscoreSystem: Failed to save highscore file. {ex.Message}");
            }
        }
    }
}

