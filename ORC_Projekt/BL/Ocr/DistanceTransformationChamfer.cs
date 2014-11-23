using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ORC_Projekt.BL.Ocr
{
    /// <summary>
    /// Chamfer algorithm
    /// </summary>
    class DistanceTransformationChamfer
    {
        private Bitmap _inputImage;
        private Bitmap _currentImage;
        private byte[,] _currentDistanceMap;

        #region Properties

        public Bitmap CurrentImage
        {
            get
            {
                return _currentImage;
            }
        }

        #endregion

        #region Publics

        public DistanceTransformationChamfer(Bitmap binaryImage)
        {
            _inputImage = (Bitmap)binaryImage.Clone();
            _currentImage = (Bitmap)binaryImage.Clone();
            _currentDistanceMap = new byte[_inputImage.Width, _inputImage.Height];
        }

        public byte[,] start()
        {
            DistanceTransformation();
            return _currentDistanceMap;
        }

        #endregion

        #region Privates

        private void DistanceTransformation()
        {
            _currentDistanceMap = CreateDistanceMap(_inputImage);

            ApplyMlMatrix();
            ApplyMrMatrix();

            UpdateCurrentBitmap(_currentDistanceMap);
        }


        private byte[,] CreateDistanceMap(Bitmap bitmap)
        {
            return GetGetDistanceMapFromBitmap(bitmap, GetDistanceMap);
        }

        private byte GetDistanceMap(int x, int y, Bitmap bitmap)
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

            return Convert.ToByte(v1);
        }

        private bool AreDifferent(int v1, int v2, int v3)
        {
            return !(v1 == v2 && v2 == v3 && v3 == v1);
        }


        private void UpdateCurrentBitmap(byte[,] map)
        {
            ForeachPixel(_currentImage, map, DistanceToBitmap);
        }

        private Color DistanceToBitmap(int x, int y, Bitmap bitmap, byte[,] distanceMap, byte maxValue)
        {
            int pixelValue = pixelValueFromDistance(distanceMap[x, y], maxValue);
            // colors 
            //int resutlPixelValue = (int)(256 / (double)maxValue * pixelValue);
            Color newColor = Color.FromArgb(pixelValue);
            return newColor;
        }

        private int pixelValueFromDistance(int distance, byte maxValue)
        {
            if (distance > 0xff)
            {
                throw new Exception(string.Format("DistanceTransformation: Distance too great - {0}", distance));
            }
            int pixelValue;

            distance = (int)(0xff / (double)maxValue * distance);

            pixelValue = unchecked((int)0xFF000000);
            pixelValue += distance;
            pixelValue += distance << 8;
            pixelValue += distance << 16;

            return pixelValue;
        }

        private int pixelGreyValueFromDistance(int distance)
        {
            if (distance > 0xff)
            {
                throw new Exception(string.Format("DistanceTransformation: Distance too great - {0}", distance));
            }
            int pixelValue;

            pixelValue = unchecked((int)0xFF000000);
            pixelValue += distance;
            pixelValue += distance << 8;
            pixelValue += distance << 16;

            return pixelValue;
        }
            

        private void ForeachPixel(Bitmap bitmap, Func<int, int, Bitmap, Color> pixelModification)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    bitmap.SetPixel(x, y, pixelModification(x, y, bitmap));
                }
            }
        }

        private void ForeachPixel(Bitmap bitmap, byte[,] distanceMap, Func<int, int, Bitmap, byte[,], byte, Color> pixelModification)
        {
            byte maxValue = 0;
            for (int x = 0; x < distanceMap.GetLength(0); x++)
            {
                for (int y = 0; y < distanceMap.GetLength(1); y++)
                {
                    maxValue = Math.Max(maxValue, distanceMap[x, y]);
                }
            }

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    bitmap.SetPixel(x, y, pixelModification(x, y, bitmap, distanceMap, maxValue));
                }
            }
        }

        private void ForeachPixel(byte[,] distanceMap, Func<int, int, byte[,], byte> pixelModification)
        {
            for (int x = 0; x < distanceMap.GetLength(0); x++)
            {
                for (int y = 0; y < distanceMap.GetLength(1); y++)
                {
                    distanceMap[x, y] = pixelModification(x, y, distanceMap);
                }
            }
        }

        private void ForeachPixelReverse(byte[,] distanceMap, Func<int, int, byte[,], byte> pixelModification)
        {
            for (int x = distanceMap.GetLength(0) - 1; x >= 0; x--)
            {
                for (int y = distanceMap.GetLength(1) - 1; y >= 0; y--)
                {
                    distanceMap[x, y] = pixelModification(x, y, distanceMap);
                }
            }
        }

        private byte[,] GetGetDistanceMapFromBitmap(Bitmap bitmap, Func<int, int, Bitmap, byte> pixelModification)
        {
            var map = new byte[bitmap.Width, bitmap.Height];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    map[x, y] = pixelModification(x, y, bitmap);
                }
            }
            return map;
        }    


        private void ApplyMlMatrix()
        {
            ForeachPixel(_currentDistanceMap, CalcMlMatrixValue);
        }

        private byte CalcMlMatrixValue(int x, int y, byte[,] distanceMap)
        {
            byte max = 0xff;
            if (distanceMap[x, y] > 0)
            {
                byte d1 = max;
                byte d2 = max;
                byte d3 = max;
                byte d4 = max;
                if (x - 1 >= 0)
                {
                    int temp = 1 + distanceMap[x - 1, y];
                    if (temp <= max)
                    {
                        d1 = (byte)temp;
                    }
                }
                if (x - 1 >= 0 && y - 1 >=0)
                {
                    int temp = 2 + distanceMap[x - 1, y - 1];
                    if (temp <= max)
                    {
                        d2 = (byte)temp;
                    }
                }
                if (y - 1 >= 0)
                {
                    int temp = 1 + distanceMap[x, y - 1];
                    if (temp <= max)
                    {
                        d3 = (byte)temp;
                    }
                }
                if (x + 1 < distanceMap.GetLength(0) && y - 1 >= 0)
                {
                    int temp = 2 + distanceMap[x + 1, y - 1];
                    if (temp <= max)
                    {
                        d4 = (byte)temp;
                    }
                }
                return Min(d1, d2, d3, d4);
            }
            return distanceMap[x, y];
        }

        private void ApplyMrMatrix()
        {
            ForeachPixelReverse(_currentDistanceMap, CalcMrMatrixValue);
        }

        private byte CalcMrMatrixValue(int x, int y, byte[,] distanceMap)
        {
            byte max = 0xff;
            if (distanceMap[x, y] > 0)
            {
                byte d1 = max;
                byte d2 = max;
                byte d3 = max;
                byte d4 = max;
                if (x + 1 < distanceMap.GetLength(0))
                {
                    int temp = 1 + distanceMap[x + 1, y];
                    if (temp <= max)
                    {
                        d1 = (byte)temp;
                    }
                }
                if (x + 1 < distanceMap.GetLength(0) && y + 1 < distanceMap.GetLength(1))
                {
                    int temp = 2 + distanceMap[x + 1, y + 1];
                    if (temp <= max)
                    {
                        d2 = (byte)temp;
                    }
                }
                if (y + 1 < distanceMap.GetLength(1))
                {
                    int temp = 1 + distanceMap[x, y + 1];
                    if (temp <= max)
                    {
                        d3 = (byte)temp;
                    }
                }
                if (x - 1 >= 0 && y + 1 < distanceMap.GetLength(1))
                {
                    int temp = 2 + distanceMap[x - 1, y + 1];
                    if (temp <= max)
                    {
                        d4 = (byte)temp;
                    }
                }
                return Min(distanceMap[x, y], d1, d2, d3, d4);
            }
            return distanceMap[x, y];
        }

        private byte Min(params byte[] values)
        {
            byte min = 0xff;
            foreach (byte value in values)
            {
                min = Math.Min(min, value);
            }
            return min;
        }

        #endregion
    }
}
