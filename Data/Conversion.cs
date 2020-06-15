using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Data.Entity;
using Kwetterprise.EventSourcing.Client.Models.Event.Tweet;
using Kwetterprise.TweetService.Common.DataTransfer;
using Kwetterprise.TweetService.Data.Entity;

namespace Kwetterprise.TweetService.Data
{
    public static class Conversion
    {
        public static TweetDto ToDto(this TweetEntity entity, AccountDto dto)
        {
            return new TweetDto(entity.Id, dto, entity.Content, entity.ParentTweet, entity.PostedOn);
        }

        public static AccountDto ToDto(this AccountEntity entity)
        {
            return new AccountDto(entity.Id, entity.Username, entity.Role, entity.ProfilePicture);
        }

        public static TweetDto ToDto(this TweetPosted @event, AccountDto account)
        {
            return new TweetDto(@event.Id, account, @event.Content, @event.ParentTweet, @event.PostedOn);
        }

        public static TweetEntity ToEntity(this TweetPosted posted)
        {
            return new TweetEntity(posted.Id, posted.Author, posted.Content, posted.PostedOn, posted.ParentTweet);
        }
    }
}
