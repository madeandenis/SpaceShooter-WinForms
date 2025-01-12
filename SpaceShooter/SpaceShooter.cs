using SpaceShooter.DataAccess.Local;
using SpaceShooter.DataAccess.Remote;
using SpaceShooter.GameConfig;
using SpaceShooter.GameObjects;
using SpaceShooter.User;
using SpaceShooter.Utils;
using System.ComponentModel;
using System.Media;
using Timer = System.Windows.Forms.Timer;

namespace SpaceShooter
{
    public partial class SpaceShooter : Form
    {
        // Settings
        private bool IsPaused = false;
        private bool IsOver = false;
        private int FPS = 60;
        private int ScoreIncrementRate = 200;
        GameDifficulty gameDifficulty;

        // User Data
        private UserData User;

        // Session
        private SessionManager sessionManager;

        // Timers - GameLoop
        private Timer GameTimer;

        // Shooting delay
        private bool CanShoot = true;
        private Timer ShootDelayTimer;
        private int ShootDelayMs = 100;

        // Asteroid Spawning
        private Timer AsteroidSpawnTimer;

        // Scoring
        private ScoreManager scoreManager;
        private Timer scoreIncrementTimer;

        // Assets
        private SoundPlayer LaserSound;
        private FontFamily HiScoreFont;
        private Sprite ShipSprite;
        private Sprite TargetCursorSprite;
        private Sprite LaserBeamSprite;

        // Factories
        Func<GameObject> LaserBeamFactory;
        Func<Asteroid> AsteroidFactory;

        // Game Objects
        private GameObject Spaceship;
        private GameObject TargetCursor;
        private List<GameObject> ActiveLaserBeams;
        private List<Asteroid> ActiveAsteroids;

        // Object Pools
        private GameObjectPool<GameObject> LaserBeamsPool;
        private GameObjectPool<Asteroid> AsteroidsPool;

        // Player 
        private int PlayerHealth;

        private DateTime lastHitTime = DateTime.MinValue;
        private int collisionDelayMs = 1000;

        private float _playerSpeed;
        private float _laserBeamSpeed;
        private float PlayerSpeed
        {
            get => _playerSpeed;
            set
            {
                _playerSpeed = TransformUtil.SpeedTransform.Adapt(value, this);
            }
        }
        private float LaserBeamSpeed
        {
            get => _laserBeamSpeed;
            set
            {
                _laserBeamSpeed = TransformUtil.SpeedTransform.Adapt(value, this);
            }
        }

        // Debugging
        private Boolean DebugMode = true;

        // Components
        Rectangle restartButtonRectangle;

        public SpaceShooter()
        {
            this.DoubleBuffered = true; // Drawing happens off-screen (buffer) then copied 

            InitializeComponent(); // Auto-generated WinForms method

            InitializeUser();

            InitializeDifficulty(Difficulty.Easy);

            InitializeAssets();
            InitializeGameObjects();

            InitializeTimers();

            Cursor.Hide();
        }

        public void InitializeUser()
        {
            string lastLoggedUser = new UserHistoryService().GetLastLoggedInUser();
            User = new UserData(lastLoggedUser);

            scoreManager = new ScoreManager(User);
            sessionManager = new SessionManager(User);
        }

        public void InitializeDifficulty(Difficulty difficulty)
        {
            gameDifficulty = new GameDifficulty(difficulty);
            PlayerHealth = gameDifficulty.PlayerHealth;
        }


        private void InitializeTimers()
        {
            InitializeGameTimer();
            InitializeScoreIncrementTimer();
            InitializeShootDelayTimer();
            InitializeAsteroidSpawnerTimer();
        }


        private void InitializeScoreIncrementTimer()
        {
            scoreIncrementTimer = new Timer();
            scoreIncrementTimer.Interval = ScoreIncrementRate;
            scoreIncrementTimer.Tick += (s, e) =>
            {
                scoreManager.IncrementScore(10);
            };
            scoreIncrementTimer.Start();
        }


        private void InitializeGameTimer()
        {
            GameTimer = new Timer();
            GameTimer.Interval = 1000 / FPS;
            GameTimer.Tick += GameLoop;
            GameTimer.Start();
        }

        private void InitializeShootDelayTimer()
        {
            ShootDelayTimer = new Timer();
            ShootDelayTimer.Interval = ShootDelayMs;
            ShootDelayTimer.Tick += (s, e) =>
            {
                CanShoot = true;
                ShootDelayTimer.Stop();
            };
        }

