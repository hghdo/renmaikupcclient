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
using RenmeiLib;
using CustomWindow;
using System.Windows.Threading;

namespace pcclient
{
    /// <summary>
    /// Interaction logic for AllCommets.xaml
    /// </summary>
    public partial class SingleOneAllTweets : EssentialWindow
    {
        private TweetCollection singleAllTweets;
        private IServiceApi saNet;
        private User saUser;

        private delegate void NoArgeDelegate();
        private delegate void OneArgDelegate(TweetCollection arg);

        public SingleOneAllTweets(IServiceApi net, User relatedUser)
        {
            InitializeComponent();
            this.saNet = net;
            this.saUser = relatedUser;

            UserTitleBar.DataContext = saUser;
            singleAllTweets = new TweetCollection();
            AllTweetsListBox.DataContext = singleAllTweets;

            DispatchAllTweetsList();

        }

        private void DispatchAllTweetsList()
        {
            NoArgeDelegate fetcher = new NoArgeDelegate(GetTweetsbyUser);
            fetcher.BeginInvoke(null, null);
        }

        public void GetTweetsbyUser()
        {
            try
            {
                AllTweetsListBox.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new NoArgeDelegate(UpdateAllTweetsList));
            }
            catch (Exception ex)
            {
                App.Logger.Debug(String.Format("There was a problem fetching new tweets  {0}", ex.ToString()));

            }
 

        }

        public void UpdateAllTweetsList()
        {
            TweetCollection newTweets = saNet.GetUserTimeline(saUser.Id.ToString());
            if( 0 != newTweets.Count )
            {
                for (int i = newTweets.Count - 1; i >= 0; i--)
                {
                    Tweet tweet = newTweets[i];

                    if (singleAllTweets.Contains(tweet)) continue;

                    singleAllTweets.Add(tweet);
                    tweet.Index = singleAllTweets.Count;
                    tweet.IsNew = true;
                }
            }
        }

        private void ShowAddCommentWin(object sender, RoutedEventArgs e)
        {
            Tweet curItem = ((ListBoxItem)AllTweetsListBox.ContainerFromElement((Image)sender)).Content as Tweet;
            CommentsWindow cw = new CommentsWindow(saNet, curItem);
            //cw.tweet = curItem;
            //cw.twitterApi = twitter;
            cw.ShowDialog();
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

    }
}
