using SpaceShooter.Utils;

namespace SpaceShooter.GameObjects
{
    internal class Asteroid : GameObject
    {

        public float InitialSpeed {  get; private set; }
        public float TimeInactive { get; set; } = 0f;

        public Asteroid(Form form, Point position, float speed, float heading, Sprite sprite, bool visible = true)
            : base(form, position, speed, heading, sprite, visible)
        {
            InitialSpeed = speed;
        }

        public Asteroid(Form form, Point position, Sprite sprite, float speed = 0, float heading = 0, bool isVisible = true)
            : base(form, position, sprite, speed, heading, isVisible)
        {
            InitialSpeed = speed;
        }

        public void Reset()
        {
            TimeInactive = 0f;
            Speed = InitialSpeed;
        }

        public void SpawnOffScreen()
        {
            Random rand = new Random();
            int side = rand.Next(4);

            Rectangle screenBounds = _parentForm.ClientRectangle;
            int ObjectSize = Sprite.Texture.Width;

            float x = 0, y = 0;
            switch (side)
            {
                case 0: // Left 
                    x = -ObjectSize;
                    y = rand.Next((int)screenBounds.Top, (int)(screenBounds.Top + screenBounds.Height));
                    break;
                case 1: // Right 
                    x = screenBounds.Right + ObjectSize;
                    y = rand.Next((int)screenBounds.Top, (int)(screenBounds.Top + screenBounds.Height));
                    break;
                case 2: // Top 
                    x = rand.Next((int)screenBounds.Left, (int)(screenBounds.Left + screenBounds.Width));
                    y = -ObjectSize;
                    break;
                case 3: // Bottom 
                    x = rand.Next((int)screenBounds.Left, (int)(screenBounds.Left + screenBounds.Width));
                    y = screenBounds.Bottom + ObjectSize;
                    break;
            }

            Position = new Point((int)x, (int)y);
        }

        public void RotateTowardsInBounds()
        {
            float angleToTarget = AngleToTarget(RandomPositionInBounds(_parentForm.ClientRectangle));
            Heading = angleToTarget;
        }

        private Point RandomPositionInBounds(Rectangle rect, float areaPercentage = 0.3f)
        {
            Random rand = new Random();

            int centerX = rect.Left + rect.Width / 2;
            int centerY = rect.Top + rect.Height / 2;

            int smallAreaWidth = (int)(rect.Width * areaPercentage);
            int smallAreaHeight = (int)(rect.Height * areaPercentage);

            int x = rand.Next(centerX - smallAreaWidth / 2, centerX + smallAreaWidth / 2);
            int y = rand.Next(centerY - smallAreaHeight / 2, centerY + smallAreaHeight / 2);

            return new Point(x, y);
        }

        public void ApplyDeviation(GameObject target)
        {
            float minimumSpeed = 5f;
            float angleDiff = Math.Abs(Heading- target.Heading);

            if (angleDiff < 100) 
            {
                Speed += 5f;
                return;
            }

            // Slowing down target
            if ( Speed > minimumSpeed)
            {
                Speed -= 0.1f;
            }
            // Changing its direction
            else
            {
                Heading = target.Heading;
            }
        }
    }
}
