using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace ORC_Projekt.BL
{
    class OcrBackgroundworker : NotifyPropertyChangedBase
    {
                /// <summary>
        /// The backgroundworker object on which the time consuming operation shall be executed
        /// </summary>
        private BackgroundWorker _worker;
        private OcrManager _manager;

        private int _progressValue = 0;
        private bool _isOcrStarted = false;
        private bool _isOcrPaused = false;

        private ManualResetEvent _waitEvent = new ManualResetEvent(false); 

        public OcrBackgroundworker(OcrManager manager)
        {
            _manager = manager;
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            _worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
        }

        #region Properties

        public int ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                if (_progressValue != value)
                {
                    _progressValue = value;
                    OnPropertyChanged("ProgressValue");
                }
            }
        }

        public bool IsOcrStarted
        {
            get
            {
                return _isOcrStarted;
            }
            set
            {
                if (_isOcrStarted != value)
                {
                    _isOcrStarted = value;
                    OnPropertyChanged("IsOcrStarted");
                }
            }
        }

        public bool IsOcrPaused
        {
            get
            {
                return _isOcrPaused;
            }
            set
            {
                if (_isOcrPaused != value)
                {
                    _isOcrPaused = value;
                    OnPropertyChanged("IsOcrPaused");
                }
            }
        }


        #endregion


        #region Publics

        public void Start()
        {
            //Start the async operation here
            _worker.RunWorkerAsync();
        }

        public void Cancel()
        {
            _waitEvent.Set();
            if (_worker.IsBusy)
            {
                //Stop/Cancel the async operation here
                _worker.CancelAsync();
            }
        }

        public void Resume()
        {
            IsOcrPaused = false;
            _waitEvent.Set();
        }

        #endregion


        #region Privates

        /// <summary>
        /// On completed do the appropriate task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsOcrStarted = false;
            IsOcrPaused = false;
        }

        /// <summary>
        /// Notification is performed here to the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            IsOcrPaused = true;
            //Here you play with the main UI thread
            ProgressValue = e.ProgressPercentage;
            if (ProgressValue < 100)
            {
                IsOcrStarted = true;
            }
        }

        /// <summary>
        /// Time consuming operations go here </br>
        /// i.e. Database operations,Reporting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //NOTE : Never play with the UI thread here...
            _worker.ReportProgress(0);

            try
            {
                _manager.StartAsync(_worker, _waitEvent);
            }
            catch (CancelException)
            {
                //If cancel button was pressed while the execution is in progress
                //Change the state from cancellation ---> cancel'ed
                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    _worker.ReportProgress(0);
                    return;
                }
            }
            catch (Exception)
            {
                //If cancel button was pressed while the execution is in progress
                //Change the state from cancellation ---> cancel'ed
                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    _worker.ReportProgress(0);
                    return;
                }
            }

            
            //Report 100% completion on operation completed
            _worker.ReportProgress(100);
            Thread.Sleep(2000);
        }

        #endregion

    }
}
