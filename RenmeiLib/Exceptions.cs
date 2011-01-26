using System;
using System.Runtime.Serialization;

namespace RenmeiLib
{
    /// <summary>
    /// Custom Exception when the per hour rate limit for authenticated Twitter API calls have been hit.
    /// </summary>
    [Serializable]
    public class RateLimitException : Exception
    {
        public RateLimitException()
        {
        }

        public RateLimitException(string message) : base(message)
        {
        }

        public RateLimitException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RateLimitException(SerializationInfo info, StreamingContext context) : base(info, context) 
        { 
        }
    }

    /// <summary>
    /// Custom Exception when a specific user id is not recognized by Twitter.
    /// </summary>
    [Serializable]
    public class UserNotFoundException : Exception
    {
        private string _userId;

        public UserNotFoundException()
        {
        }

        public UserNotFoundException(string userId)
        {
            _userId = userId;
        }

        public UserNotFoundException(string userId, string message)
            : base(message)
        {
            _userId = userId;
        }

        public UserNotFoundException(string userId, string message, Exception inner)
            : base(message, inner)
        {
            _userId = userId;
        }

        protected UserNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                if (_userId == value)
                    return;
                _userId = value;
            }
        }
        
    }
    [Serializable]
    public class ProxyAuthenticationRequiredException : Exception
    { 
        public ProxyAuthenticationRequiredException()
        {
        }

        public ProxyAuthenticationRequiredException(string message) : base(message)
        {
        }

        public ProxyAuthenticationRequiredException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ProxyAuthenticationRequiredException(SerializationInfo info, StreamingContext context) : base(info, context) 
        { 
        }
    }

    [Serializable]
    public class ProxyNotFoundException : Exception
    {
        public ProxyNotFoundException()
        {
        }

        public ProxyNotFoundException(string message)
            : base(message)
        {
        }

        public ProxyNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ProxyNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Custom exception for a timeout
    /// </summary>
    [Serializable]
    public class RequestTimeoutException : Exception
    {
        public RequestTimeoutException() { }

        public RequestTimeoutException(string message) : base(message) {}

        public RequestTimeoutException(string message, Exception inner) : base(message, inner) { }

        protected RequestTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Custom exception for when the Twitter API is freaking out.  ZOMG, FAIL WHALE!
    /// </summary>
    [Serializable]
    public class BadGatewayException : Exception
    {
        public BadGatewayException() {}

        public BadGatewayException(string message) : base(message) { }

        public BadGatewayException(string message, Exception inner) : base(message, inner) { }

        protected BadGatewayException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NoPermissionException : Exception
    {
        public NoPermissionException() {}

        public NoPermissionException(string message) : base(message) { }

        public NoPermissionException(string message, Exception inner) : base(message, inner) { }

        protected NoPermissionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
