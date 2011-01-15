using System;
using System.Collections.Generic;
using System.Text;

namespace RenmeiLib.Utilities
{

    public enum UserBehavior
    {
        NeverAlert,
        Ignore,
        AlwaysAlert,
        Default
    }

    public class UserBehaviorManager
    {
        private readonly Dictionary<string, UserBehavior> behaviors = new Dictionary<string, UserBehavior>();

        public UserBehaviorManager()
        {
        }

        public UserBehaviorManager(string serializedInfo)
        {
            Deserialize(serializedInfo);
        }

        public void Deserialize(string info)
        {
            if (string.IsNullOrEmpty(info)) return;

            behaviors.Clear();

            foreach (var behaviorString in info.Split(','))
            {
                var keys = behaviorString.Split(':');
                // love to make this an ext method when we upgrade to 3.5+
                behaviors.Add(keys[0], (UserBehavior)Enum.Parse(typeof(UserBehavior), keys[1]));
            }
        }


        public void AddBehavior(string userName, UserBehavior behavior)
        {
            if (behaviors.ContainsKey(userName))
                behaviors[userName] = behavior;
            else
                behaviors.Add(userName, behavior);
        }

        public UserBehavior GetBehavior(string userName)
        {
            if (!behaviors.ContainsKey(userName)) 
                return UserBehavior.Default;

            return behaviors[userName];
        }

        public bool HasBehavior(string userName, UserBehavior behavior)
        {
            return GetBehavior(userName) == behavior;
        }

        public string Serialize()
        {
            if (behaviors.Count == 0)
                return String.Empty;

            StringBuilder builder = new StringBuilder();

            foreach (var keyValuePair in behaviors)
            {
                if (builder.Length > 0) builder.Append(",");

                builder.Append(String.Format("{0}:{1}",
                               keyValuePair.Key,
                               keyValuePair.Value));
            }

            return builder.ToString();
        }


        public void RemoveBehavior(string userName)
        {
            behaviors.Remove(userName);
        }
    }

}
