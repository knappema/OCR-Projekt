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
        private readonly uint[,] _originalDistanceMap;
        private readonly List<Template> _resultList;
        private readonly ConfigModel _config;
        private readonly List<Template> _matchingList;

        private int _originImageAdd = 3;    // min = 1
        private int _imageAdd = 1;   // min = 1


        

        public ChamferMatching(uint[,] distanceMap, ConfigModel config)
        {
            _originalDistanceMap = distanceMap;
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
                uint[,] currentTemplateDistanceMap = OcrHelper.GetDistanceMap(currentTemplateImage);

                // Matching Original with Template
                var absoluteMin = TemplateOnOriginalMatching(currentTemplateDistanceMap);
                template.OriginalWithTemplateMatching = absoluteMin;

                // Matching Template with Original
                var absoluteMin2 = OriginalOnTemplateMatching(currentTemplateDistanceMap);
                template.TemplateWithOriginalMatching = absoluteMin2;

                //  Result = Average of matchings
                template.Average = (template.OriginalWithTemplateMatching + template.TemplateWithOriginalMatching) / 2.0;
                template.Differenz = (Math.Abs(template.OriginalWithTemplateMatching - template.TemplateWithOriginalMatching) + template.Average) / 2.0;
            }

            var tempList = new List<Template>(_matchingList.OrderBy(x => x.Average));
            var tempList2 = new List<Template>(tempList.OrderBy(x => x.Differenz));

            //_matchingList.Sort((x, y) => Convert.ToInt32(x.Average - y.Average));
            //_matchingList.Sort((x, y) => Convert.ToInt32(x.Differenz - y.Differenz));
            //for (int i = 0; i < _matchingList.Count; i++)
            //{
            //    _resultList.Add(_matchingList[i]);
            //}

            _resultList.AddRange(tempList2);
        }

        private double TemplateOnOriginalMatching(uint[,] currentTemplateImage)
        {
            if (currentTemplateImage.GetLength(1) != _originalDistanceMap.GetLength(1) || currentTemplateImage.GetLength(0) != _originalDistanceMap.GetLength(0))
            {
                throw new Exception("Tempate is unequal to original image");
            }

            double currentDistance = DistanceMatching(currentTemplateImage, _originalDistanceMap);

            return currentDistance;
        }

        private double OriginalOnTemplateMatching(uint[,] currentTemplateImage)
        {
            if (currentTemplateImage.GetLength(1) != _originalDistanceMap.GetLength(1) || currentTemplateImage.GetLength(0) != _originalDistanceMap.GetLength(0))
            {
                throw new Exception("Original image is unequal to template");
            }

            double currentDistance = DistanceMatching(_originalDistanceMap, currentTemplateImage);

            return currentDistance;
        }

        private double DistanceMatching(uint[,] template, uint[,] distanceMap)
        {
            double currentDistance = double.MaxValue;

            // iterate over distance map
            long currentSum = 0;
            long foregroundPixelCount = 0;
            uint black = 0; // black

            for (int x = 0; x < template.GetLength(0); x += _imageAdd)
            {
                for (int y = 0; y < template.GetLength(1); y += _imageAdd)
                {
                    var currentPixel = template[x, y];
                    if (currentPixel == black)
                    {
                        foregroundPixelCount++;
                        currentSum += distanceMap[x, y] * distanceMap[x, y] * distanceMap[x, y] / 10; //*distanceMap[x, y];
                        //currentSum += (long)((Math.Pow(2.0, distanceMap[x, y] * 0.4) * 0.2) - 0.2); //*distanceMap[x, y];
                    }
                }
            }
            if (foregroundPixelCount > 0)
            {
                currentDistance = (currentSum / (double)foregroundPixelCount);
            }
            return currentDistance;
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

                _matchingList.Add(new Template(path, int.Parse(parts[1])));
            }
        }

        private bool AreDifferent(int v1, int v2, int v3)
        {
            return !(v1 == v2 && v2 == v3 && v3 == v1);
        }


        #endregion
    }
}
