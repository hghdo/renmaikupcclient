using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;
using Growl.CoreLibrary;

namespace Growl
{
    /// <summary>
    /// Provides all functionality for communicating with Growl for Windows.
    /// </summary>
    /// <remarks>
    /// This class is implemented as a static class because:
    ///     1. That is how the Snarl class was implemented and I wanted to be consistent; and,
    ///     2. It prevents the MainWindow.xaml.cs or any other part of Witty from having to declare
    ///        or track any new instance variables
    /// </remarks>
    public static class GrowlCommunicator
    {
        /// <summary>
        /// The <see cref="GrowlConnector"/> that actually talks to Growl
        /// </summary>
        private static GrowlConnector growl;

        /// <summary>
        /// The Witty application instance that is registered with Growl
        /// </summary>
        private static Application application;

        /// <summary>
        /// Notification shown for new individual tweets
        /// </summary>
        public static NotificationType NewTweet;

        /// <summary>
        /// Notification shown for a tweet summary (multiple tweets)
        /// </summary>
        public static NotificationType NewTweetsSummary;

        /// <summary>
        /// Notification shown for a new reply/mention
        /// </summary>
        public static NotificationType NewReply;

        /// <summary>
        /// Notification shown for a new direct message
        /// </summary>
        public static NotificationType NewDirectMessage;


        /// <summary>
        /// Checks to see if Growl is currently running on the local machine.
        /// </summary>
        /// <returns>
        /// <c>true</c> if Growl is running;
        /// <c>false</c> otherwise
        /// </returns>
        public static bool IsRunning()
        {
            Initialize();

            return growl.IsGrowlRunning();
        }

        /// <summary>
        /// Registers Witty with the local Growl instance
        /// </summary>
        /// <remarks>
        /// This should usually be called at application start-up
        /// </remarks>
        public static void Register()
        {
            Initialize();

            growl.Register(application, new NotificationType[] { NewTweet, NewTweetsSummary, NewReply, NewDirectMessage });
        }

        /// <summary>
        /// Sends a notification to Growl
        /// </summary>
        /// <param name="notificationType">The <see cref="NotificationType">type</see> of notification to send</param>
        /// <param name="title">The notification title</param>
        /// <param name="text">The notification text</param>
        /// <param name="imageUrl">The notification image as a url</param>
        public static void Notify(NotificationType notificationType, string title, string text, string imageUrl)
        {
            Notification notification = new Notification(application.Name, notificationType.Name, String.Empty, title, text);
            notification.Icon = imageUrl;

            growl.Notify(notification);
        }

        /// <summary>
        /// Initializes the GrowlCommunicator
        /// </summary>
        private static void Initialize()
        {
            if (growl == null)
            {
                growl = new GrowlConnector();
                growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;

                application = new Application("pcclient");
                application.Icon = global::pcclient.Properties.Resources.user_coat_green_01.ToBitmap();

                NewTweet = new NotificationType("New Tweet");
                NewTweetsSummary = new NotificationType("New Tweets Summary");
                NewReply = new NotificationType("New Reply/Mention");
                NewDirectMessage = new NotificationType("New Direct Message");
            }
        }
    }
}
