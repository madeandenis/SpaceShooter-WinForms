namespace SpaceShooter.GameConfig
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
    }

    public class GameDifficulty
    {

        private Difficulty _difficulty;
        public Difficulty Difficulty {
            get => _difficulty;
            set
            {
                if (_difficulty != value) 
                {
                    _difficulty = value;
                    ConfigureGameDifficulty(value);
                }
            } 
        }

        public int PlayerHealth { get; private set; }
        public (int Min, int Max) AsteroidSpeedRange { get; private set; }
        public (float Min, float Max) AsteroidSizeRange { get; private set; }
        public int AsteroidSpawnRate { get; private set; }
        public int MaxAsteroids { get; private set; }

        Random random;

        public GameDifficulty(Difficulty difficulty)
        {
            random = new Random();
            ConfigureGameDifficulty(difficulty);
        }

        public void ConfigureGameDifficulty(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    EasyMode();
                    break;
                case Difficulty.Medium:
                    MediumMode();
                    break;
                case Difficulty.Hard:
                    HardMode();
                    break;
                default:
                    throw new ArgumentException("Invalid difficulty level.");
            }
        }

        private void EasyMode()
        {
            PlayerHealth = 3;
            AsteroidSpeedRange = (20, 35);
            AsteroidSizeRange = (1.0f, 2.0f);
            AsteroidSpawnRate = 600;
            MaxAsteroids = 10;
        }

        private void MediumMode()
        {
            PlayerHealth = 2;
            AsteroidSpeedRange = (50, 60);
            AsteroidSizeRange = (0.8f, 1.5f);
            AsteroidSpawnRate = 400;
            MaxAsteroids = 20;
        }

        private void HardMode()
        {
            PlayerHealth = 1;
            AsteroidSpeedRange = (60, 80);
            AsteroidSizeRange = (0.5f, 1.2f);
            AsteroidSpawnRate = 250;
            MaxAsteroids = 25;
        }

        public int GetRandomAsteroidSpeed() =>
            random.Next(AsteroidSpeedRange.Min, AsteroidSpeedRange.Max + 1);

        public float GetRandomAsteroidSize() =>
            (float)(random.NextDouble() * (AsteroidSizeRange.Max - AsteroidSizeRange.Min) + AsteroidSizeRange.Min);

    }
}
