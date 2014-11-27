using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace ORC_Projekt.BL.PreProcessing
{
    class ScaleWrapper
    {   
        public static List<Bitmap> scaleImages(List<Bitmap> images)
        {
            int Height = 100;
            List<Bitmap> scaledImages = new List<Bitmap>();
            foreach(Bitmap image in images)
            {
                Bitmap scaledImage = resizeImage(image, Height);
                scaledImages.Add(scaledImage);
            }
            return scaledImages;
        }

        private static Bitmap resizeImage(Bitmap imgToResize, int newHeight)
        {
           int sourceWidth = imgToResize.Width;
           int sourceHeight = imgToResize.Height;

           float nPercent = 0;

           nPercent = ((float)newHeight / (float)sourceHeight);

           int destWidth = (int)(sourceWidth * nPercent);
           int destHeight = (int)(sourceHeight * nPercent);

           Bitmap b = new Bitmap(destWidth, destHeight);
           Graphics g = Graphics.FromImage((Image)b);
           g.InterpolationMode = InterpolationMode.HighQualityBicubic;

           g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
           g.Dispose();

           return (Bitmap)b;
        }
    }
}
