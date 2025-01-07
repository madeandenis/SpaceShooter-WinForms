namespace SpaceShooter.Utils
{
    internal class VectorUtil
    {
        public static PointF GetDirection(PointF from, PointF to)
        {
            return new PointF(to.X - from.X, to.Y - from.Y);
        }

        public static PointF Normalize(PointF direction)
        {
            float magnitude = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            if (magnitude == 0)
                return new PointF(0, 0);

            return new PointF(direction.X / magnitude, direction.Y / magnitude);
        }
        public static float GetDistance(PointF p1, PointF p2)
        {
            PointF direction = GetDirection(p1, p2);
            return (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
        }

    }
}
