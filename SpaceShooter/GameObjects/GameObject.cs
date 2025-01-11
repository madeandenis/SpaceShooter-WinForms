using SpaceShooter.Utils;

namespace SpaceShooter.GameObjects
{
    internal class GameObject
    {
        protected readonly Form _parentForm;
        protected Point _position;
        protected float _speed;
        protected float _heading;
        protected Sprite _sprite;
        protected Boolean _visible;

        public GameObject(Form parentForm, Point position, float speed, float heading, Sprite sprite, Boolean visible = true)
        {
            _parentForm = parentForm;   
            Position = position;
            Speed = speed;
            Heading = heading;
            Sprite = sprite;
            Visible = visible;
        }

        public GameObject(Form parentForm, Point position, Sprite sprite, float speed = 0f, float heading = 0f, bool isVisible = true)
        {
            _parentForm = parentForm;
            Position = position;
            Speed = speed;
            Heading = heading;
            Sprite = sprite;
            Visible = isVisible;
        }

        public Point Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float Speed
        {
            get { return _speed; }
            set { _speed = TransformUtil.SpeedTransform.Adapt(value, _parentForm); }
        }

        public float Heading
        {
            get { return _heading; }
            set => _heading = value % 360; 
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
            Heading = obj.Heading;
            Sprite.Angle = Heading;
        }

        public virtual void Render(PaintEventArgs e)
        {
            if (Visible)
            {
                e.Graphics.DrawImage(TransformUtil.ImageTransform.Rotate(Sprite.Texture, Sprite.Angle), Position);
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
            double radians = Heading * (Math.PI / 180); 

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
            Heading = angleToTarget;
            Sprite.Angle = angleToTarget;
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
                _parentForm.ClientRectangle.X - offset,
                _parentForm.ClientRectangle.Y - offset,
                _parentForm.ClientRectangle.Width + (2 * offset),
                _parentForm.ClientRectangle.Height + (2 * offset)    
            );

            return !bounds.Contains(Position);
        }

        public enum CollisionShape
        {
            Rectangle, 
            Circle,    
        }

        public virtual bool Intersects(GameObject target, CollisionShape collisionShape = CollisionShape.Circle, int padding = 0)
        {
            switch (collisionShape)
            {
                case CollisionShape.Rectangle:
                    return InptersectsWithRectangle(target,padding);

                case CollisionShape.Circle:
                    return IntersectsWithCircle(target,padding);

                default:
                    throw new NotImplementedException($"Collision shape {collisionShape} not implemented.");
            }
        }
        
        private bool InptersectsWithRectangle(GameObject target, int padding)
        {
            Rectangle thisBox = new Rectangle(
                Position,
                new Size(Sprite.Texture.Width + padding, Sprite.Texture.Height + padding)
            );

            Rectangle targetBox = new Rectangle(
                target.Position,
                new Size(target.Sprite.Texture.Width, target.Sprite.Texture.Height)
            );

            return thisBox.IntersectsWith(targetBox);
        }

        private bool IntersectsWithCircle(GameObject target, int padding)
        {
            float distanceX = Position.X - target.Position.X;
            float distanceY = Position.Y - target.Position.Y;
            float distance = (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            float radiusSum = (Sprite.Texture.Width / 2) + (target.Sprite.Texture.Width / 2) + padding;
            return distance < radiusSum;
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
