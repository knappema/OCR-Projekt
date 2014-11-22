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

namespace ORC_Projekt.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ICommand _selectFileCommand;
        private readonly ICommand _startOcrCommand;
        private OcrManager _ocrManager;

        public MainWindowViewModel()
        {
            _selectFileCommand = new RelayCommand(ExecuteSelectFile, CanExecuteSelectFile);
            _startOcrCommand = new RelayCommand(ExecuteStartOcr, CanExecuteStartOcr);
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

                    BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                        IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
                    return source;
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


        #endregion

        #region CommandHelper

        private bool CanExecuteSelectFile(object o)
        {
            // TODO is ocr running ???
            return true;
        }

        private void ExecuteSelectFile(object o)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
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
            _ocrManager.Start();
        }

        #endregion


        #region Privates

        private void createOcrManager(string fileName)
        {
            _ocrManager = null;
            _ocrManager = new OcrManager(fileName);
            _ocrManager.PropertyChanged += new PropertyChangedEventHandler(PropertyHasChanged);
            OnPropertyChanged("OriginalImage");
        }

        private void PropertyHasChanged(object o, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        #endregion
    }
}
