using System;
using System.Collections.Generic;
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


    }

    /// <summary>
    /// Collection of Tweets
    /// </summary>
    [Serializable]
    public class TweetCollection : ObservableCollection<FriendGroup>
    {

    }

}
