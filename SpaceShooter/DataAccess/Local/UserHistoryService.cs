namespace SpaceShooter.DataAccess.Local
{
    internal class UserHistoryService : LocalDataService<string, List<DateTime>>
    {
        public UserHistoryService() : base("user_history.json") { }
        public void AddUsername(string username)
        {
            var data = LoadData();
            if (!data.ContainsKey(username))
            {
                data[username] = new List<DateTime>();
            }

            data[username].Add(DateTime.UtcNow);  

            SaveData(data);
        }

        public string GetLastLoggedInUser()
        {
            var data = LoadData();
            string lastLoggedInUser = "";
            DateTime? lastTimestamp = null;

            foreach (var entry in data)
            {
                if (entry.Value.Count > 0)
                {
                    DateTime currentTimestamp = entry.Value[entry.Value.Count - 1];  

                    if (lastTimestamp == null || currentTimestamp > lastTimestamp)
                    {
                        lastTimestamp = currentTimestamp;
                        lastLoggedInUser = entry.Key;
                    }
                }
            }

            return lastLoggedInUser;
        }
    }
}
