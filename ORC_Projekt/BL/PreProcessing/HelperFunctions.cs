using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;

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
            string time = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss");
            foreach(Bitmap image in images)
            {
                image.Save(String.Format("image{0}_{1}.png", time, i++));
            }
        }
        public static void SafeBitmapToDisk(Bitmap image)
        {
            image.Save(String.Format("image{0}.png", DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss")));
        }

        public static void SafeTemplateBitmapToDisk(Bitmap image)
        {
            string cur = Directory.GetCurrentDirectory();
            string path = Path.Combine(cur, "TemplateOutput");
            if (!Directory.Exists(path))
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, String.Format("image{0}.png", DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss")));
            image.Save(path);
            
        }
    }
}
