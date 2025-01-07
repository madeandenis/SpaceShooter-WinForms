namespace SpaceShooter.GameObjects
{
    internal class GameObjectPool<T> where T : GameObject
    {
        private Stack<T> _pool;
        private readonly Func<T> _objectFactory; 
        public int Count => _pool.Count;
        private readonly int _maxSize;

        public GameObjectPool(Func<T> objectFactory, int initialSize = 0, int maxSize = 1000)
        {
            _objectFactory = objectFactory;
            _maxSize = maxSize;

            _pool = new Stack<T>(initialSize);

            for (int i = 0; i < initialSize; i++)
            {
                _pool.Push(_objectFactory());
            }
        }

        public T Get()
        {
            if (_pool.Count > 0)
            {
                T obj = _pool.Pop();
                obj.Visible = true; 
                return obj;
            }
            else
            {
                return _objectFactory();
            }
        }

        public void Return(T obj)
        {
            if (obj == null)
                return;

            if (_pool.Count < _maxSize)
            {
                _pool.Push(obj);
            }
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                T obj = _pool.Pop();
            }
        }

    }
}
