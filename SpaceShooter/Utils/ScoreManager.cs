using System.Text.Json;

namespace SpaceShooter.Utils
{
    internal class ScoreManager
    {
        private readonly string filePath;

        public ScoreManager()
        {
            string subPath = $"..\\..\\..\\UserData\\highscore.json";
            filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), subPath);
        }

        public int LoadHighscore()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    SaveHighscore(0); 
                }

                File.SetAttributes(filePath, FileAttributes.ReadOnly); 
                string jsonString = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);

                return data != null && data.ContainsKey("highscore") ? data["highscore"] : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading highscore: {ex.Message}");
                return 0;
            }
        }

        public void SaveHighscore(int highscore)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }

                string jsonString = JsonSerializer.Serialize(new { highscore });
                File.WriteAllText(filePath, jsonString);

                File.SetAttributes(filePath, FileAttributes.ReadOnly); // Set back to read-only
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving highscore: {ex.Message}");
            }
        }

        public void UpdateHighscore(int newHighscore)
        {
            int currentHighscore = LoadHighscore();
            if (newHighscore > currentHighscore)
            {
                SaveHighscore(newHighscore);
            }
        }
    }
}
