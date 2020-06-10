using System;
using System.Collections.Generic;
using System.Text;

namespace Kwetterprise.TweetService.Business
{
    public static class Extensions
    {
        public static T ThrowIfNull<T>(this T? source, Func<Exception> exceptionFactory)
            where T : class
        {
            return source ?? throw exceptionFactory();
        }
    }
}
