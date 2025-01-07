using SpaceShooter.GameObjects;
using SpaceShooter.Utils;
using GameTimer = System.Windows.Forms.Timer;

namespace SpaceShooter
{
    public partial class SpaceShooter : Form
    {
        private int FPS = 60;
        private GameTimer gameTimer;
        private int tickCounter;
        private int asteroidSpawnerInterval = 3; // seconds

        private GameObject Spaceship;
        private GameObject TargetCursor;
        private GameObjectPool<GameObject> LaserBeamsPool;
        private GameObjectPool<Asteroid> AsteroidsPool;
        private List<GameObject> ActiveLaserBeams;
        private List<Asteroid> ActiveAsteroids;

        public SpaceShooter()
        {
            InitializeComponent();
            InitializeGameObjects();

            Cursor.Hide();
            this.DoubleBuffered = true;

            gameTimer = new GameTimer();
            gameTimer.Interval = 1000 / FPS;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        private void InitializeGameObjects()
        {
            Sprite shipSprite = new Sprite(AssetLoader.Load("player-ship.png"), scale: 0.6f);
            Sprite targetCursorSprite = new Sprite(AssetLoader.Load("green-target.png"), scale: 0.7f);
            Sprite laserBeamSprite = new Sprite(AssetLoader.Load("laser-beam.png"),scale: 2);
            Sprite asteroidSprite = new Sprite(AssetLoader.Load("asteroid.png"),scale: 0.6f);

            Point screenCenter = new Point(Width / 2, Height / 2);

            float playerSpeed = 15f;
            float laserBeamSpeed = 80f;
            float asteroidSpeed = 30f;

            Spaceship    = new GameObject(this, screenCenter, shipSprite, playerSpeed);
            TargetCursor = new GameObject(this, screenCenter, targetCursorSprite);
            
            Func<GameObject> LaserBeamFactory = () => new GameObject(this, screenCenter, laserBeamSprite, laserBeamSpeed);
            Func<Asteroid> AsteroidFactory  = () => new Asteroid(this, screenCenter, asteroidSprite, asteroidSpeed);

            LaserBeamsPool = new GameObjectPool<GameObject>(LaserBeamFactory, 50);
            ActiveLaserBeams = new List<GameObject>();
            
            AsteroidsPool = new GameObjectPool<Asteroid>(AsteroidFactory, 50);
            ActiveAsteroids = new List<Asteroid>();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            Spaceship.MoveTowardsTarget(TargetCursor.Position);

            UpdateActiveLaserBeams();
            UpdateActiveAsteroids();

            SpawnAsteroids();

            this.Invalidate(); // Trigger a repaint
            tickCounter++;
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
        }

        private void SpaceShooter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                SpawnLaserBeam();
            }
        }

        private void SpawnLaserBeam()
        {
            GameObject laserBeam = LaserBeamsPool.Get();
            laserBeam.MatchTransform(Spaceship);
            ActiveLaserBeams.Add(laserBeam);
        }

        private void SpawnAsteroids()
        {
            if (tickCounter % (asteroidSpawnerInterval * FPS) == 0)
            {
                Asteroid asteroid = AsteroidsPool.Get();
                asteroid.MoveOffScreen();
                asteroid.RotateTowardsInBounds();
                ActiveAsteroids.Add(asteroid);
            }
        }

        private void UpdateActiveAsteroids()
        {
            for (int i = ActiveAsteroids.Count - 1; i >= 0; i--)
            {
                GameObject obj = ActiveAsteroids[i];
                obj.MoveInOrientation();
                obj.Sprite.RotationAngle += 1f;
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

        private void RenderGameObjects<T>(List<T> gameObjects, PaintEventArgs e) where T : GameObject
        {
            foreach (var gameObject in gameObjects)
            {
                gameObject.Render(e);
            }
        }

    }
}
