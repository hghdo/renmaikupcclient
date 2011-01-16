using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Windows.Controls;
using System.Windows.Forms;


////////////////////////////////////////////////
//
// Author:  Keith Elder
// Contact Information:  http://keithelder.net/About.aspx
// Last Updated:  March 29th, 2008
//
////////////////////////////////////////////////
namespace pcclient.ClickOnce
{
    /// <summary>
    /// Uses ClickOnce to update the application when a new version
    /// is available.  Currently this is called from MainWindow based on an automatic
    /// timer.  See Settings.Settings for interval setting.  The interval is in
    /// the number of seconds.
    /// 
    /// Also used in the Options form where user's can manually click to update
    /// the application.
    /// 
    /// The only requirement needed is to pass into the constructor the TextBlock control
    /// that is going to display the updates.  There are two events that fire when an update is started:
    /// UpdatedStarted and UpdateCompleted.  Use these to turn timers on or off so
    /// updates don't overlap if the application is running and the user is away 
    /// from the computer for a period of time.
    /// </summary>
    class Deployment
    {
        #region Fields
        /// <summary>
        /// Shows how big the clickonce update is.
        /// </summary>
        private long _sizeOfUpdate;

        /// <summary>
        /// Used to turn on or off the update message.
        /// </summary>
        private bool _showUpdateMessage;

        /// <summary>
        /// This is the status label that receives update information. 
        /// </summary>
        private TextBlock _downloadStatusLabel;


        private bool _restartApplication;
        /// <summary>
        /// If there is an update and Witty needs to be restarted this will be true.
        /// </summary>
        public bool RestartApplication
        {
            get { return _restartApplication; }
            set
            {
                _restartApplication = value;
            }
        }
#endregion

        #region Events
        public delegate void UpdateCompletedDelegate(bool restartApplication);
        public event UpdateCompletedDelegate UpdateCompletedEvent;
        public delegate void UpdateStartedDelegate();
        public event UpdateStartedDelegate UpdateStartedEvent;
        #endregion

        #region Constructor
        /// <summary>
        /// ClickOnce enable any WPF form.
        /// </summary>
        /// <param name="downLoadStatusLabel">TextBlock control used to display download and update information.</param>
        /// <exception cref="NullReferenceException">
        ///  Throws NullReferenceException if TextBlock is null.
        /// </exception>
        public Deployment(TextBlock downLoadStatusLabel)
        {
            if (downLoadStatusLabel == null)
            {
                throw new ArgumentNullException("The TextBlock control used to display status updates is null.");
            }
            else
            {
                _downloadStatusLabel = downLoadStatusLabel;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Updates the application if the application is network deployed.  All updates 
        /// run asynchronously.  The completed event tells you whether or not the application
        /// should be restarted.
        /// </summary>
        public void UpdateApplication()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                ad.CheckForUpdateCompleted += new CheckForUpdateCompletedEventHandler(ad_CheckForUpdateCompleted);
                ad.CheckForUpdateProgressChanged += new DeploymentProgressChangedEventHandler(ad_CheckForUpdateProgressChanged);
                if (UpdateStartedEvent != null)
                {
                    UpdateStartedEvent();
                }
                ad.CheckForUpdateAsync();
            }
        }
        #endregion

        #region Events and misc methods
        private void ShowMessage(string msg)
        {
            MessageBox.Show(msg, 
                "Witty Automatic Update", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Exclamation, 
                MessageBoxDefaultButton.Button1, 
                MessageBoxOptions.RtlReading);
        }
        void ad_CheckForUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            _downloadStatusLabel.Text = String.Format("Checking for new version.  Downloading {0:D}K of {1:D}K.", e.BytesCompleted / 1024, e.BytesTotal / 1024);
        }

        void ad_CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
            ad.CheckForUpdateCompleted -= new CheckForUpdateCompletedEventHandler(ad_CheckForUpdateCompleted);
            if (e.Error != null)
            {
                UpdateCompleted();
                return;
            }
            else if (e.Cancelled == true)
            {
                ShowMessage("The update was cancelled.");
                UpdateCompleted();
                return;
            }

            // Ask the user if they would like to update the application now.
            if (e.UpdateAvailable)
            {
                _sizeOfUpdate = e.UpdateSizeBytes / 1024;

                if (!e.IsUpdateRequired)
                {
                    DialogResult dr = MessageBox.Show("A new version of Witty Twitter is available!\nWould you like to update to the new version?\n\nDownload Size: " + _sizeOfUpdate + "K", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (DialogResult.Yes == dr)
                    {
                        BeginUpdate();
                    }
                    else
                    {
                        // user may not update the application
                        // for whatever reason
                        UpdateCompleted();
                    }
                }
                else
                {
                    ShowMessage("A mandatory update is available for your Witty Twitter.\n\nAfter the update is completed the application will restart.");
                    BeginUpdate();
                }
            }

            if (_showUpdateMessage && e.UpdateAvailable == false)
            {
                ShowMessage("No update found.");
                _showUpdateMessage = false; // don't show it again unless user clicks on the check update.
            }
            else if (e.UpdateAvailable == false)
            {
                _downloadStatusLabel.Text = "No update found.  You have the latest version.";
            }

            UpdateCompleted();
        }

        /// <summary>
        /// Starts the asynchronous update of the application.
        /// </summary>
        private void BeginUpdate()
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
            ad.UpdateCompleted += new AsyncCompletedEventHandler(ad_UpdateCompleted);
            // Indicate progress in the application's status bar.
            ad.UpdateProgressChanged += new DeploymentProgressChangedEventHandler(ad_UpdateProgressChanged);
            ad.UpdateAsync();
        }

        void ad_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            String progressText = String.Format("{0:D}K out of {1:D}K downloaded - {2:D}% complete", e.BytesCompleted / 1024, e.BytesTotal / 1024, e.ProgressPercentage);
            _downloadStatusLabel.Text = progressText;
        }

        /// <summary>
        /// Used to call back to any events listening when 
        /// an update is completed.
        /// </summary>
        private void UpdateCompleted()
        {
            if (UpdateCompletedEvent != null)
            {
                UpdateCompletedEvent(_restartApplication);
            }
        }

        void ad_UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
            ad.UpdateCompleted -= new AsyncCompletedEventHandler(ad_UpdateCompleted);
            if (e.Cancelled)
            {
                ShowMessage("The update of the application's latest version was cancelled.");
                UpdateCompleted();
                return;
            }
            else if (e.Error != null)
            {
                ShowMessage("ERROR: Could not install the latest version of Witty Twitter. Reason: \n" + e.Error.Message + "\nPlease report this error to the Witty developers.");
                return;
            }

            DialogResult dr = MessageBox.Show("The application has been updated.\nThe application must be restarted in order for changes to take effect.\n\nWould you like to restart the application now?", "Restart Application", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (DialogResult.OK == dr)
            {
                _restartApplication = true;
            }

            UpdateCompleted();
        }
    }
    #endregion
}
