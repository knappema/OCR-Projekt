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
        private static int counter = 0;


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
            Random rnd = new Random();
            string cur = Directory.GetCurrentDirectory();
            string path = Path.Combine(cur, "TemplateOutput");
            if (!Directory.Exists(path))
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, String.Format("template_X_{0}.png", rnd.Next() + counter++));
            image.Save(path);
        } 

        public static bool CheckIfImagesAreBW(List<Bitmap> images)
        {
            foreach(Bitmap image in images)
            {
               if (!CheckIfImageIsBW(image))
                   return false;
            }
            return true;
        }


        public static bool CheckIfImageIsBW(Bitmap image)
        {
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    if ((GetGrayScaleFromColor(image.GetPixel(x, y)) != 0) && (GetGrayScaleFromColor(image.GetPixel(x, y)) != 255))
                        return false;
                }
            return true;
        }

        public static void SafeTemplateListToDisk(List<Bitmap> list)
        {
            int i = 0;
            foreach (var image in list)
            {
                Random rnd = new Random();
                string cur = Directory.GetCurrentDirectory();
                string path = Path.Combine(cur, "TemplateOutput");
                if (!Directory.Exists(path))
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, String.Format("template_{0}_{1}.png", i++, rnd.Next() + (++counter)));
                image.Save(path);
            }
        }
    }
}
