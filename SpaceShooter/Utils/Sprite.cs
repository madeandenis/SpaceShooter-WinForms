namespace SpaceShooter.Utils
{
    internal class Sprite
    {
        private Image _texture;
        private float _rotationAngle;
        private float _scale;

        public Sprite(Image texture, float rotationAngle = 0f, float scale = 1f)
        {
            Texture = texture;
            RotationAngle = rotationAngle % 360;
            Scale = scale;
        }

        public Image Texture 
        {
           get => _texture;
           set => _texture = value;
        }

        public float RotationAngle
        {
            get => _rotationAngle;
            set => _rotationAngle = value;
        }

        public float Scale
        {
            get => _scale;
            set
            {
                if(_scale != value)
                {
                    _scale = value;
                    Texture = TransformUtil.ResizeImage(Texture, _scale);
                }
            }
        }

    }
}
