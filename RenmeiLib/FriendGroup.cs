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
    /// 
    [Serializable]
    public class FriendGroup : INotifyPropertyChanged
    {
        #region Private fields

        private int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private long groupId;
        public long GroupId
        {
            get { return groupId; }
            set
            {
                if (value != groupId)
                {
                    groupId = value;
                    OnPropertyChanged("GroupId");
                }
            }
        }

        private string title;
        public string Title
        {
            get { return title + "(" + Count.ToString() + ")";  }
            set
            {
                if (value != title)
                {
                    title = value;
                    OnPropertyChanged("Title");
                }
            }
        }
        public String GroupName
        {
            get { return title; }
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

        private UserCollection memberlist;

        public UserCollection MemberList
        {
            get { return memberlist; }
            set
            {
                if (value != memberlist)
                {
                    memberlist = value;
                    OnPropertyChanged("MemberListChange");
                }
            }

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
                return ((GroupId == other.GroupId) && (title == other.Title));

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

        public bool selected = false;
        public bool IsSelected
        {
            get { return selected; }
            set
            {
                if (value != selected)
                {
                    selected = value;
                    OnPropertyChanged("Selected");
                }
            }
        }

        public bool expanded = false;
        public bool IsExpanded
        {
            get { return expanded; }
            set
            {
                if (value != expanded)
                {
                    expanded = value;
                    OnPropertyChanged("Expanded");
                }
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
    public class FriendGroupCollection : ObservableCollection<FriendGroup>
    {

    }

}
