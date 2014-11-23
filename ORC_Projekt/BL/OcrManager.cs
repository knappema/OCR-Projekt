using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ORC_Projekt.BL
{
    public class OcrManager : OcrManagerBase
    {
        private readonly Bitmap _originalImage;
        /// <summary>
        /// Current image, which can be viewed in frontend.
        /// </summary>
        private Bitmap _currentImage;
        /// <summary>
        /// Current image, which will be used for modifications
        /// </summary>
        private Bitmap _currentWorkingImage;
        private string _resultText;

        public OcrManager(string fileName)
        {
            _originalImage = new Bitmap(fileName);
        }

        #region Properties

        public Bitmap OriginalImage
        {
            get
            {
                return _originalImage;
            }
        }

        public Bitmap CurrentImage
        {
            get
            {
                return _currentImage;
            }
            private set
            {
                if (_currentImage != value)
                {
                    _currentImage = value;
                    OnPropertyChanged("CurrentImage");
                }
            }
        }

        public String ResultText
        {
            get
            {
                return _resultText;
            }
            private set
            {
                if (_resultText != value)
                {
                    _resultText = value;
                    OnPropertyChanged("ResultText");
                }
            }
        }

        #endregion

        #region Publics

        /// <summary>
        /// Starts the ocr.
        /// </summary>
        public void Start()
        {
            //test
            CurrentImage = OriginalImage;
            ResultText = "Cheese";


            PreProcessing();
            Ocr();
            PostProcessing();
        }

        #endregion

        #region Privates

        /// <summary>
        /// All methods for pre-processing
        /// </summary>
        private void PreProcessing()
        {
            CurrentImage = Binarization(new Bitmap(CurrentImage), 128);
        }



        /// <summary>
        /// All methods for (in the moment) one specific ocr 
        /// </summary>
        private void Ocr()
        {
        }

        /// <summary>
        /// All methods for post-processing
        /// </summary>
        private void PostProcessing()
        {
        }

        private Bitmap Binarization(Bitmap Bmp, byte threshold)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)((c.R + c.G + c.B) / 3);
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
        #endregion
    }
}
