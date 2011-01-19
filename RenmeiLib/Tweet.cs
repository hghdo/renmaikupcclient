using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Diagnostics;

namespace RenmeiLib
{
    /// <summary>
    /// Represents the status post for a Twitter User.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("CreatedOn = {DateCreated}, Text = {Text}")]
    public class Tweet : INotifyPropertyChanged, IEquatable<Tweet>, RenmeiLib.IMessage
    {
        #region Private fields

        private double id;
        private DateTime? dateCreated;
        private string relativeTime; 
        private string text;
        private string source;
        private User user;
        private double? inReplyTo = null;
        private bool isNew;
        private bool isInteresting;
        private int index;
        private bool isSearchResult;
        private Timeline timeline = Timeline.Friends;
        private bool isReply = false;
        private bool isDirectMessage;

        #endregion

        #region Public Properties

        /// <summary>
        /// The Tweet id 
        /// </summary>
        public double Id
        {
            get { return id; }
            set
            {
                if (value != id)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        /// <summary>
        /// Date and time the tweet was added
        /// </summary>
        public DateTime? DateCreated
        {
            get { return dateCreated; }
            set
            {
                if (value != dateCreated)
                {
                    dateCreated = value;
                    OnPropertyChanged("DateCreated");
                    UpdateRelativeTime();
                }
            }
        }
        

        /// <summary>
        /// The Tweet text
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                if (value != text)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        /// <summary>
        /// The Tweet source
        /// </summary>
        public string Source
        {
            get { return "通过" + source; }//"from " + source; }
            set
            {
                if (value != source)
                {
                    source = value;
                    OnPropertyChanged("Source");
                }
            }
        }

        /// <summary>
        /// The id of the tweet it is a reply to
        /// </summary>
        public double? InReplyTo
        {
            get { return inReplyTo; }
            set
            {
                if (value != inReplyTo)
                {
                    inReplyTo = value;
                    OnPropertyChanged("InReplyTo");
                }
            }
        }
        /// <summary>
        /// Twitter User associated with the Tweet
        /// </summary>
        public User User
        {
            get { return user; }
            set
            {
                if (value != user)
                {
                    user = value;
                    OnPropertyChanged("User");
                }
            }
        }

        /// <summary>
        /// How long ago the tweet was added based on DatedCreated and DateTime.Now
        /// </summary>
        public string RelativeTime
        {
            get
            {
                return relativeTime;
            }
            set
            {
                relativeTime = value;
                OnPropertyChanged("Relativetime");
            }
        }

        public bool IsNew
        {
            get { return isNew; }
            set
            {
                if (value != isNew)
                {
                    isNew = value;
                    OnPropertyChanged("IsNew");
                }
            }
        }

        public bool IsInteresting
        {
            get { return isInteresting; }
            set
            {
                if (value != isInteresting)
                {
                    isInteresting = value;
                    OnPropertyChanged("IsInteresting");
                }
            }
        }

        public int Index
        {
            get { return index; }
            set
            {
                if (value != index)
                {
                    index = value;
                    OnPropertyChanged("Index");
                }
            }
        }

        public bool IsSearchResult
        {
            get { return isSearchResult; }
            set
            {
                if (value != isSearchResult)
                {
                    isSearchResult = value;
                    OnPropertyChanged("IsSearchResult");
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// INotifyPropertyChanged requires a property called PropertyChanged.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires the event for the property when it changes.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IEquatable<Tweet> Members

        /// <summary>
        /// Tweets are the same if they have the same Id.
        /// Collection.Contains needs IEquatable implemented to be effective. 
        /// </summary>
        public bool Equals(Tweet other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (id != other.id)
                return false;
            if (Id == -1)  // special type of tweet, so compare the user and text
                return ((user.Id == other.user.Id) && (text == other.text));
            
            return true;
        }

        #endregion

        public override bool Equals(object obj)
        {
            Tweet tweet = obj as Tweet;
            if (tweet != null)
            {
                return Equals(tweet);
            }
            else
            {
                return false;
            }

        }


        /// <summary>
        /// Calculates a friendly display string based on an input timespan
        /// </summary>
        public static string CalculateRelativeTimeString(TimeSpan ts)
        {
            double delta = ts.TotalSeconds;

            if (delta <= 1)
            {
                return "1秒钟前";// "a second ago";
            }
            else if (delta < 60)
            {
                return ts.Seconds + "秒钟前";// " seconds ago";
            }
            else if (delta < 120)
            {
                return "1分钟前";//"about a minute ago";
            }
            else if (delta < (45 * 60))
            {
                return ts.Minutes + "分钟前";// " minutes ago";
            }
            else if (delta < (90 * 60))
            {
                return "1小时前";//"about an hour ago";
            }
            else if (delta < (24 * 60 * 60))
            {
                return ts.Hours + "小时前"; //"about " + ts.Hours + " hours ago";
            }
            else if (delta < (48 * 60 * 60))
            {
                return "1天前";//"1 day ago";
            }
            else
            {
                return ts.Days + "天前";//" days ago";
            }
        }

        /// <summary>
        /// Updates the relativeTime based on the DateCreated and DateTime.Now
        /// </summary>
        public void UpdateRelativeTime()
        {
            if (!dateCreated.HasValue)
                RelativeTime = string.Empty;

            DateTime StatusCreatedDate = (DateTime)dateCreated;

            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - StatusCreatedDate.Ticks);
            RelativeTime = CalculateRelativeTimeString(ts);
        }

        public Timeline Timeline
        {
            get
            {
                return timeline;
            }
            set
            {
                timeline = value;
            }
        }

        public bool IsReply
        {
            get
            {
                return isReply;
            }

            set
            {
                if (value != isReply)
                {
                    isReply = value;
                    OnPropertyChanged("IsReply");
                }
            }
        }

        public bool IsDirectMessage
        {
            get { return isDirectMessage; }
            set
            {
                if (value != isDirectMessage)
                {
                    isDirectMessage = value;
                    OnPropertyChanged("IsDirectMessage");
    }
            }
        }
    }

    public enum SortOrder
    {
        None,
        Ascending,
        Descending
    }
 
    /// <summary>
    /// Collection of Tweets
    /// </summary>
    [Serializable]
    public class TweetCollection : ObservableCollection<Tweet>
    {
        private SortOrder _sortOrder;

        #region Class Constants

        private class Const
        {
            public const string ApplicationFolderName = "Witty";

            public const string SaveFileName = "lastupdated.xaml";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Absolute file path to the last updated file.  The save file is stored in the user's MyDocuments folder under this application folder.
        /// </summary>
        private static string SaveFileAbsolutePath
        {
            get
            {
                string applicationFolderAbsolutePath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Const.ApplicationFolderName);

                // Create the application directory if it doesn't already exist
                if (!Directory.Exists(applicationFolderAbsolutePath))
                    Directory.CreateDirectory(applicationFolderAbsolutePath);

                return Path.Combine(applicationFolderAbsolutePath, Const.SaveFileName);
            }
        }

        #endregion

        #region Methods
        private static readonly object syncLock = new object();

        /// <summary>
        /// Persist the current list of Twitter Tweets to disk.
        /// </summary>
        public void SaveToDisk()
        {
            try
            {
                lock (syncLock)
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    using (XmlWriter writer = XmlWriter.Create(SaveFileAbsolutePath, settings))
                    {
                        XamlWriter.Save(this, writer);
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine(ex.Message);

                // Rethrow so we know about the error
                throw;
            }

        }

        /// <summary>
        /// Load the list of Tweets from disk
        /// </summary>
        public static TweetCollection LoadFromDisk()
        {
            if (!File.Exists(SaveFileAbsolutePath))
                return new TweetCollection();

            lock (syncLock)
            {
                TweetCollection tweets;
                XmlReader xmlReader = XmlReader.Create(SaveFileAbsolutePath);
                tweets = XamlReader.Load(xmlReader) as TweetCollection;
                xmlReader.Close();
                return tweets;
            }
            
            
        }


        protected override void InsertItem(int index, Tweet item)
        {
            switch (_sortOrder)
            {
                case SortOrder.None:
                    base.InsertItem(index, item);
                    return;
                    break;
                
                case SortOrder.Descending:
                    for (int i = 0; i < Count; i++)
                    {
                        if (item.DateCreated > this[i].DateCreated)
                        {
                            base.InsertItem(i, item);
                            return;
                        }
                    }
                    break;
                
                case SortOrder.Ascending:
                    for (int i = 0; i < Count; i++)
                    {
                        if (item.DateCreated < this[i].DateCreated)
                        {
                            base.InsertItem(i, item);
                            return;
                        }
                    }
                    break;
            }

            base.InsertItem(index, item);
        }



        /// <summary>
        /// Removes all tweets above a count.
        /// </summary>
        /// <param name="count"></param>
        public void TruncateAfter(int count)
        {
            if (base.Count > count)
            {
                int max = base.Count - 1;
                for (int i = max; i >= count; i--)
                {
                    base.RemoveItem(i);
                }
            }
        }

        #endregion

        public TweetCollection() : this(SortOrder.Descending)
        {
        }

        public TweetCollection(SortOrder sortOrder)
        {
            _sortOrder = sortOrder;
        }
    }

}
