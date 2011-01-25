using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace RenmeiLib
{
    [Serializable]
    public class Comment : INotifyPropertyChanged
    {
        #region Private fields
        private long id;

        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private DateTime publishedTime;

        public DateTime PublishedTime
        {
            get { return publishedTime; }
            set { publishedTime = value; }
        }

        private string clientType;

        public string ClientType
        {
            get { return clientType; }
            set { clientType = value; }
        }

        private User user;

        public User User
        {
            get { return user; }
            set { user = value; }
        }
        #endregion


        #region IEquatable<Comment> Members

        /// <summary>
        /// Tweets are the same if they have the same Id.
        /// Collection.Contains needs IEquatable implemented to be effective. 
        /// </summary>
        public bool Equals(Comment other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (id != other.id)
                return false;
            if (Id == -1)  // special type of tweet, so compare the user and text
                return ((Id == other.Id) && (text == other.Text));

            return true;
        }

        #endregion

        public override bool Equals(object obj)
        {
            Comment g = obj as Comment;
            if (g != null)
            {
                return Equals(g);
            }
            else
            {
                return false;
            }

        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }


    /// <summary>
    /// Collection of FirendGroups
    /// </summary>
    [Serializable]
    public class CommentCollection : ObservableCollection<Comment>
    {

    }

}
