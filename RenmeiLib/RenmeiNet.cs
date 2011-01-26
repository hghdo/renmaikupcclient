using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Text;
using Dimebrain.TweetSharp.Core;
using Dimebrain.TweetSharp.Fluent;
using Dimebrain.TweetSharp.Extensions;
using Dimebrain.TweetSharp.Model;
using System.Collections.Generic;

namespace RenmeiLib
{
    /// <summary>
    /// .NET wrapper for interacting with the Twitter API
    /// </summary>
    public class RenmeiNet : IServiceApi
    {
        #region Private Fields
        private string username;
        private SecureString password;
        private string publicTimelineUrl;
        private string friendsTimelineUrl;
        private string userTimelineUrl;
        private string repliesTimelineUrl;
        private string directMessagesUrl;
        private string updateUrl;
        private string favTweetUrl;
        private string friendsUrl;
        private string followersUrl;
        private string userShowUrl;
        private string sendMessageUrl;
        private string destroyUrl;
        private string destroyDirectMessageUrl;
        private string createFriendshipUrl;
        private string rateLimitStatusUrl;
        private string verifyCredentialsUrl;
        private string tweetUrl;
        private string format;
        private IWebProxy webProxy = HttpWebRequest.DefaultWebProxy;  // Jason Follas: Added initialization
        private string twitterServerUrl;                              // Jason Follas

        private User currentLoggedInUser;

        private string authToken;
        private string email;

        // REMARK: might need to fix this for globalization
        private readonly string twitterCreatedAtDateFormat = "ddd MMM dd HH:mm:ss zzzz yyyy"; // Thu Apr 26 01:36:08 +0000 2007
        private readonly string twitterSinceDateFormat = "ddd MMM dd yyyy HH:mm:ss zzzz";
        private readonly string renmaikuPublishDateFormat = "yyyy-MM-dd HH:mm:ss";

        private static int characterLimit;
        private string clientName;

        #endregion

        #region Public Properties

        public User CurrentlyLoggedInUser
        {
            get
            {
                if (null == currentLoggedInUser)
                {
                    currentLoggedInUser = Login();
                }
                return currentLoggedInUser;
            }
            set
            {
                currentLoggedInUser = value;
            }
        }

        /// <summary>
        /// Twitter username
        /// </summary>
        public string UserName
        {
            get { return (null == currentLoggedInUser ? String.Empty : currentLoggedInUser.Name) ; }
        }
        
        /// <summary>
        /// Twitter password
        /// </summary>
        public SecureString Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        /// Web proxy to use when communicating with the Twitter service
        /// </summary>
        public IWebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }


        /// <summary>
        /// URL of the Twitter host.  Defaults to http://twitter.com/.  Why would
        /// you need to override this?  Think: Alternate endpoints, like other
        /// services with identical API's, or maybe a Twitter Proxy.
        /// </summary>
        public string TwitterServerUrl
        {
            get
            {
                if (String.IsNullOrEmpty(twitterServerUrl))
                    return "http://twitter.com/";
                else
                    return twitterServerUrl;
            }
            set
            {
                twitterServerUrl = value;

                if (!twitterServerUrl.EndsWith("/"))
                    twitterServerUrl += "/";
            }
        }
        
        /// <summary>
        ///  Url to the Twitter Public Timeline. Defaults to http://twitter.com/statuses/public_timeline
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        /// timelineUrl += "?anthCode=" + authToken + "&userId=" + userId;
        public string PublicTimelineUrl
        {
            get
            {
                if (string.IsNullOrEmpty(publicTimelineUrl))
                    return TwitterServerUrl + "statuses/public_timeline";
                else
                    return publicTimelineUrl;
            }
            set { publicTimelineUrl = value; }
        }

        /// <summary>
        /// Url to the Twitter Friends Timeline. Defaults to http://twitter.com/statuses/friends_timeline
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string FriendsTimelineUrl
        {
            get
            {
                if (string.IsNullOrEmpty(friendsTimelineUrl))
                    return TwitterServerUrl + "service/twitter/list.do";
                else
                    return friendsTimelineUrl;
            }
            set { friendsTimelineUrl = value; }
        }

        /// <summary>
        /// Url to the user's timeline. Defaults to http://twitter.com/statuses/user_timeline
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string UserTimelineUrl
        {
            get
            {
                if (string.IsNullOrEmpty(userTimelineUrl))
                    return TwitterServerUrl + "statuses/user_timeline";
                else
                    return userTimelineUrl;
            }
            set { userTimelineUrl = value; }
        }

        /// <summary>
        /// Url to the 20 most recent replies (status updates prefixed with @username posted by users who are friends with the user being replied to) to the authenticating user. 
        /// Defaults to http://twitter.com/statuses/user_timeline
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string RepliesTimelineUrl
        {
            get
            {
                if (string.IsNullOrEmpty(repliesTimelineUrl))
                    //return TwitterServerUrl + "service/twitter/list.do?userType=re";
                    return TwitterServerUrl + "statuses/replies";
                else
                    return repliesTimelineUrl;
            }
            set { repliesTimelineUrl = value; }
        }

        /// <summary>
        /// Url to the list of the 20 most recent direct messages sent to the authenticating user.  Defaults to http://twitter.com/direct_messages
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string DirectMessagesUrl
        {
            get
            {
                if (string.IsNullOrEmpty(directMessagesUrl))
                    return TwitterServerUrl + "direct_messages";
                else
                    return directMessagesUrl;
            }
            set { directMessagesUrl = value; }
        }

        /// <summary>
        /// Url to the Twitter HTTP Post. Defaults to http://twitter.com/statuses/update
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string UpdateUrl
        {
            get
            {
                if (string.IsNullOrEmpty(updateUrl))
                    return TwitterServerUrl + "service/twitter/publish.do?";
                    //return TwitterServerUrl + "statuses/update";
                else
                    return updateUrl;
            }
            set { updateUrl = value; }
        }

        public string FavTweetUrl
        {
            get
            {
                if (string.IsNullOrEmpty(favTweetUrl))
                    return TwitterServerUrl + "service/twitter/tweetFavourite.do?";
                    //return TwitterServerUrl + "statuses/update";
                else
                    return favTweetUrl;
            }
            set { favTweetUrl = value; }
        }

