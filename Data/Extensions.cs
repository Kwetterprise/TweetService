using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.Context;
using Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Kwetterprise.TweetService.Data
{
    using Kwetterprise.EventSourcing.Client.Models.Event.Tweet;
    using Kwetterprise.TweetService.Common.DataTransfer;
    using Kwetterprise.TweetService.Data.Entity;

    public static class Extensions
    {
        public static Option<AccountEntity> AccountById(this ITweetContext context, Guid id)
        {
            return context.Accounts.SingleOrDefault(x => x.Id == id).OrElse($"No user with id {id} found.");
        }

        public static Option<TweetEntity> TweetById(this ITweetContext context, Guid id)
        {
            return context.Tweets.SingleOrDefault(x => x.Id == id).OrElse($"No tweet with id {id} exists.");
        }
    }
}
