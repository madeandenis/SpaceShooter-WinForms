using System.Drawing.Text;
using System.Media; 

namespace SpaceShooter.Utils
{
    internal class AssetLoader
    {
        public static Image LoadImage(string assetName)
        {

            string subPath = $"..\\..\\..\\Assets\\images\\" + assetName;
            string fullPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), subPath);
            
            return Image.FromFile(fullPath);
        }
        public static FontFamily LoadFont(string assetName)
        {

            string subPath = $"..\\..\\..\\Assets\\fonts\\" + assetName;
            string fullPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), subPath);
            
            PrivateFontCollection privateFonts = new PrivateFontCollection();
            privateFonts.AddFontFile(fullPath);
            
            return privateFonts.Families[0]; 
        }
        
        public static SoundPlayer LoadAudio(string assetName)
        {

            string subPath = $"..\\..\\..\\Assets\\sounds\\" + assetName;
            string fullPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), subPath);
            
            return new SoundPlayer(fullPath);
        }
    }
}
