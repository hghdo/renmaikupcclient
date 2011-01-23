namespace RenmeiLib
{
    public interface IServiceApi
    {
        Tweet AddTweet(string text);
        Tweet AddTweet(string text, double replyid);

        string ClientName { get; set; }
        string CreateFriendshipUrl { get; set; }
        string SendMessageUrl { get; set; }
        string UpdateUrl { get; set; }
        string UserName { get; }
        string UserShowUrl { get; set; }
        string UserTimelineUrl { get; set; }
        string DestroyDirectMessageUrl { get; set; }
        string DestroyUrl { get; set; }
        string DirectMessagesUrl { get; set; }
        string FollowersUrl { get; set; }
        string FriendsTimelineUrl { get; set; }
        string FriendsUrl { get; set; }
        string PublicTimelineUrl { get; set; }
        string RepliesTimelineUrl { get; set; }
        string Format { get; set; }

        void DestroyDirectMessage(double id);
        void DestroyTweet(double id);
        void FollowUser(string userName);
        void SendMessage(string user, string text);


        //UserCollection GetFriends(int userId);
        //UserCollection GetFriends();
        FriendGroupCollection getFriendGroups();
        User CurrentlyLoggedInUser { get; set; }
        User GetUser(int userId);
        User Login();
        Tweet RetrieveTweet(double id);
        TweetCollection GetConversation(double id);
        TweetCollection GetFriendsTimeline();
        TweetCollection GetFriendsTimeline(string since, string userId);
        TweetCollection GetFriendsTimeline(string since);
        TweetCollection GetPublicTimeline(string since);
        TweetCollection GetPublicTimeline();
        TweetCollection GetReplies();
        TweetCollection GetReplies(string since);
        TweetCollection GetUserTimeline(string userId);
        DirectMessageCollection RetrieveMessages();
        DirectMessageCollection RetrieveMessages(string since);

        System.Security.SecureString Password { get; set; }
        System.Net.IWebProxy WebProxy { get; set; }

        string TwitterServerUrl { get; set; }

        void PostPhoto(System.IO.FileInfo fileInfo, string text);
    }
}
