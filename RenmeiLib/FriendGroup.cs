using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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



    }

    /// <summary>
    /// Collection of FirendGroups
    /// </summary>
    [Serializable]
    public class FriendGroupCollection : ObservableCollection<FriendGroup>
    {

    }

}
