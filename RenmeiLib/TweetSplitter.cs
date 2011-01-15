using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RenmeiLib
{
    public static class TweetSplitter
    {
        private const int MaxCharacters = 140;
        private const string StatusDelimiter = "...";
        private const string RegexGroup = "MsgAddressInfo";

        private static readonly Regex StripWhiteSpace;
        private static readonly Regex ReplyAddress;
        private static readonly Regex DirectMessageAddress;

        static TweetSplitter()
        {
            StripWhiteSpace = new Regex(@"\s+", RegexOptions.Compiled);
            ReplyAddress = new Regex(@"(?<" + RegexGroup + @">^@\w+).*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            DirectMessageAddress = new Regex(@"(?<" + RegexGroup + @">^D\s+\w+).*",
                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Split the original status into separate parts while ensuring each part is less than or equal to 140 characters,
        /// words are not split, parts are linked using a delimiter (...), and reply and direct
        /// message addresses are maintained on each part. May need to consider other special recipient/message types.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string[] SplitTweet(string status)
        {
            status = RemoveWhiteSpace(status);

            var statuses = new List<string>();
            
            // Recipient address include x @usernames or 1 D username.
            string recipientAddresses = GetRecipients(status, out status);

            // Status length varies based on recipients string and length of delimiter (...) linking tweets.
            int maxStatusLength = GetMaxStatusLength(recipientAddresses);

            while (status.Length != 0)
            {
                // Move through the full status string adding parts to the statuses array.
                statuses.Add(GetNextPart(status, maxStatusLength, recipientAddresses, statuses.Count, out status));
            }
            
            return statuses.ToArray();
        }

        private static string RemoveWhiteSpace(string status)
        {
            return StripWhiteSpace.Replace(status.Trim(), " ");
        }

        private static string GetRecipients(string status, out string statusWithoutRecipients)
        {
            string value = string.Empty;
            string prefix = string.Empty;

            statusWithoutRecipients = status;

            string delimiter = "";
           
            while (GetMatch(ReplyAddress, statusWithoutRecipients, out value))
            {
                statusWithoutRecipients = statusWithoutRecipients.Length == value.Length ? "" : statusWithoutRecipients.Substring(value.Length + 1);
                prefix += delimiter + value;
                delimiter = " ";
            }

            if (GetMatch(DirectMessageAddress, statusWithoutRecipients, out value))
            {
                statusWithoutRecipients = statusWithoutRecipients.Length == value.Length ? "" : statusWithoutRecipients.Substring(value.Length + 1);
                prefix = value;
            }

            return prefix;
        }

        private static int GetMaxStatusLength(string recipientAddresses)
        {
            int statusLength = MaxCharacters;

            if (recipientAddresses.Length > 0)
            {
                statusLength -= recipientAddresses.Length + " ".Length;
            }

            return statusLength;
        }

        private static string GetNextPart(string status, int maxAllowableLength, string recipientAddresses, int partIndex, out string updatedStatus)
        {
            string part = string.Empty; 

            if (partIndex > 0)
            {
                status = StatusDelimiter + status; // ... to start for link to previous part.
            }

            if (status.Length > maxAllowableLength)
            {
                maxAllowableLength -= StatusDelimiter.Length; // ... will be appended to end of part.

                // Walk the string backwards looking for the index of the word 
                // ending closest to the allowable length
                while (status.Substring(maxAllowableLength, 1) != " ")
                {
                    maxAllowableLength--;
                }

                part = status.Substring(0, maxAllowableLength) + StatusDelimiter; // ... to end to link to next part.
            }
            else
            {
                part = status;
                maxAllowableLength = status.Length;
            }

            if (recipientAddresses.Length > 0)
            {
                // Reapply the @username(s) or direct message text
                part = recipientAddresses + " " + part;
            }

            // Return the status sans the current part.
            updatedStatus = status.Remove(0, maxAllowableLength).Trim(); 
            
            return part;
        }
        
        private static bool GetMatch(Regex regex, string text, out string value)
        {
            value = string.Empty;

            Match m = regex.Match(text);
            if (m.Success)
            {
                value = m.Groups[RegexGroup].Value;
            }

            return m.Success;
        }
    }
}