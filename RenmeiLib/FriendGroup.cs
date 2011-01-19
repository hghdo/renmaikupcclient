using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace RenmeiLib
{
    /// <summary>
    /// Represents the status post for a Twitter User.
    /// </summary>
    [Serializable]
    public class FriendGroup : INotifyPropertyChanged, IEquatable<FriendGroup>
    {
        #region Private fields

        private double id;

        public double Id
        {
            get { return id; }
            set { id = value; }
        }
        private long userId;

        public long UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private int order;

        public int Order
        {
            get { return order; }
            set { order = value; }
        }
        private int count;

        public int Count
        {
            get { return count; }
            set { count = value; }
        }
        #endregion

        #region IEquatable<FriendGroup> Members

        /// <summary>
        /// Tweets are the same if they have the same Id.
        /// Collection.Contains needs IEquatable implemented to be effective. 
        /// </summary>
        public bool Equals(FriendGroup other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (id != other.id)
                return false;
            if (Id == -1)  // special type of tweet, so compare the user and text
                return ((userId == other.UserId) && (title == other.Title));

            return true;
        }

        #endregion


        public override bool Equals(object obj)
        {
            FriendGroup g = obj as FriendGroup;
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

        #endregion

        #region INotifyPropertyChanged Members

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        #endregion
    }

    /// <summary>
    /// Collection of FirendGroups
    /// </summary>
    [Serializable]
    public class FriendGroupCollection : ObservableCollection<FriendGroup>
    {

    }

}
