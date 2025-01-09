namespace SpaceShooter.Utils
{
    internal class Sprite
    {
        private Form _parentForm;
        private Image _texture;
        private float _angle;
        private float _scale;

        public Sprite(Form parentForm, Image texture, float angle = 0f, float scale = 1f)
        {
            _parentForm = parentForm;
            Texture = texture;
            Angle = angle;
            Scale = scale;
        }

        public Sprite(Form parentForm, string texturePath, float angle = 0f, float scale = 1f)
          : this(parentForm, AssetLoader.LoadImage(texturePath), angle, scale)
        {
        }

        public Image Texture
        {
            get => _texture;
            set => _texture = TransformUtil.ImageTransform.Adapt(value, _parentForm);
        }

        public float Angle
        {
            get => _angle;
            set => _angle = value % 360;
        }

        public float Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    Texture = TransformUtil.ImageTransform.Resize(Texture, _scale);
                }
            }
        }

        public void Rotate(float angle)
        {
            _angle += angle;
        }

    }
}
