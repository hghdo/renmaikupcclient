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
using System.Windows.Shapes;
using CustomWindow;
using RenmeiLib;

namespace pcclient
{
    /// <summary>
    /// Interaction logic for CommentsWindow.xaml
    /// </summary>
    public partial class CommentsWindow : EssentialWindow
    {

        public IServiceApi twitterApi;

        public Tweet tweet;

        public CommentCollection comments;

        public CommentsWindow()
        {
            InitializeComponent();
            //HeadImage.Source = new BitmapImage(new Uri(tweet.User.ImageUrl)); ;

            TitleBar.DataContext = tweet;
        }

        public CommentsWindow(IServiceApi api, Tweet t)
        {
            twitterApi = api;
            tweet = t;
            InitializeComponent();
            comments = new CommentCollection();
            //HeadImage.Source = new BitmapImage(new Uri(tweet.User.ImageUrl)); ;
            TitleBar.DataContext = tweet;
            TweetText.DataContext = tweet;
            CommentsListBox.DataContext = comments;
            UpdateComments();
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

        private void Comment_Click(object sender, RoutedEventArgs e)
        {
            twitterApi.AddComment(NewCommentBox.Text, tweet);
            UpdateComments();
        }

        private void CancelComment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateComments()
        {
            CommentCollection newComments = new CommentCollection();
            newComments = twitterApi.RetriveComments(tweet);
            for (int i = newComments.Count - 1; i >= 0; i--)
            {
                Comment co = newComments[i];
                if (comments.Contains(co)) continue;
                comments.Add(co);
            }

        }

    }
}
