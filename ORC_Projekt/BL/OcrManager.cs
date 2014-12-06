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
        public void StartAsync(BackgroundWorker worker, ManualResetEvent waitEvent)
        {
            _worker = worker;
            _waitEvent = waitEvent;
            Reset();

            var list = PreProcessing();
            ForeachImage(list);
            PostProcessing();
        }

        #endregion


        #region Privates

        private void Reset()
        {
            ResultText = String.Empty;
            CurrentImage = new Bitmap(OriginalImage);
            _currentWorkingImage = new Bitmap(OriginalImage);
        }

        /// <summary>
        /// All methods for pre-processing
        /// </summary>
        private List<Bitmap> PreProcessing()
        {
            Binarize();
            Stop(15);

            Thin();
            Stop(30);

            List<Bitmap> chars = Boxing();
            List<Bitmap> scaledChars = Scale(chars);
            Stop(50);

            return scaledChars;
        }

        private List<Bitmap> Scale(List<Bitmap> chars)
        {
            List<Bitmap> scaledChars = ScaleWrapper.scaleImages(chars, Config.CreateTemplate);
            SafeBitmapsToDisk(scaledChars);
            return scaledChars;
        }

        private List<Bitmap> Boxing()
        {
            CurrentStep = "Boxed Image";
            CurrentImage = CharacterIsolationWrapper.VisualizeBoxing(_currentWorkingImage);
            List<Bitmap> chars = CharacterIsolationWrapper.IsolateCharacters(_currentWorkingImage);
            SafeBitmapToDisk(CurrentImage);
            return chars;
        }

        private void Thin()
        {
            CurrentStep = "Thinned Image";
            CurrentImage = ThinningWrapper.Thin(_currentWorkingImage);
            SetCurrentWorkingImage(CurrentImage);
            SafeBitmapToDisk(CurrentImage);
        }

        private void Binarize()
        {
            CurrentStep = "Binary Image";
            CurrentImage = BinarizationWrapper.Binarize(_currentWorkingImage);
            SetCurrentWorkingImage(CurrentImage);
        }

        private void ForeachImage(List<Bitmap> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                CurrentImage = list[i];
                SetCurrentWorkingImage(CurrentImage);

                Binarize();

                Thin();

                CurrentStep = "Boxed Image";

                Stop(50 + (int)(50 / ((double)list.Count)) * i);

                if (!Config.CreateTemplate)
                {
                    Ocr(_currentWorkingImage, list, i);
                }
                else
                {
                    SafeTamplateBitmapToDisk(CurrentImage);
                    Thread.Sleep(1100);
                }
                Stop(50 + (int)(50 / ((double)list.Count)) * i);
            }
        }

        /// <summary>
        /// All methods for (in the moment) one specific ocr 
        /// </summary>
        private void Ocr(Bitmap item, List<Bitmap> list, int i = 0)
        {
            CurrentStep = "Distance Transformation";
            var dtc = new DistanceTransformationChamfer(item, Config.ShowDistanceTransformationColored);
            var distanceMap = dtc.start();
            CurrentImage = dtc.CurrentImage;

            Stop(50 + (int)(50 / ((double)list.Count)) * i);

            CurrentStep = "Chamfer Matching";
            var cm = new ChamferMatching(distanceMap, Config);
            cm.Start();
            ResultText += cm.ResultList[0].Value.ToString();
        }

        /// <summary>
        /// All methods for post-processing
        /// </summary>
        private void PostProcessing()
        {
            // nothing yet
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

        private void SafeBitmapsToDisk(List<Bitmap> scaledChars)
        {
            DispatchIfNecessary(() =>
            {
                HelperFunctions.SafeBitmapsToDisk(scaledChars);
            });
        }

        private void SafeBitmapToDisk(Bitmap currentImage)
        {
            DispatchIfNecessary(() =>
            {
                HelperFunctions.SafeBitmapToDisk(currentImage);
            });
        }

        private void SafeTamplateBitmapToDisk(Bitmap currentImage)
        {
            DispatchIfNecessary(() =>
            {
                HelperFunctions.SafeTemplateBitmapToDisk(currentImage);
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

        private void Stop(int currentProgress)
        {
            _worker.ReportProgress(currentProgress);
            WaitForResume();
        }

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
