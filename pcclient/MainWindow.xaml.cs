using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CustomWindow;
using RenmeiLib;
using System.Windows.Threading;
using System.Net;
using System.Windows.Media.Animation;
using log4net.Repository.Hierarchy;
using log4net;

namespace pcclient
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : EssentialWindow
    {

        #region Fields and Properties


        // Main collection of tweets
        private TweetCollection tweets = new TweetCollection();

        // Main TwitterNet object used to make Twitter API calls
        private IServiceApi twitter;

        // Delegates for placing jobs onto the thread dispatcher.  
        // Used for making asynchronous calls to Twitter so that the UI does not lock up.
        private delegate void NoArgDelegate();
        private delegate void OneArgDelegate(TweetCollection arg);
        private delegate void OneStringArgDelegate(string arg);
        private delegate void OneDoubleArgDelegate(double id);
        private delegate void AddTweetsUpdateDelegate(TweetCollection arg);
        private delegate void MessagesDelegate(DirectMessageCollection arg);
        private delegate void SendMessageDelegate(string user, string text);
//        private delegate void LoginDelegate(User arg);
//        private delegate void DeleteTweetDelegate(double id);

        private delegate void LoginDelegate(RenmeiNet arg);
        private delegate void PostLoginDelegate(User arg);

        // Settings used by the application
        private Properties.Settings AppSettings = Properties.Settings.Default;
        

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            LayoutRoot.DataContext = tweets;

            //Handle login DebugAutoLogin() used to login auto use hbcjob@126.com/hbcjob and it is for dev only.
            DebugAutoLogin();
            //DisplayLoginIfUserNotLoggedIn();
            
            //Retrieve tweets
            //DelegateRecentFetch();
            
            
        }
        #endregion

        private void DelegateRecentFetch()
        {
            // Let the user know what's going on
            //StatusTextBlock.Text = "Retrieving tweets...";

            //PlayStoryboard("Fetching");

            // Create a Dispatcher to fetching new tweets
            NoArgDelegate fetcher = new NoArgDelegate(
                this.GetTweets);

            fetcher.BeginInvoke(null, null);

        }

        private void DisplayLoginIfUserNotLoggedIn()
        {
            // Does the user need to login?
            // if (string.IsNullOrEmpty(AppSettings.UserName))
            if (App.LoggedInUser.Id == 0)
            {
                ShowLogin();
            }
            else
            {
                LoginLayoutRoot.Visibility = Visibility.Collapsed;

                System.Security.SecureString password = RenmeiNet.DecryptString(AppSettings.Password);

                // Jason Follas: Reworked Web Proxy - don't need to explicitly pass into TwitterNet ctor
                //twitter = new TwitterNet(AppSettings.Username, password, WebProxyHelper.GetConfiguredWebProxy());
                twitter = new RenmeiNet(AppSettings.UserName, password);

                // Jason Follas: Twitter proxy servers, anyone?  (Network Nazis who block twitter.com annoy me)
                twitter.TwitterServerUrl = AppSettings.RenmeiHost;

                //// Let the user know what's going on
                //StatusTextBlock.Text = Properties.Resources.TryLogin;
                //PlayStoryboard("Fetching");

                //// Create a Dispatcher to attempt login on new thread
                //NoArgDelegate loginFetcher = new NoArgDelegate(this.TryLogin);
                //loginFetcher.BeginInvoke(null, null);

            }
        }

        private void ShowLogin()
        {
            LoginLayoutRoot.Visibility = Visibility.Visible;
            TopFrame.Visibility = Visibility.Hidden;
            TweetsListBox.Visibility = Visibility.Collapsed;
        }

        private void HideLogin()
        {
            LoginLayoutRoot.Visibility = Visibility.Collapsed;
            TopFrame.Visibility = Visibility.Visible;
            TweetsListBox.Visibility = Visibility.Visible;
        }
        
        private void GetTweets()
        {
            try
            {
                // Schedule the update functions in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new OneArgDelegate(UpdateUserInterface), twitter.GetFriendsTimeline());

                // TODO Comments two tasks(GetReplies and RetrieveMessages) below.
                // Direct message and replies < 70 hours old will be displayed on the recent tab.
                // Once this somewhat arbitrary (Friday, 5pm - Monday, 9am + 6 hours) threshold is met, 
                // users will still be able to access there direct messages and replies via their
                // respective tabs.  
                //TODO: Make DM and Reply threshold configurable.  Rework this logic once concept of viewed tweets is introduced to Witty.
                string since = DateTime.Now.AddHours(-70).ToString();

                //LayoutRoot.Dispatcher.BeginInvoke(
                //    DispatcherPriority.Loaded,
                //    new OneArgDelegate(UpdateUserInterface), twitter.GetReplies(since));

                //LayoutRoot.Dispatcher.BeginInvoke(
                //    DispatcherPriority.Loaded,
                //    new OneArgDelegate(UpdateUserInterface), twitter.RetrieveMessages(since).ToTweetCollection());
            }
            catch (RateLimitException ex)
            {
                //App.Logger.Debug(String.Format("There was a problem fetching new tweets from Twitter.com: {0}", ex.ToString()));
                LayoutRoot.Dispatcher.BeginInvoke(
                    DispatcherPriority.ApplicationIdle,
                    new OneStringArgDelegate(ShowStatus), ex.Message);
            }
            catch (WebException ex)
            {
                App.Logger.Debug(String.Format("There was a problem fetching new tweets from Twitter.com: {0}", ex.ToString()));
            }
            catch (ProxyAuthenticationRequiredException ex)
            {
                App.Logger.Error("Incorrect proxy configuration.", ex);
                MessageBox.Show("Proxy server is configured incorrectly.  Please correct the settings on the Options menu.");
            }
            catch (ProxyNotFoundException ex)
            {
                App.Logger.Error("Incorrect proxy configuration.");
                MessageBox.Show(ex.Message);
            }

        }

        private void UpdateUserInterface(TweetCollection newTweets)
        {
            DateTime lastUpdated = DateTime.Now;
            //StatusTextBlock.Text = "Last Updated: " + lastUpdated.ToLongTimeString();

            AppSettings.LastUpdated = lastUpdated.Ticks.ToString();
            AppSettings.Save();

            //FilterTweets(newTweets, true);
            //HighlightTweets(newTweets);
            UpdateExistingTweets();

            TweetCollection addedTweets = new TweetCollection();

            //prevents huge number of notifications appearing on startup
            bool displayPopups = !(tweets.Count == 0);

            // Add the new tweets
            for (int i = newTweets.Count - 1; i >= 0; i--)
            {
                Tweet tweet = newTweets[i];

                if (tweets.Contains(tweet)) continue;

                tweets.Add(tweet);
                tweet.Index = tweets.Count;
                tweet.IsNew = true;
                addedTweets.Add(tweet);
            }
            //delete many lines for demo

            //StopStoryboard("Fetching");
        }

        private void UpdateExistingTweets()
        {
            UpdateExistingTweets(tweets);
        }


        private static void UpdateExistingTweets(TweetCollection oldTweets)
        {
            // Update existing tweets
            foreach (Tweet tweet in oldTweets)
            {
                tweet.IsNew = false;
                tweet.UpdateRelativeTime();
            }
        }

        private void ShowStatus(string status)
        {
            StatusTextBlock.Text = status;
        }

        protected override Decorator GetWindowButtonsPlaceholder()
        {
            return WindowButtonsPlaceholder;
        }

        private void Header_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Width + e.HorizontalChange > 10)
                this.Width += e.HorizontalChange;
            if (this.Height + e.VerticalChange > 10)
                this.Height += e.VerticalChange;
        }


        #region unknown

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            DelegateRecentFetch();
        }
        #endregion

        #region Login
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //HttpRequest htr = new HttpRequest(MainWindows.website);
            twitter = new RenmeiNet(UserNameBox.Text,RenmeiNet.ToSecureString(PasswordTextBox.Password));
            twitter.TwitterServerUrl = AppSettings.RenmeiHost;

            TryLogin(twitter);
            //LoginButton.Dispatcher.BeginInvoke(
            //    System.Windows.Threading.DispatcherPriority.Normal,
            //    new LoginDelegate(TryLogin), rmnet);

        }

        private void DebugAutoLogin()
        {
            twitter = new RenmeiNet("hbcjob@126.com", RenmeiNet.ToSecureString("hbcjob"));
            twitter.TwitterServerUrl = AppSettings.RenmeiHost;
            TryLogin(twitter);
        }

        private void TryLogin(IServiceApi rmnet)
        {
            try
            {
                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new PostLoginDelegate(UpdatePostLoginInterface), rmnet.Login());
            }
            catch (WebException ex)
            {
                App.Logger.Error("There was a problem logging in Renmei.",ex);
                MessageBox.Show("There was a problem logging in to Renmei. " + ex.Message);
            }
        }

        private void UpdatePostLoginInterface(User user)
        {
            App.LoggedInUser = user;
            if (App.LoggedInUser.Id != 0)
            {
                AppSettings.UserName = user.ScreenName;

                //# AppSettings.Password = RenmeiNet.EncryptString(RenmeiNet.ToSecureString(PasswordTextBox.Password));
                AppSettings.Password = PasswordTextBox.Password;
                AppSettings.LastUpdated = string.Empty;

                AppSettings.Save();

                UserNameBox.Text = string.Empty;
                PasswordTextBox.Password = string.Empty;

                RaiseEvent(new RoutedEventArgs(LoginEvent));
                HideLogin();
                DelegateRecentFetch();
            }
            else
            {
                MessageBox.Show("Incorrect username or password. Please try again");
            }
        }

        public static readonly RoutedEvent LoginEvent =
            EventManager.RegisterRoutedEvent("Login", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));

        public event RoutedEventHandler Login
        {
            add { AddHandler(LoginEvent, value); }
            remove { RemoveHandler(LoginEvent, value); }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserNameBox.Focus();
        }
        #endregion


        private void NewTweetBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NewTweetBox.Height = 100;
        }

        private void SendTweet_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelTweet_Click(object sender, RoutedEventArgs e)
        {

        }

    }

}