        private void InitializeAsteroidSpawnerTimer()
        {
            AsteroidSpawnTimer = new Timer();
            AsteroidSpawnTimer.Interval = gameDifficulty.AsteroidSpawnRate;
            AsteroidSpawnTimer.Tick += (s, e) =>
            {
                SpawnAsteroids();
            };
            AsteroidSpawnTimer.Start();
        }
        private void UpdateAsteroidSpawnRate()
        {
            AsteroidSpawnTimer.Interval = gameDifficulty.AsteroidSpawnRate;
        }

        private void InitializeAssets()
        {
            HiScoreFont = AssetLoader.LoadFont("big_noodle_titling.ttf");
            LaserSound = AssetLoader.LoadAudio("retro-laser.wav");

            ShipSprite = new Sprite(this, "player-ship.png", scale: 1.4f);
            TargetCursorSprite = new Sprite(this, "green-target.png", scale: 1.2f);
            LaserBeamSprite = new Sprite(this, "laser-beam.png", scale: 4f);
        }

        private void InitializeGameObjects()
        {
            Point screenCenter = new Point(Width / 2, Height / 2);

            PlayerSpeed = 40;
            LaserBeamSpeed = 100;

            Spaceship = new GameObject(this, screenCenter, ShipSprite, PlayerSpeed);
            TargetCursor = new GameObject(this, screenCenter, TargetCursorSprite);

            LaserBeamFactory = () => new GameObject(this, screenCenter, LaserBeamSprite, LaserBeamSpeed);
            AsteroidFactory = () =>
            {
                float asteroidScale = gameDifficulty.GetRandomAsteroidSize();
                int asteroidSpeed = gameDifficulty.GetRandomAsteroidSpeed();

                Sprite asteroidSprite = new Sprite(this, AssetLoader.LoadImage("asteroid.png"), scale: asteroidScale);
                return new Asteroid(
                    this,
                    new Point(0, 0),
                    asteroidSprite,
                    asteroidSpeed
                );
            };

            LaserBeamsPool = new GameObjectPool<GameObject>(LaserBeamFactory, 50);
            ActiveLaserBeams = new List<GameObject>();

            AsteroidsPool = new GameObjectPool<Asteroid>(AsteroidFactory, gameDifficulty.MaxAsteroids);
            ActiveAsteroids = new List<Asteroid>();

        }

        private void GameLoop(object sender, EventArgs e)
        {
            Spaceship.MoveTowardsTarget(TargetCursor.Position);

            UpdateActiveLaserBeams();
            UpdateActiveAsteroids();

            CheckGroupCollision(ActiveLaserBeams, ActiveAsteroids, handleLaserAsteroidCollision);
            CheckGroupCollision(new List<GameObject> { Spaceship }, ActiveAsteroids, handleSpaceshipAsteroidCollision);

            this.Invalidate(); // Trigger a repaint
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            TargetCursor.MoveToCursor(e);
            Spaceship.RotateTowardsTarget(TargetCursor.Position);
        }
        private void SpaceShooter_MouseClick(object sender, MouseEventArgs e)
        {
            if (IsOver && restartButtonRectangle.Contains(e.Location))
            {
                RestartGame();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!IsOver)
            {
                TargetCursor.Render(e);
                Spaceship.Render(e);

                RenderGameObjects(ActiveLaserBeams, e);
                RenderGameObjects(ActiveAsteroids, e);

                RenderLives(e);

                PrintScore(e);
            }
            else
            {
                RenderGameOver(e);
            }
            if (DebugMode) PrintDebugInfo(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            scoreManager.SaveHiScore();
        }


        private void SpaceShooter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && CanShoot)
            {
                CanShoot = false;
                SpawnLaserBeam();
                ShootDelayTimer.Start();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                TogglePauseGame();
            }
        }

        private async void TogglePauseGame()
        {
            UpdateAsteroidSpawnRate();

            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }

