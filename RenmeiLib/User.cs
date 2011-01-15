using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RenmeiLib
{
    /// <summary>
    /// A Twitter User
    /// </summary>
    [Serializable]
    public class User : INotifyPropertyChanged, RenmeiLib.IUser
    {
        private int id;
        private string name;
        private string screenName;
        private string imageUrl;
        private string siteUrl;
        private string location;
        private string description;

        // Add for Renmei.com
        private string company;
        private string position;

        public int Id
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

        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string ScreenName
        {
            get { return screenName; }
            set
            {
                if (value != screenName)
                {
                    screenName = value;
                    OnPropertyChanged("ScreenName");
                }
            }
        }

        public string ImageUrl
        {
            get 
            { 
                return imageUrl; 
            }
            set
            {
                if (value != imageUrl)
                {
                    imageUrl = value;
                    OnPropertyChanged("ImageUrl");
                }
            }
        }

        public string SiteUrl
        {
            get { return siteUrl; }
            set
            {
                if (value != siteUrl)
                {
                    siteUrl = value;
                    OnPropertyChanged("SiteUrl");
                }
            }
        }

        public string TwitterUrl
        {
            get
            {
                return "http://twitter.com/" + screenName;
            }
        }

        public string Location
        {
            get { return location; }
            set
            {
                if (value != location)
                {
                    location = value;
                    OnPropertyChanged("location");
                }
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (value != description)
                {
                    description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public string Company
        {
            get { return company; }
            set
            {
                if (value != company)
                {
                    company = value;
                    OnPropertyChanged("Company");
                }
            }
        }

        public string Position
        {
            get { return position; }
            set
            {
                if (value != position)
                {
                    position = value;
                    OnPropertyChanged("Position");
                }
            }
        }

        /// <summary>
        /// The user name along with the screenname. This makes binding a little easier.
        /// </summary>
        public string FullName
        {
            get { return Name + " (" + screenName + ")"; }
        }

        private string backgroundColor;

        public string BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (value != backgroundColor)
                {
                    backgroundColor = value;
                    OnPropertyChanged("BackgroundColor");
                }
            }
        }

        private string textColor;

        public string TextColor
        {
            get { return textColor; }
            set
            {
                if (value != textColor)
                {
                    textColor = value;
                    OnPropertyChanged("TextColor");
                }
            }
        }
        private string linkColor;

        public string LinkColor
        {
            get { return linkColor; }
            set
            {
                if (value != linkColor)
                {
                    linkColor = value;
                    OnPropertyChanged("LinkColor");
                }
            }
        }

        private string sidebarFillColor;

        public string SidebarFillColor
        {
            get { return sidebarFillColor; }
            set
            {
                if (value != sidebarFillColor)
                {
                    sidebarFillColor = value;
                    OnPropertyChanged("SidebarFillColor");
                }
            }
        }

        private string sidebarBorderColor;

        public string SidebarBorderColor
        {
            get { return sidebarBorderColor; }
            set
            {
                if (value != sidebarBorderColor)
                {
                    sidebarBorderColor = value;
                    OnPropertyChanged("SidebarBorderColor");
                }
            }
        }

        private Tweet tweet;

        public Tweet Tweet
        {
            get { return tweet; }
            set
            {
                if (value != tweet)
                {
                    tweet = value;
                    OnPropertyChanged("Tweet");
                }
            }
        }

        private int followingCount;

        public int FollowingCount
        {
            get { return followingCount; }
            set
            {
                if (value != followingCount)
                {
                    followingCount = value;
                    OnPropertyChanged("FollowingCount");
                }
            }
        }

        private int followersCount;

        public int FollowersCount
        {
            get { return followersCount; }
            set
            {
                if (value != followersCount)
                {
                    followersCount = value;
                    OnPropertyChanged("FollowersCount");
                }
            }
        }

        private int statusesCount;

        public int StatusesCount
        {
            get { return statusesCount; }
            set
            {
                if (value != statusesCount)
                {
                    statusesCount = value;
                    OnPropertyChanged("StatusesCount");
                }
            }
        }

        private int favoritesCount;

        public int FavoritesCount
        {
            get { return favoritesCount; }
            set
            {
                if (value != favoritesCount)
                {
                    favoritesCount = value;
                    OnPropertyChanged("FavoritesCount");
                }
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
    }

    [Serializable]
    public class UserCollection : ObservableCollection<User>
    {
    }
}
