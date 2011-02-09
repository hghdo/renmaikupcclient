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
using System.ComponentModel;
using Snarl;

namespace pcclient
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : EssentialWindow
    {

        #region Fields and Properties
        private IntPtr SnarlConfighWnd;
        private NativeWindowApplication.WittySnarlMsgWnd snarlComWindow;
        private bool reallyexit = false;
        private bool isLoggedIn = false;

        // How often the automatic tweet updates occur.  TODO: Make this configurable
        private TimeSpan refreshInterval;
        private TimeSpan friendsRefreshInterval = new TimeSpan(0, 45, 0);

        // Main collection of tweets
        private TweetCollection tweets = new TweetCollection();
        private TweetCollection tweetsSentByMe = new TweetCollection();
        private TweetCollection tweetsRefersMe = new TweetCollection();
        private TweetCollection tweetsCommentByMe = new TweetCollection();
        private TweetCollection favTweets = new TweetCollection();

        private TweetCollection[] allTweetsCollection ;

        // For Friend Group
        private FriendGroupCollection group = new FriendGroupCollection();
        private DispatcherTimer friendsRefreshTimer = new DispatcherTimer();
        private DateTime lastFriendsUpdate = DateTime.MinValue;

        public static UserCollection followMeGroup = new UserCollection();
        public static UserCollection myFollowGroup = new UserCollection();

        // Main TwitterNet object used to make Twitter API calls
        private IServiceApi twitter;

        // Delegates for placing jobs onto the thread dispatcher.  
        // Used for making asynchronous calls to Twitter so that the UI does not lock up.
        private delegate void NoArgDelegate();
        private delegate void OneArgDelegate(TweetCollection arg);
        private delegate void OneStringArgDelegate(string arg);
        private delegate void OneArgDelegateFriend(FriendGroupCollection arg);
        private delegate void OneArgDelegateFollow(UserCollection arg);
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

        private ContextMenu tweetContextMenuTemplate;
        private Tweet curItemRelated2ContextMenu;
        #endregion

        #region Constructor

        public MainWindow()
        {

            InitializeComponent();

            this.Title = "人脉库";

            allTweetsCollection = new TweetCollection[] { tweets, tweetsSentByMe, tweetsRefersMe, tweetsCommentByMe, favTweets };

            //ShowLogin();

            SetupNotifyIcon();

            SetDataContextForAllOfTabs();

            //Handle login DebugAutoLogin() used to login auto use hbcjob@126.com/hbcjob and it is for dev only.
            DebugAutoLogin();
            //DisplayLoginIfUserNotLoggedIn();
            SetButtonMenuBarBackground();

            SendTweetBox.Visibility = Visibility.Visible;
            SearchFriendBox.Visibility = Visibility.Collapsed;

        }
        #endregion

        private void SetButtonMenuBarBackground()
        {

            BrushConverter bc = new BrushConverter();
            Brush selectedBrush,commonBrush;
            commonBrush = (Brush)bc.ConvertFrom("#1D9CDF");
            selectedBrush = (Brush)bc.ConvertFrom("#1783B4");
            switch (OuterTab.SelectedIndex)
            {
                case 0:
                    MainMenuTweet.Background = selectedBrush;
                    MainMenuFriend.Background = commonBrush;
                    break;
                case 1:
                    MainMenuTweet.Background = commonBrush;
                    MainMenuFriend.Background = selectedBrush;
                    break;
            }
        }

        private void SetDataContextForAllOfTabs()
        {
            TweetsTab.DataContext = tweets;
            TweetsSendByMeTab.DataContext = tweetsSentByMe;
            TweetsRefersMeTab.DataContext = tweetsRefersMe;
            TweetsCommentedByMeTab.DataContext = tweetsCommentByMe;
            TweetsFavTab.DataContext = favTweets;
            FriendsTreeView.DataContext = group;
            FollowMeTab.DataContext = followMeGroup;
            MyFollowTab.DataContext = myFollowGroup;

            
        }

        #region Get Tweets
        private void DisplayLoginIfUserNotLoggedIn()
        {
            // Does the user need to login?
            //if (string.IsNullOrEmpty(AppSettings.UserName))
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

                //Retrieve tweets
                //DelegateRecentFetch();

            }
        }

        private void ShowLogin()
        {
            LoginLayoutRoot.Visibility = Visibility.Visible;
            //TopFrame.Visibility = Visibility.Collapsed;
            //BottmNavigation.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Collapsed;
        }

        private void HideLogin()
        {
            LoginLayoutRoot.Visibility = Visibility.Collapsed;
            //TopFrame.Visibility = Visibility.Visible;
            //BottmNavigation.Visibility = Visibility.Visible;
            MainFrame.Visibility = Visibility.Visible;
        }
        
        private void GetTweets()
        {
            try
            {
                // Schedule the update functions in the UI thread.
                TweetsTab.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new NoArgDelegate(UpdateUserInterface));

                // TODO Comments two tasks(GetReplies and RetrieveMessages) below.
                // Direct message and replies < 70 hours old will be displayed on the recent tab.
                // Once this somewhat arbitrary (Friday, 5pm - Monday, 9am + 6 hours) threshold is met, 
                // users will still be able to access there direct messages and replies via their
                // respective tabs.  
                //TODO: Make DM and Reply threshold configurable.  Rework this logic once concept of viewed tweets is introduced to Witty.
                string since = DateTime.Now.AddHours(-70).ToString();


        //private TweetCollection tweets = new TweetCollection();
        //private TweetCollection tweetsSentByMe = new TweetCollection();
        //private TweetCollection tweetsRefersMe = new TweetCollection();
        //private TweetCollection tweetsCommentByMe = new TweetCollection();
        //private TweetCollection favTweets = new TweetCollection();


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
                TweetsTab.Dispatcher.BeginInvoke(
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

        private void ShowStatus(string status)
        {
            StatusTextBlock.Text = status;
        }

        private void UpdateUserInterface()
        {
            DateTime lastUpdated = DateTime.Now;
            //StatusTextBlock.Text = "Last Updated: " + lastUpdated.ToLongTimeString();

            AppSettings.LastUpdated = lastUpdated.Ticks.ToString();
            AppSettings.Save();

            //FilterTweets(newTweets, true);
            //HighlightTweets(newTweets);
            //UpdateExistingTweets();

            TweetCollection newTweets;
            //TweetCollection addedTweets = new TweetCollection();
            for (int j = 0; j < allTweetsCollection.Length; j++)
            {
                UpdateExistingTweets(allTweetsCollection[j]);

                if (j == 0)
                {
                    newTweets = twitter.GetFriendsTimeline();
                }
                else if (j == 1)
                {
                    newTweets = twitter.RetriveMySelfTweets();
                }
                else if (j == 2)
                {
                    newTweets = twitter.GetReplies();
                }
                else if (j == 3)
                {
                    newTweets = twitter.RetriveCommentedTweets();
                }
                else if (j == 4)
                {
                    newTweets = twitter.GetFavoriteTweets();
                }
                else
                {
                    newTweets = twitter.GetFriendsTimeline();
                }

                //prevents huge number of notifications appearing on startup
                //bool displayPopups = !(tweets.Count == 0);

                // Add the new tweets
                for (int i = newTweets.Count - 1; i >= 0; i--)
                {
                    Tweet tweet = newTweets[i];

                    if (allTweetsCollection[j].Contains(tweet)) continue;

                    allTweetsCollection[j].Add(tweet);
                    tweet.Index = allTweetsCollection[j].Count;
                    tweet.IsNew = true;
                    //addedTweets.Add(tweet);
                }

            }
            string aaa = "aaa";
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

        #endregion

        #region Get Friends
        private void GetFirends()
        {
            try
            {
                // Schedule the update functions in the UI thread.
                FriendsTreeView.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new OneArgDelegateFriend(UpdateFriendsList), twitter.getFriendGroups());
                FollowMeTab.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new NoArgDelegate(UpdateFollowMeList));
                MyFollowTab.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new OneArgDelegateFollow(UpdateMyFollowList), twitter.getMyFollowFriendsList());


            }
            catch (RateLimitException ex)
            {
                //App.Logger.Debug(String.Format("There was a problem fetching new tweets from Twitter.com: {0}", ex.ToString()));
                FriendsTreeView.Dispatcher.BeginInvoke(
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
        private void UpdateFriendsList(FriendGroupCollection newFriends)
        {
            if (0 != newFriends.Count)
            {
                group.Clear();
                for (int i = newFriends.Count - 1; i >= 0; i--)
                {
                    FriendGroup fg = newFriends[i];
                    group.Add(fg);
                }
            }
        }
        private void UpdateFollowMeList()
        {
            UserCollection newUsers = twitter.getFollowMeFriendsList();
                for (int i = newUsers.Count - 1; i >= 0; i--)
                {
                    if (followMeGroup.Contains(newUsers[i]))
                        continue;
                    User guy = newUsers[i];
                    followMeGroup.Add(guy);
                }
        }
        private void UpdateMyFollowList(UserCollection newUsers)
        {
            if (0 != newUsers.Count)
            {
                
                for (int i = newUsers.Count - 1; i >= 0; i--)
                {
                    if (myFollowGroup.Contains(newUsers[i]))
                        continue;
                    User nu = newUsers[i];
                    myFollowGroup.Add(nu);
                }
            }
        }

        private void DispatchFriendsList()
        {
            NoArgDelegate fetcher = new NoArgDelegate(this.GetFirends);

            fetcher.BeginInvoke(null, null);
        }
 
        private void SetupFriendsListTimer()
        {
            friendsRefreshTimer.Interval = new TimeSpan(0, 0, 5);
            friendsRefreshTimer.IsEnabled = true;
            friendsRefreshTimer.Start();
            friendsRefreshTimer.Tick += new EventHandler(friendsRefreshTimer_Tick);
        }

        void friendsRefreshTimer_Tick(object sender, EventArgs e)
        {
            friendsRefreshTimer.Interval = friendsRefreshInterval;
            DispatchFriendsList();
        }
        #endregion

        #region Login
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.IsEnabled = false;
            UserNameBox.IsEnabled = false;

            //HttpRequest htr = new HttpRequest(MainWindows.website);
            twitter = new RenmeiNet(UserNameBox.Text,RenmeiNet.ToSecureString(PasswordTextBox.Password));
            twitter.TwitterServerUrl = AppSettings.RenmeiHost;

            // 等待效果
            // 加一个等待循环的图片，然后现在enable可见

            
            // 用线程实现的效果比较好
            //# TryLogin(twitter);
            LoginButton.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new LoginDelegate(TryLogin), twitter);

        }

        private void DebugAutoLogin()
        {

            //twitter = new RenmeiNet("hbcjob@126.com", RenmeiNet.ToSecureString("hbcjob"));
            twitter = new RenmeiNet("binzhi_web@126.com", RenmeiNet.ToSecureString("111111"));
            //twitter = new RenmeiNet("renmaikuadmin@126.com", RenmeiNet.ToSecureString("woaini737727"));
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
                this.Title += "-" + twitter.CurrentlyLoggedInUser.ScreenName;
                // Display current login user Head Image
                BitmapImage myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(App.LoggedInUser.ImageUrl);
                myBitmapImage.EndInit();
                CurrentUserHeadImage.Source = myBitmapImage;
                DelegateRecentFetch();
                DispatchFriendsList();
            }
            else
            {
                MessageBox.Show("Incorrect username or password. Please try again");
            }
        }

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

        #region notification

        void CheckTrayIcon()
        {
            ShowTrayIcon(IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (_notifyIcon != null)
                _notifyIcon.Visible = show;
        }

        void openMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = _storedWindowState;
        }

        void exitMenuItem_Click(object sender, EventArgs e)
        {
            this.reallyexit = true;
            this.Close();
        }
        private void SetupNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.BalloonTipText = "右键打开菜单";
            _notifyIcon.BalloonTipTitle = "人脉库客户端";
            _notifyIcon.Text = "人脉库客户端";
            _notifyIcon.Icon = pcclient.Properties.Resources.user_coat_green_01;
            _notifyIcon.DoubleClick += new EventHandler(m_notifyIcon_Click);
            _notifyIcon.Visible = true;

            System.Windows.Forms.ContextMenu notifyMenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem openMenuItem = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem exitMenuItem = new System.Windows.Forms.MenuItem();

            notifyMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { openMenuItem, exitMenuItem });
            openMenuItem.Index = 0;
            openMenuItem.Text = "打开人脉库客户端";
            openMenuItem.Click += new EventHandler(openMenuItem_Click);
            exitMenuItem.Index = 1;
            exitMenuItem.Text = "退出";
            exitMenuItem.Click += new EventHandler(exitMenuItem_Click);

            _notifyIcon.ContextMenu = notifyMenu;

            this.Closed += new EventHandler(OnClosed);
            this.StateChanged += new EventHandler(OnStateChanged);
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(OnIsVisibleChanged);
            //this.Loaded += new RoutedEventHandler(OnLoaded);
            OverrideClosing();
        }
        private void OverrideClosing()
        {
            this.Closing += new CancelEventHandler(MainWindow_Closing);
        }
        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // If the user selected to minimize on close and the window state is normal
            // just minimize the app
            if (AppSettings.MinimizeOnClose && this.reallyexit == false)
            {
                e.Cancel = true;
                _storedWindowState = this.WindowState;
                this.WindowState = WindowState.Minimized;
                this.Visibility = Visibility.Hidden;
                
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = true;
                    _notifyIcon.ShowBalloonTip(2000);
                }
            }
        }

        #endregion

        #region Minimize to Tray

        private System.Windows.Forms.NotifyIcon _notifyIcon;

        void OnClosed(object sender, EventArgs e)
        {
            if (!AppSettings.PersistLogin)
            {
                AppSettings.UserName = string.Empty;
                AppSettings.Password = string.Empty;
                AppSettings.Save();
            }

            _notifyIcon.Dispose();
            _notifyIcon = null;

            if (SnarlConnector.GetSnarlWindow().ToInt32() != 0 && this.SnarlConfighWnd != null)
            {
                SnarlConnector.RevokeConfig(this.SnarlConfighWnd);
            }
            if (this.SnarlConfighWnd != null && snarlComWindow != null)
            {
                snarlComWindow.DestroyHandle();
            }
        }

        private WindowState _storedWindowState = WindowState.Normal;

        DispatcherTimer hideTimer = new DispatcherTimer();

        void OnStateChanged(object sender, EventArgs args)
        {
            if (AppSettings.MinimizeToTray)
            {
                if (WindowState == WindowState.Minimized)
                {
                    hideTimer.Interval = new TimeSpan(500);
                    hideTimer.Tick += new EventHandler(HideTimer_Elapsed);
                    hideTimer.Start();
                }
                else
                {
                    _storedWindowState = WindowState;
                }
            }
        }

        private void HideTimer_Elapsed(object sender, EventArgs e)
        {
            this.Hide();
            hideTimer.Stop();
        }

        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = _storedWindowState;
        }

        #endregion

        #region Single Instance