        /// <summary>
        /// Url to the user's friends. Defaults to http://twitter.com/statuses/friends
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string FriendsUrl
        {
            get
            {
                if (string.IsNullOrEmpty(friendsUrl))
                    return TwitterServerUrl + "statuses/friends";
                else
                    return friendsUrl;
            }
            set { friendsUrl = value; }
        }

        /// <summary>
        /// Url to the user's followers. Defaults to http://twitter.com/statuses/followers
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string FollowersUrl
        {
            get
            {
                if (string.IsNullOrEmpty(followersUrl))
                    return TwitterServerUrl + "statuses/followers";
                else
                    return followersUrl;
            }
            set { followersUrl = value; }
        }

        /// <summary>
        /// Returns extended information of a given user, specified by ID or screen name as per the required id parameter below.  
        /// This information includes design settings, so third party developers can theme their widgets according to a given user's preferences. 
        /// Defaults to http://twitter.com/users/show/
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string UserShowUrl
        {
            get
            {
                if (string.IsNullOrEmpty(userShowUrl))
                    return TwitterServerUrl + "users/show/";
                else
                    return userShowUrl;
            }
            set { userShowUrl = value; }
        }

        /// <summary>
        /// Url to sends a new direct message to the specified user from the authenticating user. Defaults to http://twitter.com/direct_messages/new
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string SendMessageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(sendMessageUrl))
                    return TwitterServerUrl + "direct_messages/new";
                else
                    return sendMessageUrl;
            }
            set { sendMessageUrl = value; }
        }

        /// <summary>
        /// Url to destroy a status from the authenticating user. Defaults to http://twitter.com/statuses/destroy
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string DestroyUrl
        {
            get
            {
                if (string.IsNullOrEmpty(destroyUrl))
                    return TwitterServerUrl + "service/twitter/tweetDelete.do";//"statuses/destroy/";
                else
                    return destroyUrl;
            }
            set { destroyUrl = value; }
        }

        /// <summary>
        /// Url to destroy a direct message from the authenticating user. Defaults to http://twitter.com/direct_messages/destroy/
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string DestroyDirectMessageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(destroyDirectMessageUrl))
                    return TwitterServerUrl + "direct_messages/destroy/";
                else
                    return destroyDirectMessageUrl;
            }
            set { destroyDirectMessageUrl = value; }
        }



        public string CreateFriendshipUrl
        {
            get {
                if (string.IsNullOrEmpty(createFriendshipUrl))
                {
                    return TwitterServerUrl + "friendships/create/";
                }
                else
                {
                    return createFriendshipUrl;
                }
            }
            set { createFriendshipUrl = value; }
        }


        /// <summary>
        /// Returns the remaining number of API requests available to the requesting user before the API limit is reached for the current hour. 
        /// Calls to rate_limit_status do not count against the rate limit.  If authentication credentials are provided, the rate limit status 
        /// for the authenticating user is returned.  Otherwise, the rate limit status for the requester's IP address is returned. Defaults to 
        /// http://twitter.com/direct_messages/account/rate_limit_status
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string RateLimitStatusUrl
        {
            get
            {
                if (string.IsNullOrEmpty(rateLimitStatusUrl))
                {
                    return TwitterServerUrl + "account/rate_limit_status";
                }
                else
                {
                    return rateLimitStatusUrl;
                }
            }
            set { rateLimitStatusUrl = value; }
        }

        /// <summary>
        ///This is the url to return an individual tweet. 
        /// </summary>
        /// <remarks>
        /// Origionally done for "Conversations" but I'm sure it will be re-used.
        /// </remarks>
        public string TweetUrl
        {
            get
            {
                if (string.IsNullOrEmpty(tweetUrl))
                {
                    return TwitterServerUrl + "statuses/show/";
                }
                else
                {
                    return tweetUrl;
                }
            }
            set { tweetUrl = value; }
        }
        /// <summary>
        /// Returns an HTTP 200 OK response code and a representation of the requesting user if authentication was successful; returns a 401 status
        /// code and an error message if not.  Use this method to test if supplied user credentials are valid. Defaults to
        /// http://twitter.com/account/verify_credentials
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string VerifyCredentialsUrl
        {
            get
            {
                if (string.IsNullOrEmpty(verifyCredentialsUrl))
                {
                    //return TwitterServerUrl + "account/verify_credentials";
                    return TwitterServerUrl + string.Format("service/login.do?userId={0}&passWord={1}", username, ToInsecureString(password));
                }
                else
                {
                    return verifyCredentialsUrl;
                }
            }
            set { verifyCredentialsUrl = value; }
        }

        /// <summary>
        /// The format of the results from the twitter API. Ex: .xml, .json, .rss, .atom. Defaults to ".xml"
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string Format
        {
            get
            {
                if (string.IsNullOrEmpty(format))
                    return ".xml";
                else
                    return format;
            }
            set { format = value; }
        }

        
      

        /// <summary>
        /// The number of characters available for the Tweet text. Defaults to 140.
        /// </summary>
        /// <remarks>
        /// This value should only be changed if the character limit on Twitter.com has been changed.
        /// </remarks>
        public static int CharacterLimit
        {
            get
            {
                if (characterLimit == 0)
                    return 140;
                else
                    return characterLimit;
            }
            set { characterLimit = value; }
        }

        /// <summary>
        /// The name of the current client. Defaults to "Witty"
        /// </summary>
        /// <remarks>
        /// This value can be changed if you're using this Library for your own twitter client
        /// </remarks>
        public string ClientName
        {
            get
            {
                if (string.IsNullOrEmpty(clientName))
                    return "witty";
                else
                    return clientName;
            }
            set { clientName = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Unauthenticated constructor
        /// </summary>
        public RenmeiNet()
        {
        }

        /// <summary>
        /// Authenticated constructor
        /// </summary>
        public RenmeiNet(string username, SecureString password)
        {
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Authenticated constructor with Proxy.
        /// </summary>
        /// <remarks>
        /// JMF: Note that this constructor should not be necessary.  The default proxy
        /// (HttpWebRequest.DefaultWebProxy) needs to be set to the user-configured
        /// proxy setting during app startup so that WPF controls that access the 
        /// internet directly (like Image controls) can do so.
        /// </remarks>
        public RenmeiNet(string username, SecureString password, IWebProxy webProxy)
        {
            this.username = username;
            this.password = password;
            this.webProxy = webProxy;
        }

        #endregion

        #region Public Methods

        #region Timeline Methods

        /// <summary>
        /// Retrieves the public timeline
        /// </summary>
        public TweetCollection GetPublicTimeline()
        {
            return RetrieveTimeline(Timeline.Public);
        }

        /// <summary>
        /// Retrieves the public timeline. Narrows the result to after the since date.
        /// </summary>
        public TweetCollection GetPublicTimeline(string since)
        {
            return RetrieveTimeline(Timeline.Public, since);
        }

        /// <summary>
        /// Retrieves the friends timeline
        /// </summary>
        public TweetCollection GetFriendsTimeline()
        {
            if (!string.IsNullOrEmpty(username))
                return RetrieveTimeline(Timeline.Friends);
            else
                return RetrieveTimeline(Timeline.Public);
        }

        /// <summary>
        /// Retrieves the friends timeline. Narrows the result to after the since date.
        /// </summary>
        public TweetCollection GetFriendsTimeline(string since)
        {
            if (!string.IsNullOrEmpty(username))
                return RetrieveTimeline(Timeline.Friends, since);
            else
                return RetrieveTimeline(Timeline.Public, since);
        }

        /// <summary>
        /// Retrieves the friends timeline. Narrows the result to after the since date.
        /// </summary>
        public TweetCollection GetFriendsTimeline(string since, string userId)
        {
            return RetrieveTimeline(Timeline.Friends, since, userId);
        }

        public TweetCollection GetUserTimeline(string userId)
        {
            return RetrieveTimeline(Timeline.User, string.Empty, userId);
        }

        public TweetCollection GetReplies()
        {
            return RetrieveTimeline(Timeline.Replies);
        }

        public TweetCollection GetReplies(string since)
        {
            return RetrieveTimeline(Timeline.Replies, since);
        }

        public TweetCollection GetFavoriteTweets()
        {
            return RetrieveTimeline(Timeline.Favorite);
        }

        public TweetCollection GetConversation(double id)
        {
            TweetCollection tweets = new TweetCollection();
            Tweet tweet = RetrieveTweet(id);
            while (tweet.InReplyTo != null)
            {
                tweets.Add(tweet);
                id = (double)tweet.InReplyTo;
                tweet = RetrieveTweet(id);
            }

            tweets.Add(tweet);
            return tweets;
        }

        #endregion

        /// <summary>
        /// Returns the authenticated user's friends who have most recently updated, each with current status inline.
        /// </summary>
        public UserCollection GetFriends()
        {
            return GetFriends(CurrentlyLoggedInUser.Id);
        }

        /// <summary>
        /// Returns the user's friends who have most recently updated, each with current status inline.
        /// </summary>
        public UserCollection GetFriends(int userId)
        {
            try
            {
                UserCollection users = new UserCollection();

				//This is ugly - we're mapping from TweetSharp users to our internal Twitter class
				//This will all go away when we move wholesale to TweetSharp
                foreach (TwitterUser user in GetFriendsInternal())
                    users.Add(CreateUser(user));

                return users;
            }
            catch
            {
                return null;
            }
        }

		public IEnumerable<TwitterUser> GetFriendsInternal()
		{
			IEnumerable<TwitterUser> _friends;

				var twitter = FluentTwitter.CreateRequest()
							.AuthenticateAs(username, ToInsecureString(password))
							.Configuration.UseGzipCompression() //now using compression for performance
							.Users().GetFriends().For(username)
							.CreateCursor()
							.AsJson();
				_friends = GetAllCursorValues(twitter, s => s.AsUsers());

			return _friends; //return either the newly fetched list, or the cached copy
		}

		private static IEnumerable<T> GetAllCursorValues<T>(ITwitterLeafNode twitter, Func<TwitterResult, IEnumerable<T>> conversionMethod)
		{
		   long? nextCursor = -1 ;
		   var ret = new List<T>();
		   do
		   {
			   twitter.Root.Parameters.Cursor = nextCursor;
			   var response = twitter.Request();
			   IEnumerable<T> values = conversionMethod(response);
			   if (values != null)
			   {
				   ret.AddRange(values);
			   }
			   nextCursor = response.AsNextCursor();
		   } while (nextCursor.HasValue && nextCursor.Value != 0);
		   return ret;
		}

        public User GetUser(int userId)
        {
            User user = new User();

            string requestURL = UserShowUrl  + userId + Format;

            HttpWebRequest request = CreateTwitterRequest(requestURL);

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Load the response data into a XmlDocument  
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                // Get statuses with XPath  
                XmlNode userNode = doc.SelectSingleNode("user");

                if (userNode != null)
                {
                    user = CreateUser(userNode);
                }
            }

            return user;
        }

        public void PostPhoto(System.IO.FileInfo file, string text)
        {

            //var twitter = FluentTwitter.CreateRequest()
            //.AuthenticateAs(username, ToInsecureString(password))
            //.Statuses().Update("test")  
            //.AsJson();  

            //var response = twitter.Request();
            System.Net.ServicePointManager.Expect100Continue = false;

            var request = FluentTwitter.CreateRequest(new Dimebrain.TweetSharp.TwitterClientInfo() { ClientName = "Witty" })
            .AuthenticateAs(username, ToInsecureString(password))
            .Photos().PostPhoto(file.FullName, Dimebrain.TweetSharp.Fluent.Services.SendPhotoServiceProvider.TwitGoo)
            .Statuses().Update(text)
            .AsJson();
            var response = request.Request();
            string monkey = response.AsStatus().Text;
        }

        /// <summary>
        /// Delete a tweet from a users timeline
        /// </summary>
        /// <param name="id">id of the Tweet to delete</param>
        public void DestroyTweet(double id)
        {
            string urlToCall = DestroyUrl + "?" + getAuthUrl();
            urlToCall += "&tweetId=" + id.ToString();
            //string urlToCall = string.Format("{0}{1:g}{2}", DestroyUrl, id, Format);
            MakeDestroyRequestCall(urlToCall);
        }

        /// <summary>
        /// Destroy a direct message sent by a user
        /// </summary>
        /// <param name="id">id of the direct message to delete</param>
        public void DestroyDirectMessage(double id)
        {
            string urlToCall = string.Format("{0}{1:g}{2}", DestroyDirectMessageUrl, id, Format);
            MakeDestroyRequestCall(urlToCall);
        }

        /// <summary>
        /// Post new tweet to Twitter
        /// </summary>
        /// <returns>newly added tweet</returns>
        public Tweet AddTweet(string text)
        {
            return AddTweet(text, 0);
        }

        public Tweet AddTweet(string text, double replyid)
        {
            Tweet tweet = new Tweet();
            tweet.IsDirectMessage = (text.StartsWith("d ", StringComparison.CurrentCultureIgnoreCase));

            if (string.IsNullOrEmpty(text))
                return null;

            text = HttpUtility.UrlEncode(text);
            string upUrl = UpdateUrl + string.Format("userId={0}&authCode={1}", email, authToken);
            // Create the web request  
            //UpdateUrl+=string.Format("userId={0}&authCode={1}", email, authToken);
            //UpdateUrl += string.Format("&content={0}&previousId={1}", text, replyid.ToString());
            HttpWebRequest request = CreateTwitterRequest(upUrl);

            request.ServicePoint.Expect100Continue = false;

            request.Method = "POST";



            // Set values for the request back
            request.ContentType = "application/x-www-form-urlencoded";
            string param = "content=" + text;
            string replyParam = "&previousId=" + replyid.ToString();
            //string sourceParam = "&source=" + ClientName;
            request.ContentLength = param.Length;// +sourceParam.Length;
            request.ContentLength += replyParam.Length;
            //if (replyid > 0)
            //{
            //    request.ContentLength += replyParam.Length;
            //}

            // Write the request paramater
            StreamWriter stOut = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
            stOut.Write(param);
            //stOut.Write(sourceParam);
            stOut.Write(replyParam);
            //if (replyid > 0)
            //{
            //    stOut.Write(replyParam);
            //}
            stOut.Close();

            // Do the request to get the response
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Load the response data into a XmlDocument  
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                XmlNode node = doc.SelectSingleNode("result/tweet");
                if (node == null) return null;

                tweet.Id = double.Parse(node.SelectSingleNode("tweetId").InnerText);
                tweet.Text=HttpUtility.HtmlDecode(node.SelectSingleNode("tContent").InnerText);
                 //Defect 43 - Twitter incorrectly returns last tweet sent when you direct message someone.
                 //tweet.Text = tweet.IsDirectMessage
                 //                   ? HttpUtility.UrlDecode(text)
                 //                   : HttpUtility.HtmlDecode(node.SelectSingleNode("text").InnerText);

                string source = HttpUtility.HtmlDecode(node.SelectSingleNode("clientType").InnerText);
                // Remove html from the source string
                if (!string.IsNullOrEmpty(source))
                    tweet.Source = Regex.Replace(source, @"<(.|\n)*?>", string.Empty);

                string dateString = node.SelectSingleNode("tPublishTime").InnerText;
                if (!string.IsNullOrEmpty(dateString))
                {
                    tweet.DateCreated = DateTime.ParseExact(
                        dateString,
                        renmaikuPublishDateFormat,//twitterCreatedAtDateFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces);
                    //CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"), DateTimeStyles.AllowWhiteSpaces);
                }
                else
                {
                    tweet.DateCreated = DateTime.Now;
                }
                //string replyTo = HttpUtility.HtmlDecode(node.SelectSingleNode("in_reply_to_status_id").InnerText);
                //if (!string.IsNullOrEmpty(replyTo))
                //{
                //    tweet.InReplyTo = double.Parse(HttpUtility.HtmlDecode(node.SelectSingleNode("in_reply_to_status_id").InnerText));

                //}
                tweet.IsNew = true;


                XmlNode userNode = node.SelectSingleNode("user");
                User user = CreateUser(userNode); 
                tweet.User = user;
            }

            return tweet;
        }

        public Comment AddComment(string text, Tweet tweet)
        {
            Comment co;
            string upUrl = TwitterServerUrl + "service/twitter/commentManage.do?";
            upUrl+=string.Format("userId={0}&authCode={1}", email, authToken);
            upUrl += string.Format("&operType=add&tweetId={0}", tweet.Id);//&commentSource=1
            text = HttpUtility.UrlEncode(text);
            HttpWebRequest request = CreateTwitterRequest(upUrl);
            request.ServicePoint.Expect100Continue = false;
            request.Method = "POST";

            // Set values for the request back
            request.ContentType = "application/x-www-form-urlencoded";
            string param = "content=" + text;
            request.ContentLength = param.Length;// +sourceParam.Length;

            // Write the request paramater
            StreamWriter stOut = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
            stOut.Write(param);
            stOut.Close();
            // Do the request to get the response
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Load the response data into a XmlDocument  
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                XmlNode node = doc.SelectSingleNode("result/comment");
                if (node == null) return null;
                co = CreateComment(node);
                
            }
            return co;
        }

        public CommentCollection RetriveComments(Tweet tweet)
        {
            CommentCollection comments = new CommentCollection();
            string commentsUrl = TwitterServerUrl + "service/twitter/commentList.do?";
            commentsUrl += string.Format("userId={0}&authCode={1}&tweetId={2}", email, authToken, tweet.Id);
            // Create the web request
            HttpWebRequest request = CreateTwitterRequest(commentsUrl);

            // moved this out of the try catch to use it later on in the XMLException
            // trying to fix a bug someone report
            XmlDocument doc = new XmlDocument();
            try
            {
                // Get the Web Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    doc.Load(reader);
                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/result/cList/comment");

                    foreach (XmlNode node in nodes)
                    {
                        Comment co = CreateComment(node);
                        comments.Add(co);
                    }
                }
            }
            catch (XmlException exXML)
            {
                // adding the XML document data to the exception so it will get logged
                // so we can debug the issue
                exXML.Data.Add("XMLDoc", doc);
                throw;
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }
            return comments;
        }

        public TweetCollection RetriveCommentedTweets()
        {
            return RetrieveTimeline(Timeline.Comments);
        }

        public TweetCollection RetriveMySelfTweets()
        {
            return RetrieveTimeline(Timeline.Myself);
        }

        private Comment CreateComment(XmlNode node)
        {
            Comment co = new Comment();
            co.Id = long.Parse(node.SelectSingleNode("cId").InnerText);
            co.Text = HttpUtility.HtmlDecode(node.SelectSingleNode("content").InnerText);
            co.ClientType = HttpUtility.HtmlDecode(node.SelectSingleNode("clientType").InnerText);
            string dateString = node.SelectSingleNode("time").InnerText;
            if (!string.IsNullOrEmpty(dateString))
            {
                co.PublishedTime = DateTime.ParseExact(
                    dateString,
                    renmaikuPublishDateFormat,//twitterCreatedAtDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces);
                //CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"), DateTimeStyles.AllowWhiteSpaces);
            }
            else
            {
                co.PublishedTime = DateTime.Now;
            }
            XmlNode userNode = node.SelectSingleNode("user");
            User user = CreateUser(userNode);
            co.User = user;
            return co;
        }

        public void AddFavTweet(double tid)
        {
            string favUrl = FavTweetUrl + string.Format("userId={0}&authCode={1}&tweetId={2}", email, authToken,tid.ToString());
            HttpWebRequest request = CreateTwitterRequest(favUrl);
            request.ServicePoint.Expect100Continue = false;
            request.Method = "POST";

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Load the response data into a XmlDocument  
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                XmlNode node = doc.SelectSingleNode("result/status");
                if (node == null) return ;
            }
        }

        protected string GetPropertyFromXml(XmlNode twitterNode, string propertyName)
        {
            if (twitterNode != null)
            {
                XmlNode propertyNode = twitterNode.SelectSingleNode(propertyName);
                if (propertyNode != null)
                {
                    return propertyNode.InnerText;
                }
            }
            return String.Empty;
        }

        private User CreateUser(XmlNode userNode)
        {
            User user = new User();

            if (userNode != null)
            {
                int temp = -1;
                //# int.TryParse(GetPropertyFromXml(userNode, "id"), out temp);
                int.TryParse(GetPropertyFromXml(userNode, "userId"), out temp);
                user.Id = temp;
                //# user.Name = GetPropertyFromXml(userNode, "name");
                user.Name = GetPropertyFromXml(userNode, "nickName");
                //# user.ScreenName = GetPropertyFromXml(userNode, "screen_name");
                user.ScreenName = GetPropertyFromXml(userNode, "nickName");
                //# user.ImageUrl = GetPropertyFromXml(userNode, "profile_image_url");
                user.ImageUrl = GetPropertyFromXml(userNode, "headImgUrl");
                //# user.SiteUrl = GetPropertyFromXml(userNode, "url");
                //# user.Location = GetPropertyFromXml(userNode, "location");
                //# user.Description = GetPropertyFromXml(userNode, "description");

                // Add for Renmei.com
                user.Description = GetPropertyFromXml(userNode, "company");
                user.Description = GetPropertyFromXml(userNode, "position");

                //# user.BackgroundColor = GetPropertyFromXml(userNode, "profile_background_color");
                //# user.TextColor = GetPropertyFromXml(userNode, "profile_text_color");
                //# user.LinkColor = GetPropertyFromXml(userNode, "profile_link_color");
                //# user.SidebarBorderColor = GetPropertyFromXml(userNode, "profile_sidebar_border_color");
                //# user.SidebarFillColor = GetPropertyFromXml(userNode, "profile_sidebar_fill_color");

                temp = 0;
                int.TryParse(GetPropertyFromXml(userNode, "friends_count"), out temp);
                user.FollowingCount = temp;

                temp = 0;
                int.TryParse(GetPropertyFromXml(userNode, "favourites_count"), out temp);
                user.FavoritesCount = temp;

                temp = 0;
                int.TryParse(GetPropertyFromXml(userNode, "statuses_count"), out temp);
                user.StatusesCount = temp;

                temp = 0;
                int.TryParse(GetPropertyFromXml(userNode, "followers_count"), out temp);
                user.FollowersCount = temp;
            }

            return user;
        }

        private User CreateUser(TwitterUser twitterUser)
        {
            User user = new User();

            if (twitterUser != null)
            {
                user.Id = twitterUser.Id;

                user.Name = twitterUser.Name;
                user.ScreenName = twitterUser.ScreenName;
                user.ImageUrl = twitterUser.ProfileImageUrl;
                user.SiteUrl = twitterUser.Url;
                user.Location = twitterUser.Location;
                user.Description = twitterUser.Description;
                user.BackgroundColor = twitterUser.ProfileBackgroundColor;
                user.TextColor = twitterUser.ProfileTextColor;
                user.LinkColor = twitterUser.ProfileLinkColor;
                user.SidebarBorderColor = twitterUser.ProfileSidebarBorderColor;
                user.SidebarFillColor = twitterUser.ProfileSidebarFillColor;
                user.FollowingCount = twitterUser.FriendsCount;
                user.FavoritesCount = twitterUser.FavouritesCount;
                user.StatusesCount = twitterUser.StatusesCount;
                user.FollowersCount = twitterUser.FollowersCount;
            }

            return user;
        }

        private FriendGroup CreateFriendGroup(XmlNode groupNode)
        {
            FriendGroup group = new FriendGroup();
            if (groupNode != null)
            {
                int temp = -1;
                //int.TryParse(GetPropertyFromXml(groupNode, "groupId"), out temp);
                //group.Id = temp;
                group.Title = GetPropertyFromXml(groupNode, "title");

                long ltmp;
                long.TryParse(GetPropertyFromXml(groupNode, "groupId"), out ltmp);
                group.GroupId = ltmp;

                int.TryParse(GetPropertyFromXml(groupNode, "order_num"), out temp);
                group.Order = temp;
                int.TryParse(GetPropertyFromXml(groupNode, "friend_count"), out temp);
                group.Count = temp;
            }
            return group;
        }

        public FriendGroupCollection getFriendGroups()
        {
            FriendGroupCollection groups = new FriendGroupCollection();
            string groupUrl = TwitterServerUrl + "service/twitter/groupList.do";
            groupUrl += "?" + getAuthUrl();
            // Create the web request
            HttpWebRequest request = CreateTwitterRequest(groupUrl);

            // moved this out of the try catch to use it later on in the XMLException
            // trying to fix a bug someone report
            XmlDocument doc = new XmlDocument();
            try
            {
                // Get the Web Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    doc.Load(reader);
                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/result/groupList/group");

                    foreach (XmlNode node in nodes)
                    {
                        FriendGroup fg = CreateFriendGroup(node);
                        fg.MemberList = GetFriendsbyGroupID(fg.GroupId, "", fg.Count);
                        groups.Add(fg);
                    }
                }
            }
            catch (XmlException exXML)
            {
                // adding the XML document data to the exception so it will get logged
                // so we can debug the issue
                exXML.Data.Add("XMLDoc", doc);
                throw;
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }
            return groups;
        }

        public UserCollection getFollowMeFriendsList()
        {
            UserCollection list = new UserCollection();
            string groupUrl = TwitterServerUrl + "service/twitter/friendList.do";

            groupUrl += "?listType=beConcern&limit=50&" + getAuthUrl();
            // Create the web request
            HttpWebRequest request = CreateTwitterRequest(groupUrl);

            // moved this out of the try catch to use it later on in the XMLException
            // trying to fix a bug someone report
            XmlDocument doc = new XmlDocument();
            try
            {
                // Get the Web Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    doc.Load(reader);
                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/result/friendList/user");

                    foreach (XmlNode node in nodes)
                    {
                        User fg = CreateUser(node);
                        list.Add(fg);
                    }
                }
            }
            catch (XmlException exXML)
            {
                // adding the XML document data to the exception so it will get logged
                // so we can debug the issue
                exXML.Data.Add("XMLDoc", doc);
                throw;
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }
            return list;
        }

        public UserCollection getMyFollowFriendsList()
        {
            UserCollection list = new UserCollection();
            string groupUrl = TwitterServerUrl + "service/twitter/friendList.do";

            groupUrl += "?listType=concern&limit=50&" + getAuthUrl();
            // Create the web request
            HttpWebRequest request = CreateTwitterRequest(groupUrl);

            // moved this out of the try catch to use it later on in the XMLException
            // trying to fix a bug someone report
            XmlDocument doc = new XmlDocument();
            try
            {
                // Get the Web Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    doc.Load(reader);
                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/result/friendList/user");

                    foreach (XmlNode node in nodes)
                    {
                        User fg = CreateUser(node);
                        list.Add(fg);
                    }
                }
            }
            catch (XmlException exXML)
            {
                // adding the XML document data to the exception so it will get logged
                // so we can debug the issue
                exXML.Data.Add("XMLDoc", doc);
                throw;
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }
            return list;
        }
        /// <summary>
        /// Returns the authenticated user's friends who have most recently updated, each with current status inline.
        /// </summary>
        public UserCollection GetFriendsbyGroupID(long groupId, string listTpye, int limit)
        {
            UserCollection members = new UserCollection();
            string groupUrl = TwitterServerUrl + "service/twitter/friendList.do";
            if (0 != groupId)
            {
                groupUrl += "?groupId=" + groupId.ToString();
            }
            if (!string.IsNullOrEmpty(listTpye))
            {
                groupUrl += "&listType=" + listTpye;
            }
            if (0 != limit)
            {
                groupUrl += "&limit=" + limit.ToString();
            }
            groupUrl += "&" + getAuthUrl();
            // Create the web request
            HttpWebRequest request = CreateTwitterRequest(groupUrl);

            // moved this out of the try catch to use it later on in the XMLException
            // trying to fix a bug someone report
            XmlDocument doc = new XmlDocument();
            try
            {
                // Get the Web Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    doc.Load(reader);
                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/result/friendList/user");

                    foreach (XmlNode node in nodes)
                    {
                        User us = CreateUser(node);
                        members.Add(us);
                    }
                }
            }
            catch (XmlException exXML)
            {
                // adding the XML document data to the exception so it will get logged
                // so we can debug the issue
                exXML.Data.Add("XMLDoc", doc);
                throw;
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }
            return members;

        }

        private string getAuthUrl()
        {
            return string.Format("userId={0}&authCode={1}", email, authToken);
        }

        /// <summary>
        /// Authenticating with the provided credentials and retrieve the user's settings
        /// </summary>
        /// <returns></returns>
        public User Login()
        {
            User user = new User();

            // Create the web request
            // HttpWebRequest request = CreateTwitterRequest(VerifyCredentialsUrl + Format);
            HttpWebRequest request = CreateTwitterRequest(VerifyCredentialsUrl);

            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);
                    XmlNode authNode = doc.SelectSingleNode("results/authCode");
                    if (authNode != null)
                    {
                        authToken = authNode.InnerText;
                    }
                    email = username;

                    // Get statuses with XPath  
                    XmlNode userNode = doc.SelectSingleNode("results/user");

                    if (userNode != null)
                    {
                        email = GetPropertyFromXml(userNode, "email");
                        user = CreateUser(userNode);
                    }
                }
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }
            currentLoggedInUser = user;
            return user;
        }

        #region Secure String Members
        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("WittyPasswordSalt");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EncryptString(System.Security.SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }
        #endregion

        /// <summary>
        /// Get rate limit status for the user
        /// </summary>
        /// <returns>String with the number of usages left out over the total, e.g. "93/100"</returns>
        public string RetrieveRateLimitStatus()
        {
            HttpWebRequest request = CreateTwitterRequest(RateLimitStatusUrl + Format);
            string result = String.Empty;

            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);

                    XmlNode limit = doc.SelectSingleNode("/hash/hourly-limit");
                    XmlNode remaining = doc.SelectSingleNode("/hash/remaining-hits");
                    result = remaining.InnerText + "/" + limit.InnerText;
                }
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }
            return result;
        }

        public DirectMessageCollection RetrieveMessages()
        {
            return RetrieveMessages(string.Empty);
        }

        /// <summary>
        /// Gets direct messages for the user
        /// </summary>
        /// <returns>Collection of direct messages</returns>
        public DirectMessageCollection RetrieveMessages(string since)
        {
            DirectMessageCollection messages = new DirectMessageCollection();

            string url = DirectMessagesUrl + Format;

            if (!string.IsNullOrEmpty(since))
            {
                DateTime sinceDate;
                DateTime.TryParse(since, out sinceDate);

                // Go back a minute to compensate for latency.
                sinceDate = sinceDate.AddMinutes(-1);
                string sinceDateString = sinceDate.ToString(twitterSinceDateFormat);
                url += "?since=" + sinceDateString;
            }

            HttpWebRequest request = CreateTwitterRequest(url);
            
            try
            {
                // Get the Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);

                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/direct-messages/direct_message");

                    foreach (XmlNode node in nodes)
                    {
                        DirectMessage message = new DirectMessage();
                        message.Id = double.Parse(node.SelectSingleNode("id").InnerText);
                        message.Text = HttpUtility.HtmlDecode(node.SelectSingleNode("text").InnerText);

                        string dateString = node.SelectSingleNode("created_at").InnerText;
                        if (!string.IsNullOrEmpty(dateString))
                        {
                            message.DateCreated = DateTime.ParseExact(
                                dateString,
                                twitterCreatedAtDateFormat,
                                CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"), DateTimeStyles.AllowWhiteSpaces);
                        }

                        XmlNode senderNode = node.SelectSingleNode("sender");
                        User sender = CreateUser(senderNode);
                        message.Sender = sender;

                        XmlNode recipientNode = node.SelectSingleNode("recipient");
                        User recipient = CreateUser(recipientNode);
                        message.Recipient = recipient;

                        messages.Add(message);
                    }
                }
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }
            return messages;
        }

        public Tweet RetrieveTweet(double id)
        {
            Tweet tweet = new Tweet();

            string url = string.Format("{0}{1}{2}", TweetUrl, id, Format);
            

           

            HttpWebRequest request = CreateTwitterRequest(url);
            try
            {
                // Get the Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);

                    // Get statuses with XPath  
                    XmlNode node = doc.SelectSingleNode("/status");

                    tweet.Id = double.Parse(node.SelectSingleNode("id").InnerText);
                    tweet.Text = HttpUtility.HtmlDecode(node.SelectSingleNode("text").InnerText);
                    string source = HttpUtility.HtmlDecode(node.SelectSingleNode("source").InnerText);
                    if (!string.IsNullOrEmpty(source))
                        tweet.Source = Regex.Replace(source, @"<(.|\n)*?>", string.Empty);

                    string dateString = node.SelectSingleNode("created_at").InnerText;
                    if (!string.IsNullOrEmpty(dateString))
                    {
                        tweet.DateCreated = DateTime.ParseExact(
                            dateString,
                            twitterCreatedAtDateFormat,
                            CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"), DateTimeStyles.AllowWhiteSpaces);
                    }
                    string replyTo = HttpUtility.HtmlDecode(node.SelectSingleNode("in_reply_to_status_id").InnerText);
                    if (!string.IsNullOrEmpty(replyTo))
                    {
                        tweet.InReplyTo = double.Parse(HttpUtility.HtmlDecode(node.SelectSingleNode("in_reply_to_status_id").InnerText));

                    }
                    XmlNode userNode = node.SelectSingleNode("user");
                    User user = CreateUser(userNode);
                    tweet.User = user;
                    tweet.IsReply = IsReplyTweet(this.CurrentlyLoggedInUser.ScreenName, tweet);

                
                }
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }

            return tweet;
        }

        public void SendMessage(string user, string text)
        {
            // Jason Follas: Make sure that the user isn't trying to DM themselves.
            if (String.Compare(user, CurrentlyLoggedInUser.ScreenName, true) == 0)
                return;

            if (string.IsNullOrEmpty(text))
                return;

            text = HttpUtility.UrlEncode(text);

            // Create the web request  
            HttpWebRequest request = CreateTwitterRequest(SendMessageUrl + Format);
            request.ServicePoint.Expect100Continue = false;

            request.Method = "POST";

            // Set values for the request back
            request.ContentType = "application/x-www-form-urlencoded";
            string param = "text=" + text;
            string userParam = "&user=" + user;
            request.ContentLength = param.Length + userParam.Length;

            // Write the request paramater
            StreamWriter stOut = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
            stOut.Write(param);
            stOut.Write(userParam);
            stOut.Close();

            try
            {
                // Perform the web request
                request.GetResponse();
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }

        }

        /// <summary>
        /// Follow the user specified by userId
        /// </summary>
        /// <param name="userId"></param>
        public void FollowUser(string userName)
        {
            // Jason Follas: Make sure that the user isn't trying to follow themselves.
            if (String.Compare(userName, CurrentlyLoggedInUser.ScreenName, true) == 0)
                return;

            string followUrl = CreateFriendshipUrl + userName + Format;
            MakeTwitterApiCall(followUrl, "POST");
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Retrieves the specified timeline from Twitter
        /// </summary>
        /// <returns>Collection of Tweets</returns>
        private TweetCollection RetrieveTimeline(Timeline timeline)
        {
            return RetrieveTimeline(timeline, string.Empty);
        }

        /// <summary>
        /// Retrieves the specified timeline from Twitter
        /// </summary>
        /// <returns>Collection of Tweets</returns>
        private TweetCollection RetrieveTimeline(Timeline timeline, string since)
        {
            return RetrieveTimeline(timeline, since, string.Empty);
        }

        /// <summary>
        /// The Main function for interfacing with the Twitter API
        /// </summary>
        /// <returns>Collection of Tweets. Twitter limits the max to 20.</returns>
        private TweetCollection RetrieveTimeline(Timeline timeline, string since, string userId)
        {
            TweetCollection tweets = new TweetCollection();

            string timelineUrl = FriendsTimelineUrl;
            timelineUrl += "?authCode=" + authToken + "&userId=" + email;

            switch (timeline)
            {
                case Timeline.Public:
                    timelineUrl = PublicTimelineUrl;
                    break;
                case Timeline.Friends:
                    //timelineUrl = FriendsTimelineUrl;
                    break;
                case Timeline.User:
                    timelineUrl = UserTimelineUrl;
                    break;
                case Timeline.Replies:
                    timelineUrl += "&userType=re";
                    break;
                case Timeline.Favorite:
                    timelineUrl += "&userType=fa";
                    break;
                case Timeline.Comments:
                    timelineUrl += "&userType=comm";
                    break;
                case Timeline.Myself:
                    timelineUrl += "&userType=my";
                    break;
                default:
                    timelineUrl = PublicTimelineUrl;
                    break;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                //timelineUrl += "/" + userId + Format;
                //timelineUrl += "?anthCode=" + authToken + "&userId=" + userId;
            }
            else
            {
                //timelineUrl += Format;
            }

            if (!string.IsNullOrEmpty(since))
            {
                DateTime sinceDate;
                DateTime.TryParse(since, out sinceDate);

                // Go back a minute to compensate for latency.
                sinceDate = sinceDate.AddMinutes(-1);
                string sinceDateString = sinceDate.ToString(twitterSinceDateFormat);
                timelineUrl = timelineUrl + "?since=" + sinceDateString;
            }
            else
            {

                DateTime sinceDate=DateTime.Now;//.AddHours(-70);
                timelineUrl += "&timePoint=" + sinceDate.ToString("yyyy-MM-dd HH:mm:ss") + "&direction=back";

            }
            timelineUrl += "&limit=50";

            // Create the web request
           HttpWebRequest request = CreateTwitterRequest(timelineUrl);

            // moved this out of the try catch to use it later on in the XMLException
            // trying to fix a bug someone report
            XmlDocument doc = new XmlDocument();
            try
            {
                // Get the Web Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    doc.Load(reader);
                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/result/tList/tweet");

                    foreach (XmlNode node in nodes)
                    {
                        Tweet tweet = new Tweet();
                        tweet.Id = double.Parse(node.SelectSingleNode("tweetId").InnerText);
                        tweet.Text = HttpUtility.HtmlDecode(node.SelectSingleNode("tContent").InnerText);
                        string source = HttpUtility.HtmlDecode(node.SelectSingleNode("clientType").InnerText);
                        if (!string.IsNullOrEmpty(source))
                            tweet.Source = Regex.Replace(source, @"<(.|\n)*?>", string.Empty);

                        string dateString = node.SelectSingleNode("tPublishTime").InnerText;
                        if (!string.IsNullOrEmpty(dateString))
                        {
                            tweet.DateCreated = DateTime.ParseExact(
                                dateString,
                                renmaikuPublishDateFormat,//twitterCreatedAtDateFormat,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AllowWhiteSpaces);
                                //CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"), DateTimeStyles.AllowWhiteSpaces);
                        }
                        //string replyTo = HttpUtility.HtmlDecode(node.SelectSingleNode("in_reply_to_status_id").InnerText);
                        //if (!string.IsNullOrEmpty(replyTo))
                        //{
                        //    tweet.InReplyTo = double.Parse(HttpUtility.HtmlDecode(node.SelectSingleNode("in_reply_to_status_id").InnerText));

                        //}
                        XmlNode userNode = node.SelectSingleNode("user");
                        User user = CreateUser(userNode);
                        tweet.User = user;
                        tweet.Timeline = timeline;
                        tweet.IsReply = IsReplyTweet(this.CurrentlyLoggedInUser.ScreenName, tweet);

                        tweets.Add(tweet);
                    }

                    tweets.SaveToDisk();
                }
            }
            catch (XmlException exXML)
            {
                // adding the XML document data to the exception so it will get logged
                // so we can debug the issue
                exXML.Data.Add("XMLDoc", doc);
                throw;
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp, timeline, userId);
            }
            return tweets;
        }

        private bool IsReplyTweet(string userId, Tweet tweet)
        {
            if (tweet.Timeline == Timeline.Replies)
                return true;

            if (tweet.Timeline == Timeline.Friends)
            {
                if (tweet.Text.Contains("@" + userId))
                    return true;
            }

            return false;
        }

        public FriendGroupCollection RetrieveFriendGroups()
        {
            return null;
        }

        /// <summary>
        /// Generic call to destroy a status ** still in progress **
        /// </summary>
        /// <param name="urlToCall"></param>
        private void MakeDestroyRequestCall(string urlToCall)
        {
            MakeTwitterApiCall(urlToCall, "POST");
        }

        /// <summary>
        /// Default Twitter API call uses GET
        /// </summary>
        /// <param name="urlToCall"></param>
        private void MakeTwitterApiCall(string urlToCall)
        {
            MakeTwitterApiCall(urlToCall, "GET");
        }

        /// <summary>
        /// Generic Twitter API call. Use this when you don't need/want to parse the return message
        ///  and just want to make a succeed/fail call to the API.
        /// </summary>
        /// <param name="urlToCall"></param>
        private void MakeTwitterApiCall(string urlToCall, string method)
        {
            //REMARK: We may want to refactor this to return the message returned by the API.
            // Create the web request  
            HttpWebRequest request = CreateTwitterRequest(urlToCall);
            request.ServicePoint.Expect100Continue = false;

            request.Method = method;

            try
            {
                // perform the destroy web request
                request.GetResponse();
            }
            catch (WebException webExcp)
            {
                ParseWebException(webExcp);
            }
        }


        private HttpWebRequest CreateTwitterRequest(string Uri)
        {
            // Create the web request
            HttpWebRequest request = WebRequest.Create(Uri) as HttpWebRequest;

            //// Add credentials to request  
            //byte[] authBytes = Encoding.UTF8.GetBytes(String.Format("{0}:{1}",username,RenmeiNet.ToInsecureString(password)).ToCharArray());
            //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);

            // Add configured web proxy
            request.Proxy = webProxy;
            return request;
        }

        private void ParseWebException(WebException webExcp)
        {
            ParseWebException(webExcp, null, null);
        }

        private void ParseWebException(WebException webExcp, Timeline? timeline, string userId)
        {
            // Get the WebException status code.
            WebExceptionStatus status = webExcp.Status;
            // If status is WebExceptionStatus.ProtocolError, 
            //   there has been a protocol error and a WebResponse 
            //   should exist. Display the protocol error.
            if (status == WebExceptionStatus.ProtocolError)
            {
                // Get HttpWebResponse so that you can check the HTTP status code.
                HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;

                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.NotModified: // no new tweets so ignore error.
                        break;

                    case HttpStatusCode.BadRequest: // rate limit exceeded
                        string message = String.Format("Rate limit exceeded. You have {0} of your requests left. Please try again in a few minutes", RetrieveRateLimitStatus());
                        throw new RateLimitException(message);

                    case HttpStatusCode.Unauthorized:
                        throw new SecurityException("Not Authorized.");

                    case HttpStatusCode.NotFound: // specified user does not exist
                        if (timeline == Timeline.User)
                            throw new UserNotFoundException(userId, "@" + userId + " does not exist (probably mispelled)");
                        else // what if a 404 happens to occur in another scenario?
                            throw webExcp;

                    case HttpStatusCode.ProxyAuthenticationRequired: 
                        throw new ProxyAuthenticationRequiredException("Proxy authentication required.");

                    case HttpStatusCode.RequestTimeout:
                        throw new RequestTimeoutException("Request timed out. We'll try again in a few minutes");

                    case HttpStatusCode.BadGateway: //Twitter is freaking out.
                        throw new BadGatewayException("Fail Whale!  There was a problem calling the Twitter API.  Please try again in a few minutes.");

                    default:
                        throw webExcp;
                }
            }
            else if (status == WebExceptionStatus.ProxyNameResolutionFailure)
            {
                throw new ProxyNotFoundException("The proxy server could not be found.  Check that it was entered correctly in the Options dialog.  You may need to disable your web proxy in the Options, if for instance you use a proxy server at work and are now at home.");
            }
            else throw webExcp;
        }

        #endregion
    }

    /// <summary>
    /// Enumeration of available Twitter timelines
    /// </summary>
    public enum Timeline
    {
        Public,
        Friends,
        User,
        Replies,
        DirectMessages,
        Favorite,
        Comments,
        Myself
    }
}
