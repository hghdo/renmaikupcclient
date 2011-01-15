using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;

namespace RenmeiLib.Utilities
{
    public class TweetScanHelper
    {
        public TweetCollection GetSearchResults(string searchText)
        {
            return GetSearchResults(searchText, null);
        }

        public TweetCollection GetSearchResults(string searchText, IWebProxy webProxy)
        {
            TweetCollection tweets = new TweetCollection();

            string tweetscanUrl = "http://tweetscan.com/trss.php?s=" + searchText;

            HttpWebRequest request = WebRequest.Create(tweetscanUrl) as HttpWebRequest;

            // Add configured web proxy
            request.Proxy = webProxy;

            //try
            //{
                // Get the Web Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);

                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/rss/channel/item");

                    foreach (XmlNode node in nodes)
                    {
                        Tweet tweet = new Tweet();
                        tweet.Id = double.Parse(node.SelectSingleNode("tweetid").InnerText);
                        tweet.Text = HttpUtility.HtmlDecode(node.SelectSingleNode("text").InnerText);

                        string dateString = node.SelectSingleNode("pubdate").InnerText;
                        if (!string.IsNullOrEmpty(dateString))
                        {
                            tweet.DateCreated = DateTime.Parse(dateString);
                        }

                        User user = new User();

                        user.Name = node.SelectSingleNode("username").InnerText;
                        user.ScreenName = node.SelectSingleNode("screenname").InnerText;
                        user.ImageUrl = node.SelectSingleNode("image").InnerText;

                        tweet.User = user;

                        tweets.Add(tweet);
                    }

                    tweets.SaveToDisk();
                }
            //}
            //catch { 
            ////TODO: not sure what kind of errors are thrown by tweetcan
            //    // eat it.
            //}

            return tweets;
        }
    }
}
