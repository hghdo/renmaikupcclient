using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RenmeiLib
{
    /// <summary>
    /// Represents a message sent to a User
    /// </summary>
    public class DirectMessage : INotifyPropertyChanged, IEquatable<DirectMessage>
    {
        private double id;

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

        private string text;

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

        private DateTime dateCreated;

        public DateTime DateCreated
        {
            get { return dateCreated; }
            set
            {
                if (value != dateCreated)
                {
                    dateCreated = value;
                    UpdateRelativeTime();
                    OnPropertyChanged("DateCreated");
                }
            }
        }

        private User sender;

        public User Sender
        {
            get { return sender; }
            set
            {
                if (value != sender)
                {
                    sender = value;
                    OnPropertyChanged("Sender");
                }
            }
        }

        private User recipient;

        public User Recipient
        {
            get { return recipient; }
            set
            {
                if (value != recipient)
                {
                    recipient = value;
                    OnPropertyChanged("Recipient");
                }
            }
        }

        private bool isNew;

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

        private string relativeTime;

        /// <summary>
        /// How long ago the Direct Message was added based on DatedCreated and DateTime.Now
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

        /// <summary>
        /// Updates the relativeTime based on the DateCreated and DateTime.Now
        /// </summary>
        public void UpdateRelativeTime()
        {
            DateTime StatusCreatedDate = dateCreated;

            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - StatusCreatedDate.Ticks);
            double delta = ts.TotalSeconds;

            if (delta == 1)
            {
                RelativeTime = "a second ago";
            }
            else if (delta < 60)
            {
                RelativeTime = ts.Seconds + " seconds ago";
            }
            else if (delta < 120)
            {
                RelativeTime = "about a minute ago";
            }
            else if (delta < (45 * 60))
            {
                RelativeTime = ts.Minutes + " minutes ago";
            }
            else if (delta < (90 * 60))
            {
                RelativeTime = "about an hour ago";
            }
            else if (delta < (24 * 60 * 60))
            {
                RelativeTime = "about " + ts.Hours + " hours ago";
            }
            else if (delta < (48 * 60 * 60))
            {
                RelativeTime = "1 day ago";
            }
            else
            {
                RelativeTime = ts.Days + " days ago";
            }
        }

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

        #region IEquatable<DirectMessage> Members

        public bool Equals(DirectMessage other)
        {
            if (other == null)
                return false;
            
            return (Id == other.Id);
        }

        #endregion

        public Tweet ToTweet()
        {
            Tweet tweet = new Tweet();
            tweet.DateCreated = this.DateCreated;
            tweet.User = this.Sender;
            tweet.Text = this.Text;
            tweet.Timeline = Timeline.DirectMessages;
            tweet.Id = this.Id;
            return tweet;
        }
    }

    public class DirectMessageCollection : ObservableCollection<DirectMessage>
    {
        public TweetCollection ToTweetCollection()
        {
            TweetCollection tweets = new TweetCollection();

            foreach (DirectMessage directMessage in this)
            {
                tweets.Add(directMessage.ToTweet());
            }


            return tweets;
        }
    }
}
