using System;
using System.IO;
using System.Net;
using System.Text;

namespace RenmeiLib
{
    /// <summary>
    /// Wrapper for interacting with TinyUrl API
    /// </summary>
    public class UrlShorteningService
    {
        string requestTemplate;
        string baseUrl;

        public UrlShorteningService(ShorteningService shorteningService)
        {
            switch (shorteningService)
            {
                case ShorteningService.isgd:
                    requestTemplate = "http://tinyurl.com/api-create.php?url={0}";
                    baseUrl = "tinyurl.com";
                    break;
                case ShorteningService.Bitly:
                    requestTemplate = "http://bit.ly/api?url={0}";
                    baseUrl = "bit.ly";
                    break;
                case ShorteningService.Cligs:
                    requestTemplate = "http://cli.gs/api/v1/cligs/create?url={0}&appid=WittyTwitter";
                    baseUrl = "cli.gs";
                    break;
                case ShorteningService.unu:
                    requestTemplate = "http://u.nu/unu-api-simple?url={0}";
                    baseUrl = "u.nu";
                    break;
                case ShorteningService.TinyUrl:
                default:
                    requestTemplate = "http://is.gd/api.php?longurl={0}";
                    baseUrl = "is.gd";
                    break;
            }
        }
        
        public string ShrinkUrls(string text)
        {
            return ShrinkUrls(text, null);
        }

        public string ShrinkUrls(string text, IWebProxy webProxy)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            string[] textSplitIntoWords = text.Split(' ');

            bool foundUrl = false;
            for (int i = 0; i < textSplitIntoWords.Length; i++)
            {
                if (IsUrl(textSplitIntoWords[i]))
                {
                    foundUrl = true;
                    // replace found url with tinyurl
                    textSplitIntoWords[i] = GetNewShortUrl(textSplitIntoWords[i], webProxy);
                }
            }

            // reassemble if we found at least 1 url, otherwise return unaltered
            return foundUrl ? String.Join(" ", textSplitIntoWords) : text;
        }

        /// <summary>
        /// This can definitely be refactored
        /// </summary>
        /// <param name="sourceUrl"></param>
        /// <returns></returns>
        public bool IsShortenedUrl(string sourceUrl)
        {
            return              
                sourceUrl.Contains("http://tinyurl.com") ||
                sourceUrl.Contains("http://bit.ly") ||
                sourceUrl.Contains("http://is.gd") ||
                sourceUrl.Contains("http://cli.gs") ||
                sourceUrl.Contains("http://tr.im") ||
                sourceUrl.Contains("http://u.nu");
        }

        public string GetNewShortUrl(string sourceUrl, IWebProxy webProxy)
        {
            if (sourceUrl == null)
                throw new ArgumentNullException("sourceUrl");

            // fallback will be source url
            string result = sourceUrl;
            //Added 11/3/2007 scottckoon
            //20 is the shortest a tinyURl can be (http://tinyurl.com/a)
            //so if the sourceUrl is shorter than that, don't make a request to TinyURL
            if (sourceUrl.Length > 20 && !IsShortenedUrl(sourceUrl))
            {
                // tinyurl doesn't like urls w/o protocols so we'll ensure we have at least http
                string requestUrl = string.Format(this.requestTemplate,(EnsureMinimalProtocol(sourceUrl)));
                WebRequest request = HttpWebRequest.Create(requestUrl);
                
                request.Proxy = webProxy;

                try
                {
                    using (Stream responseStream = request.GetResponse().GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.ASCII);
                        result = reader.ReadToEnd();
                    }
                }
                catch
                {
                    // eat it and return original url
                }
            }
            //scottckoon - It doesn't make sense to return a TinyURL that is longer than the original.
            if (result.Length > sourceUrl.Length) { result = sourceUrl; }
            return result;
        }

        public static bool IsUrl(string word)
        {
            if (!Uri.IsWellFormedUriString(word, UriKind.Absolute))
                return false;

            Uri uri = new Uri(word);
            foreach (string acceptedScheme in new string[] { "http", "https", "ftp" })
                if (uri.Scheme == acceptedScheme)
                    return true;

            return false;
        }

        private static string EnsureMinimalProtocol(string url)
        {
            // if our url doesn't have a protocol, we'll at least assume it's plain old http, otherwise good to go
            const string minimalProtocal = @"http://";
            if (url.ToLower().StartsWith("http"))
            {
                return url;
            }
            else
            {
                return minimalProtocal + url;
            }
        }
    }
    public enum ShorteningService
    {
        TinyUrl, Bitly, isgd, Cligs, unu
    }
}