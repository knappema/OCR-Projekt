using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace ORC_Projekt.BL.Ocr
{
    class ChamferMatching
    {
        private readonly uint[,] _distanceMap;
        private readonly List<Template> _resultList;
        private readonly ConfigModel _config;
        private readonly List<Template> _matchingList;

        private int _originImageAdd = 40;
        private int _tempaltImageAdd = 4;


        

        public ChamferMatching(uint[,] distanceMap, ConfigModel config)
        {
            _distanceMap = distanceMap;
            _resultList = new List<Template>();
            _config = config;
            _matchingList = new List<Template>();
            GetTempaltes();
        }

        public List<Template> ResultList
        {
            get
            {
                return _resultList;
            }
        }

        #region Publics

        public void Start()
        {
            foreach (var template in _matchingList)
            {
                Bitmap currentTemplateImage = new Bitmap(template.Path);

                if (currentTemplateImage.Height > _distanceMap.GetLength(1) || currentTemplateImage.Width > _distanceMap.GetLength(0))
                {
                    throw new Exception("Tempate is bigger than distance map");
                }

                int absoluteMin = int.MaxValue;
                for (int originX = 0; originX < (_distanceMap.GetLength(0) - currentTemplateImage.Width); originX += _originImageAdd)
                {
                    for (int originY = 0; originY < (_distanceMap.GetLength(1) - currentTemplateImage.Height); originY += _originImageAdd)
                    {


                        // iterate over distance map
                        long currentPositionSum = 0;
                        long foregroundPixelCount = 0;
                        int black = Color.Black.ToArgb();

                        for (int x = 0; x < currentTemplateImage.Width; x += _tempaltImageAdd)
                        {
                            for (int y = 0; y < currentTemplateImage.Height; y += _tempaltImageAdd)
                            {
                                var currentPixel = currentTemplateImage.GetPixel(x, y).ToArgb();
                                if (currentPixel == black)
                                {
                                    foregroundPixelCount++;
                                    currentPositionSum += _distanceMap[x + originX, y + originY];
                                }
                            }
                        }
                        if (foregroundPixelCount > 0)
                        {
                            int currentDistance = (int)(currentPositionSum / foregroundPixelCount);
                            absoluteMin = Math.Min(absoluteMin, currentDistance);
                        }
                    }
                }

                template.Matching = absoluteMin;
            }

            _matchingList.Sort((x, y) => x.Matching - y.Matching);
            for (int i = 0; i < 3; i++)
            {
                _resultList.Add(_matchingList[i]);
            }

        }

        #endregion

        #region Privates

        private void GetTempaltes()
        {
            string[] filePaths = Directory.GetFiles(_config.TemplatePath);

            foreach (var path in filePaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                var parts = fileName.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

                _matchingList.Add(new Template(path, int.Parse(parts[0])));
            }
        }

        private bool AreDifferent(int v1, int v2, int v3)
        {
            return !(v1 == v2 && v2 == v3 && v3 == v1);
        }


        #endregion
    }
}
