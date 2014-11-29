using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using ORC_Projekt.BL.Ocr;
using ORC_Projekt.BL.PreProcessing;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;

namespace ORC_Projekt.BL
{
    public class OcrManager : NotifyPropertyChangedBase
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
        private BackgroundWorker _worker;
        private ManualResetEvent _waitEvent;


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

        /// <summary>
        /// Starts the ocr.
        /// </summary>
        public void StartAsync(BackgroundWorker worker, ManualResetEvent waitEvent)
        {
            _worker = worker;
            _waitEvent = waitEvent;
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
            _currentWorkingImage = new Bitmap(OriginalImage);
        }

        /// <summary>
        /// All methods for pre-processing
        /// </summary>
        private List<Bitmap> PreProcessing()
        {

            CurrentImage = BinarizationWrapper.Binarize(_currentWorkingImage);
            SetCurrentWorkingImage(CurrentImage);
            CurrentStep = "Binary Image";

            _worker.ReportProgress(20);
            WaitForResume();

            CurrentImage = ThinningWrapper.Thin(_currentWorkingImage);
            SetCurrentWorkingImage(CurrentImage);
            CurrentStep = "Thinned Image";
            SafeBitmapToDisk(CurrentImage);

            _worker.ReportProgress(40);
            WaitForResume();

            CurrentImage = CharacterIsolationWrapper.VisualizeBoxing(_currentWorkingImage);
            List<Bitmap> chars = CharacterIsolationWrapper.IsolateCharacters(_currentWorkingImage);
            CurrentStep = "Boxed";

            SafeBitmapToDisk(CurrentImage);
           
            List<Bitmap> scaledChars = ScaleWrapper.scaleImages(chars);
            HelperFunctions.SafeBitmapsToDisk(scaledChars);

            _worker.ReportProgress(60);
            WaitForResume();
                
            return scaledChars;
        }

        /// <summary>
        /// All methods for (in the moment) one specific ocr 
        /// </summary>
        private void Ocr()
        {
            SetCurrentWorkingImage(CurrentImage);
            var dtc = new DistanceTransformationChamfer(_currentWorkingImage, Config.ShowDistanceTransformationColored);
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


        #region Dispatcher Methods

        private void SetCurrentWorkingImage(Bitmap newCurrentImage)
        {
            DispatchIfNecessary(() =>
            {
                _currentWorkingImage = (Bitmap)CurrentImage.Clone();
            });
        }

        private void SafeBitmapToDisk(Bitmap CurrentImage)
        {
            DispatchIfNecessary(() =>
            {
                HelperFunctions.SafeBitmapToDisk(CurrentImage);
            });
        }

        private void DispatchIfNecessary(Action action)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (!dispatcher.CheckAccess())
                dispatcher.Invoke(action);
            else
                action.Invoke();
        }

        #endregion

        #region Thread

        private void WaitForResume()
        {
            CancellationPending();

            if (Config.ShowSteps == true)
            {
                _waitEvent.WaitOne();
                _waitEvent.Reset();
            }
        }

        private void CancellationPending()
        {
            if (_worker.CancellationPending)
            {
                throw new CancelException();
            }
        }

        #endregion
    }
}
