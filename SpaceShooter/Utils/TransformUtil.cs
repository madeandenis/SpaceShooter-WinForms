using System.Drawing.Drawing2D;

namespace SpaceShooter.Utils
{
    internal class TransformUtil
    {
        public static class ImageTransform
        {

            public static Image Rotate(Image img, float rotationAngle)
            {
                //create an empty Bitmap image
                Bitmap bmp = new Bitmap(img.Width, img.Height);

                //turn the Bitmap into a Graphics object
                Graphics gfx = Graphics.FromImage(bmp);

                //now we set the rotation point to the center of our image
                gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

                //now rotate the image
                gfx.RotateTransform(rotationAngle);

                gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

                //set the InterpolationMode to HighQualityBicubic so to ensure a high
                //quality image once it is transformed to the specified size
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

                //now draw our new image onto the graphics object
                gfx.DrawImage(img, new Point(0, 0));

                //dispose of our Graphics object
                gfx.Dispose();

                //return the image
                return bmp;
            }

            public static Image Resize(Image img, float scaleFactor)
            {
                int newWidth = (int)(img.Width * scaleFactor);
                int newHeight = (int)(img.Height * scaleFactor);

                Bitmap bmp = new Bitmap(newWidth, newHeight);

                Graphics gfx = Graphics.FromImage(bmp);

                // Set the InterpolationMode to HighQualityBicubic for smooth scaling
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Draw the scaled image onto the Graphics object
                gfx.DrawImage(img, new Rectangle(0, 0, newWidth, newHeight));

                return bmp;
            }

            public static Image Adapt(Image img, Form form)
            {
                float scalingFactor = ScreenUtil.GetScreenScalingFactor(form);
                return Resize(img, scalingFactor);
            }
        }

        public static class SpeedTransform
        {
            public static float Adapt(float speed, Form form)
            {
                float scalingFactor = ScreenUtil.GetScreenScalingFactor(form);
                return scalingFactor * speed;
            }
        }

    }
}
