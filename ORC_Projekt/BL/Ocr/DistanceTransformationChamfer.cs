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
        private uint[,] _currentDistanceMap;

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
            _currentDistanceMap = new uint[_inputImage.Width, _inputImage.Height];
        }

        public uint[,] start()
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


        private uint[,] CreateDistanceMap(Bitmap bitmap)
        {
            return GetGetDistanceMapFromBitmap(bitmap, GetDistanceMap);
        }

        private uint GetDistanceMap(int x, int y, Bitmap bitmap)
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

        private bool AreDifferent(int v1, int v2, int v3)
        {
            return !(v1 == v2 && v2 == v3 && v3 == v1);
        }


        private void UpdateCurrentBitmap(uint[,] map)
        {
            ForeachPixel(_currentImage, map, DistanceToBitmap);
        }

        private Color DistanceToBitmap(int x, int y, Bitmap bitmap, uint[,] distanceMap, uint maxValue)
        {
            int pixelValue = pixelValueFromDistance(distanceMap[x, y], maxValue);
            Color newColor = Color.FromArgb(pixelValue);
            return newColor;
        }

        private int pixelValueFromDistance(uint distance, uint maxValue)
        {
            if (distance > uint.MaxValue)
            {
                throw new Exception(string.Format("DistanceTransformation: Distance too great - {0}", distance));
            }
            int pixelValue;

            byte byteDistance = (byte)(0xff / (double)maxValue * distance);

            pixelValue = unchecked((int)0xFF000000);
            pixelValue += byteDistance;
            pixelValue += byteDistance << 8;
            pixelValue += byteDistance << 16;

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

        private void ForeachPixel(Bitmap bitmap, uint[,] distanceMap, Func<int, int, Bitmap, uint[,], uint, Color> pixelModification)
        {
            uint maxValue = 0;
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

        private void ForeachPixel(uint[,] distanceMap, Func<int, int, uint[,], uint> pixelModification)
        {
            for (int x = 0; x < distanceMap.GetLength(0); x++)
            {
                for (int y = 0; y < distanceMap.GetLength(1); y++)
                {
                    distanceMap[x, y] = pixelModification(x, y, distanceMap);
                }
            }
        }

        private void ForeachPixelReverse(uint[,] distanceMap, Func<int, int, uint[,], uint> pixelModification)
        {
            for (int x = distanceMap.GetLength(0) - 1; x >= 0; x--)
            {
                for (int y = distanceMap.GetLength(1) - 1; y >= 0; y--)
                {
                    distanceMap[x, y] = pixelModification(x, y, distanceMap);
                }
            }
        }

        private uint[,] GetGetDistanceMapFromBitmap(Bitmap bitmap, Func<int, int, Bitmap, uint> pixelModification)
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


        private void ApplyMlMatrix()
        {
            ForeachPixel(_currentDistanceMap, CalcMlMatrixValue);
        }

        private uint CalcMlMatrixValue(int x, int y, uint[,] distanceMap)
        {
            uint max = uint.MaxValue;
            if (distanceMap[x, y] > 0)
            {
                uint d1 = max;
                uint d2 = max;
                uint d3 = max;
                uint d4 = max;
                if (x - 1 >= 0)
                {
                    ulong temp = 1 + (ulong)distanceMap[x - 1, y];
                    if (temp <= max)
                    {
                        d1 = (uint)temp;
                    }
                }
                if (x - 1 >= 0 && y - 1 >=0)
                {
                    ulong temp = 2 + (ulong)distanceMap[x - 1, y - 1];
                    if (temp <= max)
                    {
                        d2 = (uint)temp;
                    }
                }
                if (y - 1 >= 0)
                {
                    ulong temp = 1 + (ulong)distanceMap[x, y - 1];
                    if (temp <= max)
                    {
                        d3 = (uint)temp;
                    }
                }
                if (x + 1 < distanceMap.GetLength(0) && y - 1 >= 0)
                {
                    ulong temp = 2 + (ulong)distanceMap[x + 1, y - 1];
                    if (temp <= max)
                    {
                        d4 = (uint)temp;
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

        private uint CalcMrMatrixValue(int x, int y, uint[,] distanceMap)
        {
            uint max = uint.MaxValue;
            if (distanceMap[x, y] > 0)
            {
                uint d1 = max;
                uint d2 = max;
                uint d3 = max;
                uint d4 = max;
                if (x + 1 < distanceMap.GetLength(0))
                {
                    ulong temp = 1 + (ulong)distanceMap[x + 1, y];
                    if (temp <= max)
                    {
                        d1 = (uint)temp;
                    }
                }
                if (x + 1 < distanceMap.GetLength(0) && y + 1 < distanceMap.GetLength(1))
                {
                    ulong temp = 2 + (ulong)distanceMap[x + 1, y + 1];
                    if (temp <= max)
                    {
                        d2 = (uint)temp;
                    }
                }
                if (y + 1 < distanceMap.GetLength(1))
                {
                    ulong temp = 1 + (ulong)distanceMap[x, y + 1];
                    if (temp <= max)
                    {
                        d3 = (uint)temp;
                    }
                }
                if (x - 1 >= 0 && y + 1 < distanceMap.GetLength(1))
                {
                    ulong temp = 2 + (ulong)distanceMap[x - 1, y + 1];
                    if (temp <= max)
                    {
                        d4 = (uint)temp;
                    }
                }
                return Min(distanceMap[x, y], d1, d2, d3, d4);
            }
            return distanceMap[x, y];
        }

        private uint Min(params uint[] values)
        {
            uint min = uint.MaxValue;
            foreach (uint value in values)
            {
                min = Math.Min(min, value);
            }
            return min;
        }

        #endregion
    }
}
