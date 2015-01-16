using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ORC_Projekt.BL.Ocr
{
    public static class OcrHelper
    {
        public static uint[,] CreateNewDistanceMap(Bitmap bitmap)
        {
            return GetGetDistanceMapFromBitmap(bitmap, GetNewDistanceMap);
        }

        public static uint[,] GetDistanceMap(Bitmap bitmap)
        {
            return GetGetDistanceMapFromBitmap(bitmap, GetDistanceMap);
        }

        private static uint[,] GetGetDistanceMapFromBitmap(Bitmap bitmap, Func<int, int, Bitmap, uint> pixelModification)
        {
            var map = new uint[bitmap.Width, bitmap.Height];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    map[x, y] = pixelModification(x, y, bitmap);
                }
            }
            return map;
        }    

        private static uint GetNewDistanceMap(int x, int y, Bitmap bitmap)
        {
            Color pixel = bitmap.GetPixel(x, y);
            int pixelValue = pixel.ToArgb();
            int v1 = pixelValue & 0x000000ff;
            int v2 = (pixelValue & 0x0000ff00) >> 8;
            int v3 = (pixelValue & 0x00ff0000) >> 16;

            if (AreDifferent(v1, v2, v3))
            {
                throw new Exception("DistanceTransformation: There is no binary image");
            }
            if (v1 > 0xff)
            {
                throw new Exception(string.Format("DistanceTransformation: Distance too great - {0}", v1));
            }

            uint result = uint.MaxValue;
            if (v1 == 0)
            {
                result = 0;
            }

            return result;
        }

        private static uint GetDistanceMap(int x, int y, Bitmap bitmap)
        {
            Color pixel = bitmap.GetPixel(x, y);
            int pixelValue = pixel.ToArgb();
            int v1 = pixelValue & 0x000000ff;
            int v2 = (pixelValue & 0x0000ff00) >> 8;
            int v3 = (pixelValue & 0x00ff0000) >> 16;

            if (AreDifferent(v1, v2, v3))
            {
                throw new Exception("DistanceTransformation: There is no binary image");
            }
            if (v1 > 0xff)
            {
                throw new Exception(string.Format("DistanceTransformation: Distance too great - {0}", v1));
            }

            return (uint)v1;
        }

        private static bool AreDifferent(int v1, int v2, int v3)
        {
            return !(v1 == v2 && v2 == v3 && v3 == v1);
        }


        //public static Bitmap RemoveRed(Bitmap input)
        //{
        //    //top
        //    for (int x = 0; x < input.Width; x++) 
        //    {
        //        for (int y = 0; y < 10; y++)
        //        {
        //            var c = input.GetPixel(x, 0);
        //            if (c == Color.Red)
        //            {
        //                input.SetPixel(x, 0, Color.White);
        //            }
        //        }

        //    }

        //    //botom
        //    for (int x = 0; x < input.Width; x++)
        //    {
        //        if (input.GetPixel(x, input.Height - 1) == Color.Red)
        //        {
        //            input.SetPixel(x, input.Height - 1, Color.White);
        //        }
        //    }

        //    //left
        //    for (int y = 0; y < input.Height; y++)
        //    {
        //        if (input.GetPixel(0, y) == Color.Red)
        //        {
        //            input.SetPixel(0, y, Color.White);
        //        }
        //    }

        //    //right
        //    for (int y = 0; y < input.Height; y++)
        //    {
        //        if (input.GetPixel(input.Width - 1, y) == Color.Red)
        //        {
        //            input.SetPixel(input.Width - 1, y, Color.White);
        //        }
        //    }

        //    return input;
        //}
    }
}
