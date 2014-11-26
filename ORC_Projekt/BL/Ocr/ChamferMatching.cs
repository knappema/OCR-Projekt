using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ORC_Projekt.BL.Ocr
{
    class ChamferMatching
    {
        private readonly uint[,] _distanceMap;
        private readonly List<int> _resultList;

        public ChamferMatching(uint[,] distanceMap)
        {
            _distanceMap = distanceMap;
            _resultList = new List<int>();
        }

        public List<int> ResultList
        {
            get
            {
                return _resultList;
            }
        }

        #region Publics

        public void Start()
        {
        }

        #endregion
    }
}
