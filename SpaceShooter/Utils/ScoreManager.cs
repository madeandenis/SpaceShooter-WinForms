using System.Text.Json;

namespace SpaceShooter.Utils
{
    internal class ScoreManager
    {
        private string filePath;

        public ScoreManager() 
        {
            string subPath = $"..\\..\\..\\UserData\\score.json";
            filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), subPath);
        }

        public void SaveScores(int highscore, int lastScore)
        {
            try
            {
                var scores = new Dictionary<string, int>
                {
                    { "highscore", highscore },
                    { "lastScore", lastScore }
                };

                string jsonString = JsonSerializer.Serialize(scores);
                File.WriteAllText(this.filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving scores: {ex.Message}");
            }
        }

        public Dictionary<string, int> LoadScores()
        {
            try
            {
                var defaultScores = new Dictionary<string, int>
                    {
                        { "highscore", 0 },
                        { "lastScore", 0 }
                    };

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    var scores = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString)
                                 ?? new Dictionary<string, int>(); ;

                    foreach (var key in defaultScores.Keys)
                    {
                        if (!scores.ContainsKey(key))
                        {
                            scores[key] = defaultScores[key];
                        }
                    }

                    jsonString = JsonSerializer.Serialize(scores);
                    File.WriteAllText(filePath, jsonString);

                    return scores;
                }
                else
                {
                    string jsonString = JsonSerializer.Serialize(defaultScores);
                    File.WriteAllText(filePath, jsonString);
                    return defaultScores;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading scores: {ex.Message}");
                return new Dictionary<string, int>(); 
            }
        }

        public void UpdateHighscore(int newHighscore)
        {
            try
            {
                var scores = LoadScores(); 
                scores["highscore"] = newHighscore; 

                SaveScores(scores["highscore"], scores["lastScore"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating highscore: {ex.Message}");
            }
        }

        public void UpdateLastScore(int newLastScore)
        {
            try
            {
                var scores = LoadScores(); 
                scores["lastScore"] = newLastScore; 

                SaveScores(scores["highscore"], scores["lastScore"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating last score: {ex.Message}");
            }
        }
    }
}
