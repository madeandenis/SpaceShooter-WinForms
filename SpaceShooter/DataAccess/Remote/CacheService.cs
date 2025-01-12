using StackExchange.Redis;

namespace SpaceShooter.DataAccess.Remote
{
    public class CacheService
    {
        protected readonly ConnectionMultiplexer _redis;
        protected readonly IDatabase _db;

        public CacheService(string connectionString = "localhost:6379")
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(connectionString);
                _db = _redis.GetDatabase();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not connect to Redis: {ex.Message}", ex);
            }
        }

        public bool SortedSetAdd(string setKey, string member, double score)
        {
            try
            {
                return _db.SortedSetAdd(setKey, member, score);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding/updating sorted set member '{member}' in Redis: {ex.Message}", ex);
            }
        }

        public double? SortedSetScore(string setKey, string member)
        {
            try
            {
                return _db.SortedSetScore(setKey, member);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving score for member '{member}' in sorted set '{setKey}': {ex.Message}", ex);
            }
        }

        public List<(string Member, double Score)> SortedSetGetTop(string setKey, int count)
        {
            try
            {
                var entries = _db.SortedSetRangeByRankWithScores(setKey, 0, count - 1, Order.Descending);
                return entries.Select(e => (e.Element.ToString(), e.Score)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving top members from sorted set '{setKey}': {ex.Message}", ex);
            }
        }
    }
}
