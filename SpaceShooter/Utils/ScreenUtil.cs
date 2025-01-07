public class ScreenUtil
{
    public static Screen GetActiveScreen(Form form)
    {
        return Screen.FromControl(form);
    }

    public static float GetScreenScalingFactor(Form form)
    {
        Screen activeScreen = GetActiveScreen(form);

        float activeScreenSize = (float)Math.Sqrt(
            Math.Pow(activeScreen.Bounds.Width, 2) + Math.Pow(activeScreen.Bounds.Height, 2)
        );

        int baseWidth = 4096;
        int baseHeight = 2160;

        float baseSize = (float)Math.Sqrt(
            Math.Pow(baseWidth, 2) + Math.Pow(baseHeight, 2)
        );

        float scalingFactor = activeScreenSize / baseSize;

        return scalingFactor;
    }
}
