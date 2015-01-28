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
        private Bitmap _currentTemplate;
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
            _currentTemplate = null;
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

            OCR();

            //if (Config.CreateTemplate)
            //{
            //    CreateTemplates();
            //}
            //else
            //{
            //    var list = PreProcessing();
            //    ForeachImage(list);
            //    PostProcessing();
            //}
        }

        #endregion


        #region Privates

        private void Reset()
        {
            ResultText = String.Empty;
            CurrentImage = new Bitmap(OriginalImage);
            _currentWorkingImage = new Bitmap(OriginalImage);
        }

        private void OCR()
        {
            Binarize();
            SetCurrentWorkingImage(CurrentImage);
            Stop(10);

            List<Bitmap> chars = Boxing();
            Stop(20);

            chars = Thin(chars);
            Stop(20);

            List<Bitmap> scaledChars = Scale(chars);
            Stop(20);

            bool bw = HelperFunctions.CheckIfImagesAreBW(chars);

            bool createTemplatesLater = false;
            List<Bitmap> transformedTemplates = new List<Bitmap>();
            if (scaledChars.Count == 10 && Config.CreateTemplate)
            {
                createTemplatesLater = true;
            }

            int i = 1;
            foreach (var bitmap in scaledChars)
            {
                CurrentImage = bitmap;
                SetCurrentWorkingImage(bitmap);
                Stop((int)(70.0 / scaledChars.Count) * i);

                Binarize();
                SetCurrentWorkingImage(CurrentImage);
                Stop((int)(70.0 / scaledChars.Count) * i);

                var distanceMap = DistanceTransformation();
                Stop((int)(70.0 / scaledChars.Count) * i);

                if (Config.CreateTemplate)
                {
                    if (!createTemplatesLater)
                    {
                        SafeTamplateBitmapToDisk(_currentTemplate);
                    }
                    else
                    {
                        transformedTemplates.Add((Bitmap)_currentTemplate.Clone());
                    }
                }
                else
                {
                    string result = Matching(distanceMap);
                    ResultText += result;
                    Stop((int)(70.0 / scaledChars.Count) * i);
                }
                i++;
            }

            if (createTemplatesLater)
            {
                SafeTamplateListToDisk(transformedTemplates);
            }
        }

        private List<Bitmap> Scale(List<Bitmap> chars)
        {
            List<Bitmap> scaledChars = ScaleWrapper.scaleImages(chars);
            return scaledChars;
        }

        private List<Bitmap> Boxing()
        {
            CurrentStep = "Boxing";
            CurrentImage = CharacterIsolationWrapper.VisualizeBoxing(_currentWorkingImage);
            List<Bitmap> chars = CharacterIsolationWrapper.IsolateCharacters(_currentWorkingImage);
            return chars;
        }

        private List<Bitmap> Thin(List<Bitmap> images)
        {
            CurrentStep = "Thining";
            CurrentImage = ThinningWrapper.Thin(CurrentImage);
            List<Bitmap> thinnedImages = new List<Bitmap>();
            foreach(Bitmap image in images)
            {
                thinnedImages.Add(ThinningWrapper.Thin(image));
            }
            return thinnedImages;
        }

        private Bitmap Thin(Bitmap images)
        {
            CurrentStep = "Thining";
            return ThinningWrapper.Thin(images);            
        }

        private void Binarize()
        {
            CurrentStep = "Binarize";
            CurrentImage = BinarizationWrapper.Binarize(_currentWorkingImage);
        }


        private string Matching(uint[,] distanceMap)
        {
            CurrentStep = "Chamfer Matching";
            var cm = new ChamferMatching(distanceMap, Config);
            cm.Start();
            string result = cm.ResultList[0].Value.ToString();
            return result;
        }

        private uint[,] DistanceTransformation()
        {
            CurrentStep = "Distance Transformation";
            var dtc = new DistanceTransformationChamfer(_currentWorkingImage, Config.ShowDistanceTransformationColored, Config.CreateTemplate);
            var distanceMap = dtc.start();
            CurrentImage = dtc.CurrentImage;
            _currentTemplate = dtc.CurrentTemplate;
            return distanceMap;
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

        private void SafeTamplateListToDisk(List<Bitmap> currentList)
        {
            DispatchIfNecessary(() =>
            {
                HelperFunctions.SafeTemplateListToDisk(currentList);
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
