using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceShooter.Utils
{
    internal class AssetLoader
    {
        public static Image Load(string assetName)
        {
            string subPath = "..\\..\\..\\Assets\\" + assetName;
            string fullPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), subPath);
            return Image.FromFile(fullPath);
        }
    }
}
