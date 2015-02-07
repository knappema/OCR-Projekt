using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ORC_Projekt.Commands;
using System.IO;
using System.Windows.Forms;
using ORC_Projekt.BL;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Interop;
using System.Drawing;
using ORC_Projekt.View;

namespace ORC_Projekt.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ICommand _selectFileCommand;
        private readonly ICommand _startOcrCommand;
        private readonly ICommand _resumeOcrCommand;
        private readonly ICommand _cancelOcrCommand;
        private readonly ConfigModel _config;
        private OcrManager _ocrManager;
        private OcrBackgroundworker _ocrWorker;

        public MainWindowViewModel()
        {
            _selectFileCommand = new RelayCommand(ExecuteSelectFile, CanExecuteSelectFile);
            _startOcrCommand = new RelayCommand(ExecuteStartOcr, CanExecuteStartOcr);
            _resumeOcrCommand = new RelayCommand(ExecuteResumeOcr, CanExecuteResumeOcr);
            _cancelOcrCommand = new RelayCommand(ExecuteCancelOcr);
            _config = new ConfigModel();
        }

        #region Properties

        public ImageSource OriginalImage
        {
            get
            {
                if (_ocrManager != null)
                {
                    Bitmap bitmap = _ocrManager.OriginalImage;

                    BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), 
                        IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
                    return source;
                }
                return null;
            }
        }

        public ImageSource CurrentImage
        {
            get
            {
                if (_ocrManager != null)
                {
                    Bitmap bitmap = _ocrManager.CurrentImage;
                    if (bitmap != null)
                    {
                        BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                            IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
                        return source;
                    }
                }
                return null;
            }
        }

        public String ResultText
        {
            get
            {
                if (_ocrManager != null)
                {
                    return _ocrManager.ResultText;
                }
                return String.Empty;
            }
        }

        public String CurrentStep
        {
            get
            {
                if (_ocrManager != null)
                {
                    return _ocrManager.CurrentStep;
                }
                return String.Empty;
            }
        }
        
        public bool IsColoredDistanceTransformationSelected
        {
            get 
            {
                return _config.ShowDistanceTransformationColored;
            }
            set
            {
                if (_config.ShowDistanceTransformationColored != value)
                {
                    _config.ShowDistanceTransformationColored = value;
                    OnPropertyChanged("IsColoredDistanceTransformationSelected");

                    if(_ocrManager != null)
                    {
                        _ocrManager.Config = _config;
                    }
                }
            }
        }

        public bool IsOcrStarted
        {
            get
            {
                if (_ocrWorker != null)
                {
                    return _ocrWorker.IsOcrStarted;
                }
                return false;
            }
        }

        public bool IsOcrPaused
        {
            get
            {
                if (_ocrWorker != null)
                {
                    return _ocrWorker.IsOcrPaused;
                }
                return false;
            }
        }

        public int ProgressValue
        {
            get
            {
                if (_ocrWorker != null)
                {
                    return _ocrWorker.ProgressValue;
                }
                return 0;
            }
            set
            {
                if (_ocrWorker != null)
                {
                    if (_ocrWorker.ProgressValue != value)
                    {
                        _ocrWorker.ProgressValue = value;
                        OnPropertyChanged("ProgressValue");
                    }
                }
            }
        }

        public bool ShowSteps
        {
            get
            {
                return _config.ShowSteps;
            }
            set
            {
                if (_config.ShowSteps != value)
                {
                    _config.ShowSteps = value;
                    OnPropertyChanged("ShowSteps");

                    if (_ocrManager != null)
                    {
                        _ocrManager.Config = _config;
                    }
                }
            }

        }

        public bool CreateTemplate
        {
            get
            {
                return _config.CreateTemplate;
            }
            set
            {
                if (_config.CreateTemplate != value)
                {
                    _config.CreateTemplate = value;
                    OnPropertyChanged("CreateTemplate");

                    if (_ocrManager != null)
                    {
                        _ocrManager.Config = _config;
                    }
                }
            }

        }

        #endregion


        #region Commands

        public ICommand SelectFileCommand
        {
            get
            {
                return _selectFileCommand;
            }
        }

        public ICommand StartOcrCommand
        {
            get
            {
                return _startOcrCommand;
            }
        }

        public ICommand ResumeOcrCommand
        {
            get
            {
                return _resumeOcrCommand;
            }
        }

        public ICommand CancelOcrCommand
        {
            get
            {
                return _cancelOcrCommand;
            }
        }

        #endregion

        #region CommandHelper

        private bool CanExecuteSelectFile(object o)
        {
            return !IsOcrStarted;
        }

        private void ExecuteSelectFile(object o)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"; ;
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fileName = openFileDialog1.FileName;
                    if (fileName != null)
                    {
                        createOcrManager(fileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private bool CanExecuteStartOcr(object o)
        {
            return _ocrManager != null;
        }

        private void ExecuteStartOcr(object o)
        {
            _ocrWorker.Start();
        }

        private void ExecuteResumeOcr(object o)
        {
            _ocrWorker.Resume();
        }

        private bool CanExecuteResumeOcr(object o)
        {
            return ShowSteps && IsOcrPaused;
        }

        private void ExecuteCancelOcr(object o)
        {
            _ocrWorker.Cancel();
        }

        #endregion


        #region Privates

        private void createOcrManager(string fileName)
        {
            _ocrManager = null;
            _ocrManager = new OcrManager(fileName, _config);
            _ocrManager.PropertyChanged += new PropertyChangedEventHandler(PropertyHasChanged);
            OnPropertyChanged("OriginalImage");
            OnPropertyChanged("ResultText");

            _ocrWorker = new OcrBackgroundworker(_ocrManager);
            _ocrWorker.PropertyChanged += new PropertyChangedEventHandler(PropertyHasChanged);
        }

        private void PropertyHasChanged(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsOcrPaused")
            {
                ((RelayCommand)_resumeOcrCommand).UpdateCanExecute();
            }

            OnPropertyChanged(e.PropertyName);
        }

        #endregion
    }
}
