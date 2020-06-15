using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Data.Context;
using Data.Entity;
using Kwetterprise.TweetService.Data;

namespace Business.Manager
{
    using System.Threading.Tasks;
    using Kwetterprise.TweetService.Common.DataTransfer;

    public interface ITweetCommandManager
    {
        Task<Option<TweetDto>> Post(PostTweetRequest request);

        Task<Option> Delete(DeleteTweetRequest request);
    }

    public interface ITweetQueryManager
    {
        Option<TimedData<TweetDto>> GetFromUser(Guid user, Guid? from, bool ascending, int count);

        Option<TimedData<TweetDto>> GetForFriends(Guid user, Guid? from, bool ascending, int count);
    }

    public class TweetQueryManager : ITweetQueryManager
    {
        private readonly ITweetContextFactory contextFactory;

        public TweetQueryManager(ITweetContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public Option<TimedData<TweetDto>> GetFromUser(Guid user, Guid? from, bool ascending, int count)
        {
            using var context = this.contextFactory.Create();

            var account = context.Accounts.Single(x => x.Id == user).OrElse($"No user with id {user} found.");
            if (account.HasFailed)
            {
                return account.CastError<TimedData<TweetDto>>();
            }

            var totalCount = context.Tweets.Count(x => x.Author == user);

            var query = context.Tweets
                .Where(x => x.Author == user);

            if (ascending)
            {
                query = query.OrderBy(x => x.PostedOn);
            }
            else
            {
                query = query.OrderByDescending(x => x.PostedOn);
            }

            var entities = query.AsEnumerable();
            if (from.HasValue)
            {
                entities = entities.SkipWhile(x => x.Id != from);
            }
            
            var data = entities
                .Take(count + 1)
                .AsEnumerable()
                .Select(x => x.ToDto(account.Value!.ToDto()))
                .ToList();

            Guid? next = null;
            if (data.Count == count + 1 && data.Count > 1)
            {
                var last = data.Last();
                next = last.Id;
                data.Remove(last);
            }
            return Option<TimedData<TweetDto>>.FromResult(new TimedData<TweetDto>(data, ascending, next));
        }

        public Option<TimedData<TweetDto>> GetForFriends(Guid user, Guid? from, bool ascending, int count)
        {
            return Option<TimedData<TweetDto>>.FromError("You have no friends.");
        }
    }
}
