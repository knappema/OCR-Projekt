using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ORC_Projekt.BL
{
    class BinarizationWrapper
    {
        private static byte EstablishThreshold(Bitmap Bmp)
        {
            int rgb;

            int sum = 0;
            int amoutOfPixels = 0;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    rgb = HelperFunctions.GetGrayScaleFromColor(Bmp.GetPixel(x, y));
                    sum += rgb;
                    amoutOfPixels++;
                }
            return (byte)(sum / amoutOfPixels);
        }

        public static Bitmap Binarize(Bitmap Bmp)
        {
            Bmp = new Bitmap(Bmp);
            byte threshold = EstablishThreshold(Bmp);

            int rgb;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    rgb = rgb = HelperFunctions.GetGrayScaleFromColor(Bmp.GetPixel(x, y));
                    if (rgb < threshold)
                    {
                        rgb = 0;
                    }
                    else
                    {
                        rgb = 255;
                    }
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));                 

                }
            return Bmp;
        }

    }
}