//        /// <summary>
//        /// Enforce single instance for release mode
//        /// </summary>
//        private void SetupSingleInstance()
//        {
//#if !DEBUG
//            Application.Current.Exit += new ExitEventHandler(Current_Exit);
//            _instanceManager = new SingleInstanceManager(this, ShowApplication);
//#endif
//        }

//        SingleInstanceManager _instanceManager;

//        public void ShowApplication()
//        {
//            if (this.Visibility == Visibility.Hidden)
//            {
//                this.Visibility = Visibility.Visible;
//            }
//        }

//        void Current_Exit(object sender, ExitEventArgs e)
//        {
//            Environment.Exit(0);
//        }

        #endregion

        #region Bottom Menu
        private void SystemMenu_Initialized(object sender, EventArgs e)
        {
            SystemLogo.ContextMenu = null;
        }

        private void SystemLogo_Click(object sender, RoutedEventArgs e)
        {
            SystemLogoMenu.PlacementTarget = SystemLogo;
            SystemLogoMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            SystemLogoMenu.IsOpen = true;
        }


        private void OpenFriendsPad(object sender, MouseButtonEventArgs e)
        {
            OuterTab.SelectedIndex = 1;
            //TweetsPad.Visibility = Visibility.Collapsed;
            //FriendsPad.Visibility = Visibility.Visible;
            //DispatchFriendsList();
        }

        private void OpenTweetsPad(object sender, MouseButtonEventArgs e)
        {
            OuterTab.SelectedIndex = 0;
            //TweetsPad.Visibility = Visibility.Visible;
            //FriendsPad.Visibility = Visibility.Collapsed;
            //DelegateRecentFetch();
        }


        #endregion

        #region Refresh Button click

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            DelegateRecentFetch();
            DispatchFriendsList();
        }
        #endregion

        #region Unknown
        private void ExtendTweetsInputBox(Boolean extend)
        {
            if (extend)
            {
                NewTweetBox.Height = 85;
                NewTweetButtons.Visibility = Visibility.Visible;
            }
            else
            {
                NewTweetBox.Clear();
                NewTweetBox.Height = 35;
                NewTweetButtons.Visibility = Visibility.Collapsed;
            }
        }
        
        private void NewTweetBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ExtendTweetsInputBox(true);
        }

        private void NewTweetBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //NewTweetBox.Height = 35;
        }

        private void SendTweet_Click(object sender, RoutedEventArgs e)
        {
            if (NewTweetBox.Text.Length > 0)
            {
                try
                {
                    twitter.AddTweet(NewTweetBox.Text);
                    //TweetCollection tweets = new TweetCollection();
                    //tweets.Add(twitter.AddTweet(NewTweetBox.Text));
                    LayoutRoot.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new NoArgDelegate(UpdateUserInterface));
                    //twitter.AddTweet(NewTweetBox.Text);
                    //UpdateUserInterface(tweets);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
                ExtendTweetsInputBox(false);
            }
            else
            {
                MessageBox.Show("至少写点东西吧！");
            }
        }

        private void CancelTweet_Click(object sender, RoutedEventArgs e)
        {
            ExtendTweetsInputBox(false);
        }

        private void OuterTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetButtonMenuBarBackground();
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


        #endregion

        #region Friend Group right click
        private void taHomePage_onClick(object sender, RoutedEventArgs e)
        {
            MenuItem curMenu = sender as MenuItem;
            ContextMenu curContext = curMenu.Parent as ContextMenu;

            Point p = curContext.TranslatePoint(new Point(0, 0), FriendsTreeView);

            // 取父节点的父节点
            DependencyObject obj = FriendsTreeView.InputHitTest(p) as DependencyObject;
            obj = VisualTreeHelper.GetParent(obj);
            obj = VisualTreeHelper.GetParent(obj);

            User curUser = ((ContentPresenter)obj).Content as User;

            //System.Diagnostics.Process.Start(AppSettings.RenmeiHost); 
            System.Diagnostics.Process.Start(curUser.ImageUrl);
            
        }

        private void sendTweet_onClick(object sender, RoutedEventArgs e)
        {

        }

        private void sendPrivateMsg_onClick(object sender, RoutedEventArgs e)
        {

        }

        private void viewTwitter_onClick(object sender, RoutedEventArgs e)
        {
            MenuItem curMenu = sender as MenuItem;
            ContextMenu curContext = curMenu.Parent as ContextMenu;

            Point p = curContext.TranslatePoint(new Point(0, 0), FriendsTreeView);

            // 取父节点的父节点
            DependencyObject obj = FriendsTreeView.InputHitTest(p) as DependencyObject;
            obj = VisualTreeHelper.GetParent(obj);
            obj = VisualTreeHelper.GetParent(obj);

            User curUser = ((ContentPresenter)obj).Content as User;

            SingleOneAllTweets soa = new SingleOneAllTweets(twitter, curUser);
            soa.Show();
        }

        private void removeFriend_onClick(object sender, RoutedEventArgs e)
        {

        }

        private void shildTweets_onClick(object sender, RoutedEventArgs e)
        {

        }

        private ListBox getTweetListBox()
        {
            switch (TweetsPad.SelectedIndex)
            {
                case 0:
                    return AllTweetsListBox;
                case 1:
                    return MyTweetsListBox;
                case 2:
                    return SentToMeTweetsListBox;
                case 3:
                    return CommentsTweetsListBox;
                case 4:
                    return FavTweetsListBox;
                default:
                    return AllTweetsListBox;
            }
        }
        
        private void CommentTweet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Tweet curItem = ((ListBoxItem)getTweetListBox().ContainerFromElement((Image)sender)).Content as Tweet;
            CommentsWindow cw = new CommentsWindow(twitter, curItem);
            //cw.tweet = curItem;
            //cw.twitterApi = twitter;
            cw.ShowDialog();
        }

        private void FavTweet_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Tweet curItem = ((ListBoxItem)getTweetListBox().ContainerFromElement((Image)sender)).Content as Tweet;

            twitter.AddFavTweet(curItem.Id);
            //TabItem ti = TweetsPad.SelectedItem as TabItem;
            //TweetCollection cl=ti.DataContext as TweetCollection;
            //Tweet tw = AllTweetsListBox.SelectedItem as Tweet;
            //MessageBox.Show("This is " + tw.Text + " tab");
        }
        private void DeleteTweet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Tweet curItem = ((ListBoxItem)getTweetListBox().ContainerFromElement((Image)sender)).Content as Tweet;
                twitter.DestroyTweet(curItem.Id);
                for (int j = 0; j < allTweetsCollection.Length; j++) allTweetsCollection[j].Remove(curItem);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void ReplyTweet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Tweet curItem = ((ListBoxItem)getTweetListBox().ContainerFromElement((Image)sender)).Content as Tweet;
            ExtendTweetsInputBox(true);
            NewTweetBox.Text = "RT @" + curItem.User.ScreenName +" "+ curItem.Text;
        }

        private void AddFollowAction_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("服务器端异常");
            //User curUser = ((ListBoxItem)FollowMeListBox.ContainerFromElement((Image)sender)).Content as User;
            //twitter.ChangeFollowStatus( curUser.Id, "add" );
            //myFollowGroup.Add(curUser);
        }

        private void ShowSearchTextBox(object sender, RoutedEventArgs e)
        {
            SendTweetBox.Visibility = Visibility.Collapsed;
            SearchFriendBox.Visibility = Visibility.Visible;
        }

        private void HideSearchTextBox(object sender, RoutedEventArgs e)
        {
            SendTweetBox.Visibility = Visibility.Visible;
            SearchFriendBox.Visibility = Visibility.Collapsed;
        }

        private void SearchTextBox_Clear(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string username = SearchTextBox.Text;
            //    if (group.Count != 0 && username.Length !=0 )
            //    {
            //        foreach( TreeViewItem it in FriendsTreeView.Items)
            //           if( it.Header.ToString() == username)
            //           {
            //               it.IsSelected =true;
            //               break;
            //           }
            //    }
        }

        #endregion

        private void Menu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                curItemRelated2ContextMenu = ((ListBoxItem)getTweetListBox().ContainerFromElement((Image)sender)).Content as Tweet;
                if (curItemRelated2ContextMenu == null) return;
                Image image = sender as Image;
                ContextMenu cm=PrepareTweetContextMenuTemplate();
                cm.PlacementTarget = image;
                cm.IsOpen = true;

            }            
        }

        private ContextMenu PrepareTweetContextMenuTemplate()
        {
            //if (tweetContextMenuTemplate == null)
            //{
                tweetContextMenuTemplate = new ContextMenu();
                bool ttt = myFollowGroup.Contains(curItemRelated2ContextMenu.User);
                tweetContextMenuTemplate.Items.Add(creatTweetMenuItem("TA主页", "homepage"));
                //if (curItemRelated2ContextMenu.User.Id != App.LoggedInUser.Id && !myFollowGroup.Contains(curItemRelated2ContextMenu.User))
                //    tweetContextMenuTemplate.Items.Add(creatTweetMenuItem("关注", "follow"));
                //if (curItemRelated2ContextMenu.User.Id != App.LoggedInUser.Id && myFollowGroup.Contains(curItemRelated2ContextMenu.User))
                //    tweetContextMenuTemplate.Items.Add(creatTweetMenuItem("取消关注", "unfollow"));
                if (curItemRelated2ContextMenu.User.Id != App.LoggedInUser.Id )
                    tweetContextMenuTemplate.Items.Add(creatTweetMenuItem("发微博", "sendTweet"));
                if (curItemRelated2ContextMenu.User.Id != App.LoggedInUser.Id)
                    tweetContextMenuTemplate.Items.Add(creatTweetMenuItem("发私信", "sendMsg"));
                if (curItemRelated2ContextMenu.User.Id != App.LoggedInUser.Id)
                    tweetContextMenuTemplate.Items.Add(creatTweetMenuItem("查看微博", "viewTweet"));
                //if (curItemRelated2ContextMenu.User.Id != App.LoggedInUser.Id)
                //    tweetContextMenuTemplate.Items.Add(creatTweetMenuItem("屏蔽微博", "block"));
            //}
            return tweetContextMenuTemplate;
        }

        private MenuItem creatTweetMenuItem(string header, string name)
        {
            MenuItem mi = new MenuItem();
            mi.Name = name;
            mi.Header = header;
            mi.Click += new RoutedEventHandler(mi_Click);
            return mi;
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            //public static UserCollection followMeGroup = new UserCollection();
            //public static UserCollection myFollowGroup = new UserCollection();
            MenuItem m = sender as MenuItem;
            try
            {
                if (m.Name.Equals("viewTweet"))
                {
                    SingleOneAllTweets soa = new SingleOneAllTweets(twitter, curItemRelated2ContextMenu.User);
                    soa.Show();
                }
                else if (m.Name.Equals("follow"))
                {
                    twitter.ChangeFollowStatus(curItemRelated2ContextMenu.User.Id, "add");
                    myFollowGroup.Add(curItemRelated2ContextMenu.User);
                }
                else if (m.Name.Equals("unfollow"))
                {
                    twitter.ChangeFollowStatus(curItemRelated2ContextMenu.User.Id, "delete");
                    myFollowGroup.Remove(curItemRelated2ContextMenu.User);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

    }

}
