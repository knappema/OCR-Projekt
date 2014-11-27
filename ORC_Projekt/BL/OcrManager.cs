using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using ORC_Projekt.BL.Ocr;
using ORC_Projekt.BL.PreProcessing;

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
        private string _currentStep;
        

        public OcrManager(string fileName, ConfigModel config)
        {
            Config = config;
            _originalImage = new Bitmap(fileName);
            CurrentImage = new Bitmap(fileName);
            _currentWorkingImage = CurrentImage;
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

        public String CurrentStep
        {
            get
            {
                return _currentStep;
            }
            private set
            {
                if (_currentStep != value)
                {
                    _currentStep = value;
                    OnPropertyChanged("CurrentStep");
                }
            }
        }


        public ConfigModel Config { get; set; }


        #endregion

        #region Publics

        /// <summary>
        /// Starts the ocr.
        /// </summary>
        public void Start()
        {
            Reset();
            PreProcessing();
            //Ocr();
            PostProcessing();
        }

        #endregion

        #region Privates

        private void Reset()
        {
            CurrentImage = new Bitmap(OriginalImage);
        }

        /// <summary>
        /// All methods for pre-processing
        /// </summary>
        private List<Bitmap> PreProcessing()
        {

            CurrentImage = BinarizationWrapper.Binarize(new Bitmap(_currentWorkingImage));
            _currentWorkingImage = CurrentImage;
            CurrentStep = "Binary Image";
           
            CurrentImage = ThinningWrapper.Thin(new Bitmap(_currentWorkingImage));
            _currentWorkingImage = CurrentImage;
            CurrentStep = "Thinned Image";
            HelperFunctions.SafeBitmapToDisk(CurrentImage);

            CurrentImage = CharacterIsolationWrapper.VisualizeBoxing(new Bitmap(_currentWorkingImage));
            List<Bitmap> chars = CharacterIsolationWrapper.IsolateCharacters(new Bitmap(_currentWorkingImage));
            CurrentStep = "Boxed";

            HelperFunctions.SafeBitmapToDisk(CurrentImage);
           
            List<Bitmap> scaledChars = ScaleWrapper.scaleImages(chars);
            HelperFunctions.SafeBitmapsToDisk(scaledChars);
                
            return scaledChars;
        }



        /// <summary>
        /// All methods for (in the moment) one specific ocr 
        /// </summary>
        private void Ocr()
        {
            var dtc = new DistanceTransformationChamfer(new Bitmap(CurrentImage), Config.ShowDistanceTransformationColored);
            var distanceMap = dtc.start();
            CurrentImage = dtc.CurrentImage;
            CurrentStep = "Distance Transformation";

        }

        /// <summary>
        /// All methods for post-processing
        /// </summary>
        private void PostProcessing()
        {
        }

       
        #endregion
    }
}