            this.Invalidate();
        }

        private void ResumeGame()
        {
            GameTimer.Start();
            AsteroidSpawnTimer.Start();
            scoreIncrementTimer.Start();
            IsPaused = false;
        }

        private void PauseGame()
        {
            GameTimer.Stop();
            AsteroidSpawnTimer.Stop();
            scoreIncrementTimer.Stop();
            IsPaused = true;

            scoreManager.SaveHiScore();

            ShowPauseMenu();
            TogglePauseGame();
        }
        private void StopGame()
        {
            GameTimer.Stop();
            AsteroidSpawnTimer.Stop();
            scoreIncrementTimer.Stop();

            scoreManager.SaveHiScore();

            IsOver = true;

            Cursor.Show();
        }

        private void RestartGame()
        {
            PlayerHealth = gameDifficulty.PlayerHealth;
            scoreManager.CurrentScore = 0;

            ActiveAsteroids.Clear();
            ActiveLaserBeams.Clear();

            GameTimer.Start();
            AsteroidSpawnTimer.Start();
            scoreIncrementTimer.Start();

            IsPaused = false;
            IsOver = false;
        }

        private void ShowPauseMenu()
        {
            Cursor.Show();

            GameMenu pauseMenu = new GameMenu(gameDifficulty, User);
            pauseMenu.ShowDialog(this);

            Cursor.Hide();
        }


        private void SpawnLaserBeam()
        {
            LaserSound.Play();
            GameObject laserBeam = LaserBeamsPool.Get();
            laserBeam.MatchTransform(
                Spaceship,
                Spaceship.Sprite.Texture.Width / 2,
                +Spaceship.Sprite.Texture.Height / 2
            );
            ActiveLaserBeams.Add(laserBeam);
        }

        private void SpawnAsteroids()
        {
            if (ActiveAsteroids.Count < gameDifficulty.MaxAsteroids)
            {
                Asteroid asteroid = AsteroidsPool.Get();
                asteroid.SpawnOffScreen();
                asteroid.RotateTowardsInBounds();
                ActiveAsteroids.Add(asteroid);
            }
        }

        private void UpdateActiveAsteroids()
        {
            for (int i = ActiveAsteroids.Count - 1; i >= 0; i--)
            {
                Asteroid obj = ActiveAsteroids[i];
                obj.MoveInOrientation();

                if (obj.IsOutsideOfBounds())
                {
                    obj.TimeInactive++;

                    if (obj.TimeInactive > 0.5 * FPS)
                    {
                        obj.TimeInactive = 0;
                        ActiveAsteroids.RemoveAt(i);
                        obj.Reset();
                        AsteroidsPool.Return(obj);
                        continue;
                    }
                }

                obj.Sprite.Rotate(0.3f);
            }
        }

        private void UpdateActiveLaserBeams()
        {
            for (int i = ActiveLaserBeams.Count - 1; i >= 0; i--)
            {
                GameObject obj = ActiveLaserBeams[i];
                obj.MoveInOrientation();
                if (obj.IsOutsideOfBounds())
                {
                    ActiveLaserBeams.RemoveAt(i);
                    LaserBeamsPool.Return(obj);
                }
            }
        }

        private void CheckGroupCollision<T1, T2>(List<T1> group1, List<T2> group2, Action<T1, T2> collisionHandler)
            where T1 : GameObject
            where T2 : GameObject
        {

            if (group1.Count == 0 || group2.Count == 0)
            {
                return;
            }

            for (int i = group1.Count - 1; i >= 0; i--)
            {
                if (i >= group1.Count) continue;
                T1 obj1 = group1[i];

                for (int j = group2.Count - 1; j >= 0; j--)
                {
                    if (j >= group2.Count) continue;
                    T2 obj2 = group2[j];

                    collisionHandler(obj1, obj2);
                }
            }
        }

        private void handleLaserAsteroidCollision(GameObject obj1, GameObject obj2)
        {
            if (obj1 is not GameObject laserBeam || obj2 is not Asteroid asteroid)
                return;

            if (obj1.Intersects(obj2, GameObject.CollisionShape.Rectangle, -20))
            {
                ActiveLaserBeams.Remove(laserBeam);
                LaserBeamsPool.Return(laserBeam);
                asteroid.ApplyDeviation(laserBeam);
            }
        }

        private void handleSpaceshipAsteroidCollision(GameObject obj1, GameObject obj2)
        {
            if (obj1 is not GameObject spaceship || obj2 is not Asteroid asteroid)
                return;

            if (spaceship.Intersects(asteroid)
                && DateTime.Now - lastHitTime > TimeSpan.FromMilliseconds(collisionDelayMs))
            {
                PlayerHealth--;

                asteroid.ApplyDeviation(spaceship);

                lastHitTime = DateTime.Now;

                if (PlayerHealth <= 0)
                {
                    StopGame();
                }

            }
        }

        private void RenderGameObjects<T>(List<T> gameObjects, PaintEventArgs e) where T : GameObject
        {
            foreach (var gameObject in gameObjects)
            {
                gameObject.Render(e);
            }
        }

        private void RenderLives(PaintEventArgs e)
        {
            int paddingX = 40;
            int paddingY = 100;

            int startX =
                this.Bounds.Width / 2
                - ((ShipSprite.Texture.Width * PlayerHealth) + (paddingX * PlayerHealth)) / 2;
            int startY = this.Bounds.Top + paddingY;

            for (int i = 0; i < PlayerHealth; i++)
            {
                Sprite shipSprite = new Sprite(this, "player-ship.png", 90, 1.7f);
                Point position = new Point(startX + i * (ShipSprite.Texture.Width + paddingX), 50);

                GameObject health = new GameObject(this, position, shipSprite);

                health.Render(e);
            }
        }

        private void RenderGameOver(PaintEventArgs e)
        {
            Font gameOverFont = new Font(HiScoreFont, 100, FontStyle.Bold);
            Brush brush = Brushes.Red;
            string gameOverText = "GAME OVER";

            SizeF textSize = e.Graphics.MeasureString(gameOverText, gameOverFont);

            float x = (this.ClientSize.Width - textSize.Width) / 2;
            float y = (this.ClientSize.Height - textSize.Height * 2) / 2;

            e.Graphics.DrawString(gameOverText, gameOverFont, brush, x, y);

            Font valueFont = new Font(HiScoreFont, 60);
            Brush valueBrush = new SolidBrush(Color.FromArgb(180, 245, 17));

            string ScoreText = "Your Score: " + scoreManager.CurrentScore;
            SizeF ScoreTextSize = e.Graphics.MeasureString(ScoreText, valueFont);

            float highScoreX = (this.ClientSize.Width - ScoreTextSize.Width) / 2;
            float highScoreY = y + ScoreTextSize.Height + 40;

            e.Graphics.DrawString(ScoreText, valueFont, valueBrush, highScoreX, highScoreY);

            float restartButtonYPosition = highScoreY + ScoreTextSize.Height + 30;
            DrawRestartButton(e, restartButtonYPosition);

        }
        private void DrawRestartButton(PaintEventArgs e, float yPosition)
        {
            int buttonWidth = 300;
            int buttonHeight = 80;

            int buttonX = (this.ClientSize.Width - buttonWidth) / 2;
            int buttonY = (int)yPosition;

            restartButtonRectangle = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

            Brush buttonBrush = Brushes.Black;
            e.Graphics.FillRectangle(buttonBrush, restartButtonRectangle);

            Font buttonFont = new Font(HiScoreFont, 40);
            Brush buttonTextBrush = Brushes.White;

            string restartButtonText = "Restart";

            SizeF buttonTextSize = e.Graphics.MeasureString(restartButtonText, buttonFont);
            float buttonTextX = restartButtonRectangle.X + (restartButtonRectangle.Width - buttonTextSize.Width) / 2;
            float buttonTextY = restartButtonRectangle.Y + (restartButtonRectangle.Height - buttonTextSize.Height) / 2;

            e.Graphics.DrawString(restartButtonText, buttonFont, buttonTextBrush, buttonTextX, buttonTextY);
        }


        private void PrintScore(PaintEventArgs e)
        {
            Font labelFont = new Font(HiScoreFont, 30);
            Font valueFont = new Font(HiScoreFont, 60);

            Brush brush = new SolidBrush(Color.FromArgb(180, 245, 17));

            float padding = this.Bounds.Width / 10;
            float tilt = 2;

            float X = this.Bounds.Left + padding;
            float Y = this.Bounds.Bottom - padding;

            DrawRotatedString(e, "POINTS", labelFont, brush, X, Y, tilt);

            X -= labelFont.Size * FormatScore(scoreManager.CurrentScore).Length * 1.9f;
            Y -= labelFont.Size * 2.2f;

            DrawRotatedString(e, FormatScore(scoreManager.CurrentScore), valueFont, brush, X, Y, tilt);

            X = this.Bounds.Right - padding * 2 - FormatScore(scoreManager.HiScore).Length * valueFont.Size;
            Y = this.Bounds.Bottom - padding;

            DrawRotatedString(e, FormatScore(scoreManager.HiScore), valueFont, brush, X, Y, -tilt);

            Y -= valueFont.Size * 0.9f;
            X += labelFont.Size * FormatScore(scoreManager.HiScore).Length * 1.7f;

            DrawRotatedString(e, "HISCORE", labelFont, brush, X, Y, -tilt);

        }

        private void PrintDebugInfo(PaintEventArgs e)
        {
            Font font = new Font("Arial", 12);
            Brush brush = Brushes.White;
            int X = 10;
            int Y = 10;

            string varString = $"Current user: {User.Name}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;

            // Active Laser Beams & Laser Beam Pool
            varString = $"Active Laser Beams: {ActiveLaserBeams.Count}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;
            varString = $"Pool Laser Beams: {LaserBeamsPool.Count}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;

            // Active Asteroids & Asteroid Pool
            varString = $"Active Asteroids: {ActiveAsteroids.Count}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;
            varString = $"Pool Asteroids: {AsteroidsPool.Count}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;

            // Player Health
            varString = $"Player Health: {PlayerHealth}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;

            // FPS
            varString = $"FPS: {FPS}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;

            // Game Difficulty Level
            varString = $"Difficulty: {gameDifficulty.Difficulty}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;

            // Max Active Asteroids
            varString = $"Max Active Asteroids: {gameDifficulty.MaxAsteroids}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;

            // Asteroids Range
            varString = $"Asteroid Size Range: {gameDifficulty.AsteroidSizeRange}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;
            varString = $"Asteroid Speed Asteroids:  {gameDifficulty.AsteroidSpeedRange}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;
            varString = $"Asteroid Spawn Rate:  {AsteroidSpawnTimer.Interval}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;
        }

        private void DrawRotatedString(PaintEventArgs e, string text, Font font, Brush brush, float x, float y, float rotationAngle)
        {
            // Current transformation matrix
            var currentTransform = e.Graphics.Transform;

            int defaultTextLength = text.IndexOf(' ');
            if (defaultTextLength == -1) defaultTextLength = text.Length;

            e.Graphics.TranslateTransform(x + font.Size * defaultTextLength, y - font.Size / 2);
            e.Graphics.RotateTransform(rotationAngle);

            e.Graphics.DrawString(text, font, brush, 0, 0);

            e.Graphics.Transform = currentTransform;
        }

        public string FormatScore(int score)
        {
            return score.ToString("00000000");
        }
    }

    internal class SessionManager
    {
        private readonly UserHistoryService userHistoryService;

        private readonly UserData user;

        public SessionManager(UserData user)
        {
            this.user = user;
            this.userHistoryService = new UserHistoryService(); 

            if (!string.IsNullOrEmpty(user.Name))
            {
                userHistoryService.AddUsername(user.Name);
            }

            user.PropertyChanged += User_PropertyChanged;
        }

        private void User_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserData.Name) && !string.IsNullOrEmpty(user.Name))
            {
                userHistoryService.AddUsername(user.Name);
            }
        }
    }


    internal class ScoreManager
    {
        private readonly LocalScoreService localScoreService;
        private readonly RemoteScoreService remoteScoreService;
        private readonly UserData user;

        public int CurrentScore;
        public int HiScore;

        public ScoreManager(UserData user)
        {
            this.user = user;
            localScoreService = new LocalScoreService();
            remoteScoreService = new RemoteScoreService();

            HiScore = localScoreService.LoadHighscore(user.Name); 

            user.PropertyChanged += User_PropertyChanged;
        }

        private void User_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserData.Name) && !string.IsNullOrEmpty(user.Name))
            {
                UpdateScore();
            }
        }

        private void UpdateScore()
        {
            HiScore = localScoreService.LoadHighscore(user.Name);
            CurrentScore = 0;
        }

        public void SaveHiScore()
        {
            localScoreService.SaveHighscore(user.Name, HiScore);
            remoteScoreService.SetScore("SpaceShooter", user.Name, HiScore);
        }

        public void IncrementScore(int increment)
        {
            CurrentScore += increment;

            if (CurrentScore > HiScore)
            {
                HiScore = CurrentScore;
            }
        }

    }

}
