namespace SpaceShooter.DataAccess.Local
{
    internal class LocalScoreService : LocalDataService<string, int>
    {
        public LocalScoreService() : base("highscore.json") { }

        public int LoadHighscore(string username)
        {
            var data = LoadData();
            return data.ContainsKey(username) ? data[username] : 0;
        }

        public void SaveHighscore(string username, int highscore)
        {
            var data = LoadData();
            data[username] = highscore;
            SaveData(data);
        }

        public void UpdateHighscore(string username, int newHighscore)
        {
            int currentHighscore = LoadHighscore(username);
            if (newHighscore > currentHighscore)
            {
                SaveHighscore(username, newHighscore);
            }
        }
    }
}
