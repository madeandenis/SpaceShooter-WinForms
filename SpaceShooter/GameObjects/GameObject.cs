using SpaceShooter.Utils;

namespace SpaceShooter.GameObjects
{
    internal class GameObject
    {
        protected readonly Form form;
        protected Point _position;
        protected float _speed;
        protected float _rotationAngle;
        protected Sprite _sprite;
        protected Boolean _visible;

        public GameObject(Form form, Point position, float speed, float rotationAngle, Sprite sprite, Boolean visible = true)
        {
            this.form = form;   
            Position = position;
            Speed = speed;
            RotationAngle = rotationAngle;
            Sprite = sprite;
            Visible = visible;
        }

        public GameObject(Form form, Point position, Sprite sprite, float speed = 0f, float rotationAngle = 0f, bool isVisible = true)
        {
            this.form = form;
            this.Position = position;
            this.Speed = speed;
            this.RotationAngle = rotationAngle;
            this.Sprite = sprite;
            this.Visible = isVisible;
        }

        public Point Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float Speed
        {
            get { return _speed; }
            set { _speed = TransformUtil.AdaptSpeed(value, form); }
        }

        public float RotationAngle
        {
            get { return _rotationAngle; }
            set
            {
                _rotationAngle = value % 360; 
                if (_rotationAngle < 0) _rotationAngle += 360; 
            }
        }
        public Sprite Sprite { 
            get { return _sprite; }
            set { _sprite = value; }
        }
        
        public Boolean Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public void MatchTransform(GameObject obj, int offsetX = 0, int offsetY = 0)
        {
            Position = new Point(obj.Position.X + offsetX, obj.Position.Y + offsetY);
            RotationAngle = obj.RotationAngle;
            Sprite.RotationAngle = RotationAngle;
        }


        public virtual void Render(PaintEventArgs e)
        {
            if (Visible)
            {
                e.Graphics.DrawImage(TransformUtil.RotateImage(Sprite.Texture, Sprite.RotationAngle), Position);
            }
        }

        public virtual void MoveTowardsTarget(Point targetPosition)
        {

            PointF direction = VectorUtil.GetDirection(Position, targetPosition);
            float distance = VectorUtil.GetDistance(Position, targetPosition);

            if (distance < 50) return;

            direction = VectorUtil.Normalize(direction);

            Position = new Point(
                (int)(Position.X + direction.X * Speed),
                (int)(Position.Y + direction.Y * Speed)
            );
        }

        public virtual void MoveInOrientation()
        {
            double radians = RotationAngle * (Math.PI / 180); 

            // Calculate the direction vector 
            float directionX = (float)Math.Cos(radians);  // X component (horizontal)
            float directionY = (float)Math.Sin(radians);  // Y component (vertical)

            PointF direction = new PointF(directionX, directionY);
            direction = VectorUtil.Normalize(direction);  // necessary to get consistent speed

            // Update the position by moving in the calculated direction
            Position = new Point(
                (int)(Position.X + direction.X * Speed),
                (int)(Position.Y + direction.Y * Speed)
            );
        }

        public virtual void RotateTowardsTarget(Point targetPosition)
        {
            float angleToTarget = AngleToTarget(targetPosition);
            RotationAngle = angleToTarget;
            Sprite.RotationAngle = angleToTarget;
        }
        
        public virtual float AngleToTarget(Point targetPosition)
        {
            // arctangent function that takes into account the signs of dx and dy
            double angleRadians = Math.Atan2(
                targetPosition.Y - Position.Y,
                targetPosition.X - Position.X
            );
            double angleDegrees = angleRadians * (180 / Math.PI);
            return (float)angleDegrees;
        }

        public virtual bool IsOutsideOfBounds(int offset = 0)
        {
            Rectangle bounds = new Rectangle(
                form.ClientRectangle.X - offset,
                form.ClientRectangle.Y - offset,
                form.ClientRectangle.Width + (2 * offset),
                form.ClientRectangle.Height + (2 * offset)    
            );

            return !bounds.Contains(Position);
        }

        public void MoveToCursor(MouseEventArgs e)
        {
            Position = new Point(
                e.X - Sprite.Texture.Width / 2,
                e.Y - Sprite.Texture.Height / 2
            );
        }
    }
}
