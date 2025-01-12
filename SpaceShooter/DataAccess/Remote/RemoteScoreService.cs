namespace SpaceShooter.DataAccess.Remote
{
    internal class RemoteScoreService : CacheService
    {   
        public RemoteScoreService() : base()
        {
        }

        public bool SetScore(string leaderboardKey, string playerName, double score)
        {
            var currentScore = GetScore(leaderboardKey, playerName);

            if (!currentScore.HasValue || score > currentScore.Value)
            {
                return SortedSetAdd(leaderboardKey, playerName, score);
            }

            return false; 
        }

        public double? GetScore(string leaderboardKey, string playerName)
        {
            return SortedSetScore(leaderboardKey, playerName);
        }

        public List<(string PlayerName, double Score)> GetTopScores(string leaderboardKey, int count)
        {
            return SortedSetGetTop(leaderboardKey, count);
        }
    }
}
