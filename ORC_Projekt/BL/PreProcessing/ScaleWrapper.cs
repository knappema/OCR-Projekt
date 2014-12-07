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
        public static List<Bitmap> scaleImages(List<Bitmap> images, bool isTemplate = false)
        {
            int Height = 100;
            List<Bitmap> scaledImages = new List<Bitmap>();
            foreach(Bitmap image in images)
            {
                Bitmap scaledImage = resizeImage(image, Height);
                if (isTemplate)
                {
                    scaledImage = squareImage(scaledImage, Height);
                }
                else
                {
                    scaledImage = squareImage(scaledImage, Height, 3);
                }
                scaledImages.Add(scaledImage);
            }
            return scaledImages;
        }

        private static Bitmap squareImage(Bitmap scaledImage, int newHeight, int frameSize = 0)
        {
            int size = newHeight + 2 * frameSize;
            Bitmap squaredImage = new Bitmap(size, size);

            Graphics g = Graphics.FromImage((Image)squaredImage);

            SolidBrush b = new SolidBrush(Color.White);
            g.FillRectangle(b, 0, 0, size, size);
            g.DrawImage(scaledImage, (size - scaledImage.Width) / 2, (size - scaledImage.Height) / 2);
            
            g.Dispose();

            return (Bitmap)squaredImage;
        }

        private static Bitmap resizeImage(Bitmap imgToResize, int newHeight)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;

            float nPercentVertical = ((float)newHeight / (float)sourceHeight);
            float nPercentHorizontal = ((float)newHeight / (float)sourceWidth);
            if (nPercentVertical < nPercentHorizontal)
            {
                nPercent = nPercentVertical;
            }
            else
            {
                nPercent = nPercentHorizontal;
            }

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
