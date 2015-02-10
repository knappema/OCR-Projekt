using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace OCR_Projekt.BL.Ocr
{
    /// <summary>
    /// Chamfer algorithm
    /// </summary>
    class DistanceTransformationChamfer
    {
        private Bitmap _inputImage;
        private Bitmap _currentImage;
        private Bitmap _currentTemplate;
        private uint[,] _currentDistanceMap;
        private bool _showColored = false;
        private bool _createTemplate = false;

        public DistanceTransformationChamfer(Bitmap binaryImage)
        {
            _inputImage = binaryImage;
            _currentImage = (Bitmap)binaryImage.Clone();
            _currentTemplate = (Bitmap)binaryImage.Clone();
            _currentDistanceMap = new uint[_inputImage.Width, _inputImage.Height];
        }

        public DistanceTransformationChamfer(Bitmap binaryImage, bool showColored, bool createTemplate)
            : this(binaryImage)
        {
            _showColored = showColored;
            _createTemplate = createTemplate;
        }


        #region Properties

        public Bitmap CurrentImage
        {
            get
            {
                return _currentImage;
            }
        }

        public Bitmap CurrentTemplate
        {
            get
            {
                return _currentTemplate;
            }
        }

        #endregion


        #region Publics

        public uint[,] start()
        {
            DistanceTransformation();
            return _currentDistanceMap;
        }

        #endregion


        #region Privates

        private void DistanceTransformation()
        {
            _currentDistanceMap = OcrHelper.CreateNewDistanceMap(_inputImage);

            ApplyMlMatrix();
            ApplyMrMatrix();

            UpdateCurrentImage(_currentDistanceMap);

            if (_createTemplate)
            {
                UpdateCurrentTemplate(_currentDistanceMap);
            }
        }


        private void UpdateCurrentImage(uint[,] map)
        {
            ForeachPixel(_currentImage, map, DistanceToBitmap);
        }

        private void UpdateCurrentTemplate(uint[,] map)
        {
            ForeachPixel(_currentTemplate, map, DistanceToTemplate);
        }

        private Color DistanceToBitmap(int x, int y, Bitmap bitmap, uint[,] distanceMap, uint maxValue)
        {
            int pixelValue;
            if (_showColored)
            {
                pixelValue = coloredPixelValueFromDistance(distanceMap[x, y], maxValue);
            }
            else
            {
                pixelValue = pixelValueFromDistanceWithPadding(distanceMap[x, y], maxValue);
            }

            Color newColor = Color.FromArgb(pixelValue);
            return newColor;
        }

        private Color DistanceToTemplate(int x, int y, Bitmap bitmap, uint[,] distanceMap, uint maxValue)
        {
            int pixelValue = pixelValueFromDistance(distanceMap[x, y]);

            Color newColor = Color.FromArgb(pixelValue);
            return newColor;
        }

        private int pixelValueFromDistanceWithPadding(uint distance, uint maxValue)
        {
            if (distance > uint.MaxValue)
            {
                throw new Exception(string.Format("DistanceTransformation: Distance too great - {0}", distance));
            }
            int pixelValue;

            byte byteDistance = 0;
            if (distance != 0)
            {
                byteDistance = (byte)((150 / (double)maxValue * distance) + 105);
            }

            pixelValue = unchecked((int)0xFF000000);
            pixelValue += byteDistance;
            pixelValue += byteDistance << 8;
            pixelValue += byteDistance << 16;

            return pixelValue;
        }

        private int pixelValueFromDistance(uint distance)
        {
            if (distance > byte.MaxValue)
            {
                throw new Exception(string.Format("DistanceTransformation: Distance too great - {0}", distance));
            }
            int pixelValue;

            byte byteDistance = 0;
            if (distance != 0)
            {
                byteDistance = (byte)distance;
            }

            pixelValue = unchecked((int)0xFF000000);
            pixelValue += byteDistance;
            pixelValue += byteDistance << 8;
            pixelValue += byteDistance << 16;

            return pixelValue;
        }

        private int coloredPixelValueFromDistance(uint distance, uint maxValue)
        {
            if (distance > uint.MaxValue)
            {
                throw new Exception(string.Format("DistanceTransformation: Distance too great - {0}", distance));
            }
            uint pixelValue;

            uint byteDistance = 0;
            pixelValue = unchecked((uint)0xFF000000);

            if (distance != 0)
            {
                byteDistance = (uint)(3 * 256 / (double)maxValue * distance);
                if (byteDistance < 256)
                {
                    pixelValue += 0xff;
                    pixelValue += (0xff - byteDistance) << 8;
                    pixelValue += 0 << 16;
                }
                else if (byteDistance < 2 * 256)
                {
                    byteDistance -= 256;
                    pixelValue += 0xff;
                    pixelValue += 0 << 8;
                    pixelValue += byteDistance << 16;
                }
                else
                {
                    byteDistance -= 2 * 256;
                    pixelValue += (0xff - byteDistance);
                    pixelValue += 0 << 8;
                    pixelValue += 0xff << 16;
                }
            }
            else
            {
                pixelValue += byteDistance;
                pixelValue += byteDistance << 8;
                pixelValue += byteDistance << 16;
            }
            

            return (int)pixelValue; 
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
