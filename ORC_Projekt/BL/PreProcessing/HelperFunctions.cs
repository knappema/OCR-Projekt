using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ORC_Projekt.BL.PreProcessing
{
    class HelperFunctions
    {
        public static int GetGrayScaleFromColor(Color c)
        {
            return (int)((c.R + c.G + c.B) / 3);
        }
        public static void SafeBitmapsToDisk(List<Bitmap> images)
        {
            int i = 1;
            foreach(Bitmap image in images)
            {
                image.Save("image" + i++ + ".png");
            }
        }
        public static void SafeBitmapToDisk(Bitmap image)
        {
            image.Save("image" + DateTime.Now.Second + ".png");
        }
    }
}
