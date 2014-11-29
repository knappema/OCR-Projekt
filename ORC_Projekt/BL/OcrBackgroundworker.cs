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

        #endregion


        #region Publics

        public void Start()
        {
            //Start the async operation here
            _worker.RunWorkerAsync();
        }

        public void Cancel()
        {
            if (_worker.IsBusy)
            {
                //Stop/Cancel the async operation here
                _worker.CancelAsync();
            }
        }

        public void Resume()
        {
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
            //If it was cancelled midway
            //if (e.Cancelled)
            //{
            //    lblStatus.Text = "Task Cancelled.";
            //}
            //else if (e.Error != null)
            //{
            //    lblStatus.Text = "Error while performing background operation.";
            //}
            //else
            //{
            //    lblStatus.Text = "Task Completed...";
            //}
            //btnStartAsyncOperation.Enabled = true;
            //btnCancel.Enabled = false;
        }

        /// <summary>
        /// Notification is performed here to the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Here you play with the main UI thread
            ProgressValue = e.ProgressPercentage;
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

            //time consuming operation
            //for (int i = 0; i < 100; i++)
            //{
                //Thread.Sleep(100);

                _manager.Start();
                _worker.ReportProgress(50);

                //If cancel button was pressed while the execution is in progress
                //Change the state from cancellation ---> cancel'ed
                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    _worker.ReportProgress(0);
                    return;
                }

            //}
            
            //Report 100% completion on operation completed
            _worker.ReportProgress(100);
        }

        #endregion

    }
}
