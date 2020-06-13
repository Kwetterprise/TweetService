using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.Context;
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
        Option<List<TweetDto>> GetFromUser(Guid user, int pageSize, int pageNumber);

        Option<List<TweetDto>> GetForFriends(Guid user, int pageSize, int pageNumber);
    }

    public class TweetQueryManager : ITweetQueryManager
    {
        private readonly ITweetContextFactory contextFactory;

        public TweetQueryManager(ITweetContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public Option<List<TweetDto>> GetFromUser(Guid user, int pageSize, int pageNumber)
        {
            using var context = this.contextFactory.Create();

            var account = context.Accounts.Single(x => x.Id == user).OrElse($"No user with id {user} found.");
            if (account.HasFailed)
            {
                return account.CastError<List<TweetDto>>();
            }

            return context.Tweets
                .OrderByDescending(x => x.PostedOn)
                .Skip(pageNumber * pageSize).Take(pageSize)
                .AsEnumerable()
                .Select(x => x.ToDto(account.Value!.ToDto()))
                .ToList();
        }

        public Option<List<TweetDto>> GetForFriends(Guid user, int pageSize, int pageNumber)
        {
            return Option<List<TweetDto>>.FromError("You have no friends.");
        }
    }
}
