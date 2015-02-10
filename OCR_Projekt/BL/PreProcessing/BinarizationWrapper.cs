using OtsuThreshold;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace OCR_Projekt.BL.PreProcessing
{
    class BinarizationWrapper
    {
        /// <summary>
        /// Establish a threshold by taking the average gray scale values of all pixels
        /// </summary>
        private static int EstablishThreshold(Bitmap Bmp)
        {
            int grayScale;

            int grayScaleTotal = 0;
            int amoutOfPixels = 0;

            for (int y = 0; y < Bmp.Height; y++)
            { 
                for (int x = 0; x < Bmp.Width; x++)
                {
                    grayScale = HelperFunctions.GetGrayScaleFromColor(Bmp.GetPixel(x, y));
                    grayScaleTotal += grayScale;
                    amoutOfPixels++;
                }
            }
            return (int)(grayScaleTotal / amoutOfPixels);
        }

        /// <summary>
        /// Binarize the image (all pixels are either 0 or 255)
        /// </summary>
        public static Bitmap Binarize(Bitmap Bmp)
        {
            Bmp = (Bitmap)Bmp.Clone();
            //int threshold = EstablishThreshold(Bmp);
            Otsu o = new Otsu();
            int threshold = o.getOtsuThreshold(Bmp);

            int grayScale;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    grayScale = grayScale = HelperFunctions.GetGrayScaleFromColor(Bmp.GetPixel(x, y));
                    if (grayScale < threshold)
                    {
                        grayScale = 0;
                    }
                    else
                    {
                        grayScale = 255;
                    }

                    Bmp.SetPixel(x, y, Color.FromArgb(grayScale, grayScale, grayScale));
                }
            return Bmp;
        }

    }
}
