using System;
using System.Collections.Generic;
using System.Text;

namespace Kwetterprise.TweetService.Data
{
    using Kwetterprise.EventSourcing.Client.Models.Event.Tweet;
    using Kwetterprise.TweetService.Common.DataTransfer;
    using Kwetterprise.TweetService.Data.Entity;

    public static class Extensions
    {
        public static TweetDto ToDto(this TweetPosted @event, AccountEntity account)
        {
            return new TweetDto(@event.Id, account.ToDto(), @event.Content, @event.ParentTweet);
        }

        public static AccountDto ToDto(this AccountEntity entity)
        {
            return new AccountDto(entity.Id, entity.Username, entity.Role, entity.ProfilePicture);
        }
    }
}
