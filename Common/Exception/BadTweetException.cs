using System;
using System.Collections.Generic;
using System.Text;

namespace Kwetterprise.TweetService.Common.Exception
{
    using Exception = System.Exception;

    public class BadTweetException : Exception
    {
        public BadTweetException(string message) : base(message)
        {

        }
    }
}
