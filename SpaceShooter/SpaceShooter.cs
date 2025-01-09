using SpaceShooter.GameObjects;
using SpaceShooter.Utils;
using System.Media;
using Timer = System.Windows.Forms.Timer;

namespace SpaceShooter
{
    public partial class SpaceShooter : Form
    {
        // Configs
        private int FPS = 60;
        private int shootDelayMs = 400;
        private int asteroidSpawnRateMs = 1000;
        private int scoreIncrementRate = 200;
        private int maxActiveAsteroids = 10;
        
        // Timers - GameLoop
        private Timer gameTimer;

        // Shooting delay
        private bool canShoot = true; 
        private Timer shootDelayTimer;

        // Asteroid Spawning
        private Timer asteroidSpawnTimer;

        // Scoring
        private int Score;
        private int HiScore;
        private Timer scoreIncrementTimer;
        private ScoreManager scoreManager;

        // Assets
        private SoundPlayer LaserSound;
        private FontFamily HiScoreFont;
        private Sprite ShipSprite;
        private Sprite TargetCursorSprite;
        private Sprite LaserBeamSprite;

        // Game Objects
        private GameObject Spaceship;
        private GameObject TargetCursor;
        private List<GameObject> ActiveLaserBeams;
        private List<Asteroid> ActiveAsteroids;

        // Object Pools
        private GameObjectPool<GameObject> LaserBeamsPool;
        private GameObjectPool<Asteroid> AsteroidsPool;

        // Speed
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
        
        public SpaceShooter()
        {
            Cursor.Hide();
            this.DoubleBuffered = true; // Drawing happens off-screen (buffer) then copied 

            InitializeComponent(); // Auto-generated WinForms method

            InitializeAssets();
            InitializeGameObjects();
            
            InitializeScore();
            InitializeScoreIncrementTimer();

            InitializeGameTimer();
            InitializeShootDelayTimer();
            InitializeAsteroidSpawnerTimer();
        }

        private void InitializeScore()
        {
            scoreManager = new ScoreManager();
            Dictionary<string, int> scores = scoreManager.LoadScores();
            Score = scores["lastScore"];
            HiScore = scores["highscore"];
        }
        private void InitializeScoreIncrementTimer()
        {
            scoreIncrementTimer = new Timer();
            scoreIncrementTimer.Interval = scoreIncrementRate; 
            scoreIncrementTimer.Tick += (s, e) =>
            {
                Score += 10;
                if(Score > HiScore) 
                    HiScore = Score;
            };
            scoreIncrementTimer.Start();
        }


        private void InitializeGameTimer()
        {
            gameTimer = new Timer();
            gameTimer.Interval = 1000 / FPS;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }
        private void InitializeShootDelayTimer()
        {
            shootDelayTimer = new Timer();
            shootDelayTimer.Interval = shootDelayMs;
            shootDelayTimer.Tick += (s, e) =>
            {
                canShoot = true;
                shootDelayTimer.Stop();
            };
        }

        private void InitializeAsteroidSpawnerTimer()
        {
            asteroidSpawnTimer = new Timer();
            asteroidSpawnTimer.Interval = asteroidSpawnRateMs;
            asteroidSpawnTimer.Tick += (s, e) =>
            {
                SpawnAsteroids();
            };
            asteroidSpawnTimer.Start();
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

            PlayerSpeed = 20;
            LaserBeamSpeed = 70;

            Spaceship    = new GameObject(this, screenCenter, ShipSprite, PlayerSpeed);
            TargetCursor = new GameObject(this, screenCenter, TargetCursorSprite);
            
            Func<GameObject> LaserBeamFactory = () => new GameObject(this, screenCenter, LaserBeamSprite, LaserBeamSpeed);
            Func<Asteroid> AsteroidFactory = () =>
            {
                float asteroidScale = new Random().Next(50, 250) / 100f;
                int asteroidSpeed = new Random().Next(15, 30);

                Sprite asteroidSprite = new Sprite(this, AssetLoader.LoadImage("asteroid.png"), scale: asteroidScale);
                return new Asteroid(
                    this,
                    screenCenter,
                    asteroidSprite,
                    asteroidSpeed    
                );
            };

            LaserBeamsPool = new GameObjectPool<GameObject>(LaserBeamFactory, 50);
            ActiveLaserBeams = new List<GameObject>();
            
            AsteroidsPool = new GameObjectPool<Asteroid>(AsteroidFactory, 20);
            ActiveAsteroids = new List<Asteroid>();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            Spaceship.MoveTowardsTarget(TargetCursor.Position);

            UpdateActiveLaserBeams();
            UpdateActiveAsteroids();

            CheckGroupCollision(ActiveLaserBeams, ActiveAsteroids, handleLaserAsteroidCollision);

            this.Invalidate(); // Trigger a repaint
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            TargetCursor.MoveToCursor(e);
            Spaceship.RotateTowardsTarget(TargetCursor.Position);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            TargetCursor.Render(e);
            Spaceship.Render(e);
            RenderGameObjects(ActiveLaserBeams, e);
            RenderGameObjects(ActiveAsteroids, e);

            PrintScore(e);
            if(DebugMode) PrintDebugInfo(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            scoreManager.SaveScores(HiScore, Score);
        }


        private void SpaceShooter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && canShoot)
            {
                canShoot = false;        
                SpawnLaserBeam();         
                shootDelayTimer.Start(); 
            }
        }

        private void SpawnLaserBeam()
        {
            LaserSound.Play();
            GameObject laserBeam = LaserBeamsPool.Get();
            laserBeam.MatchTransform(Spaceship);
            ActiveLaserBeams.Add(laserBeam);
        }

        private void SpawnAsteroids()
        {
            if (ActiveAsteroids.Count < maxActiveAsteroids) 
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

                    if (obj.TimeInactive > 3 * FPS) 
                    {
                        obj.TimeInactive = 0;
                        ActiveAsteroids.RemoveAt(i);
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

        private void RenderGameObjects<T>(List<T> gameObjects, PaintEventArgs e) where T : GameObject
        {
            foreach (var gameObject in gameObjects)
            {
                gameObject.Render(e);
            }
        }

        private void PrintDebugInfo(PaintEventArgs e)
        {
            Font font = new Font("Arial", 14);
            Brush brush = Brushes.White;
            int X = 10;
            int Y = 10;

            String varString = $"Active Laser Beams: {ActiveLaserBeams.Count}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;
            varString = $"Pool Laser Beams: {LaserBeamsPool.Count}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 40;

            varString = $"Active Asteroids: {ActiveAsteroids.Count}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;
            varString = $"Pool Asteroids: {AsteroidsPool.Count}";
            e.Graphics.DrawString(varString, font, brush, X, Y);
            Y += 25;
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

            X -= labelFont.Size * FormatScore(Score).Length * 1.9f;
            Y -= labelFont.Size * 2.2f;

            DrawRotatedString(e, FormatScore(Score), valueFont, brush, X, Y,tilt);

            X = this.Bounds.Right - padding * 2 - FormatScore(HiScore).Length * valueFont.Size;
            Y = this.Bounds.Bottom - padding;

            DrawRotatedString(e, FormatScore(HiScore), valueFont, brush, X, Y, -tilt);

            Y -= valueFont.Size * 0.9f;
            X += labelFont.Size * FormatScore(Score).Length * 1.7f;

            DrawRotatedString(e, "HISCORE", labelFont, brush, X, Y, -tilt);

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
}
