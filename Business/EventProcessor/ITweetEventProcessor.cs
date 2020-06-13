using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kwetterprise.TweetService.Data;
using Kwetterprise.TweetService.Data.Entity;

namespace Business.EventProcessor
{
    using System.Threading.Tasks;
    using Data.Context;
    using Kwetterprise.EventSourcing.Client.Models.Event;
    using Kwetterprise.EventSourcing.Client.Models.Event.Account;
    using Kwetterprise.EventSourcing.Client.Models.Event.Tweet;
    using Microsoft.Extensions.Logging;

    public interface ITweetEventProcessor
    {
        Task Process(EventBase @event);
    }

    public class TweetEventProcessor : ITweetEventProcessor
    {
        private readonly ILogger<TweetEventProcessor> logger;
        private readonly ITweetContextFactory contextFactory;

        public TweetEventProcessor(ILogger<TweetEventProcessor> logger, ITweetContextFactory contextFactory)
        {
            this.logger = logger;
            this.contextFactory = contextFactory;
        }

        public Task Process(EventBase @event)
        {
            this.logger.LogInformation($"Processing: {@event.Type}.");

            switch (@event)
            {
                case AccountCreated accountCreated:
                    {
                        using var context = this.contextFactory.Create();

                        context.Accounts.Add(new AccountEntity(accountCreated.Id, accountCreated.Username, accountCreated.Role, accountCreated.ProfilePicture));
                        context.SaveChanges();
                        break;
                    }
                case AccountDeleted accountDeleted:
                    {
                        using var context = this.contextFactory.Create();

                        var account = context.Accounts.Single(x => x.Id == accountDeleted.AccountId);
                        context.Accounts.Remove(account);
                        context.SaveChanges();
                        break;
                    }
                case AccountRoleChanged accountRoleChanged:
                    {
                        using var context = this.contextFactory.Create();

                        var account = context.Accounts.Single(x => x.Id == accountRoleChanged.Target);
                        account.Role = accountRoleChanged.NewRole;
                        context.SaveChanges();
                        break;
                    }
                case AccountUpdated accountUpdated:
                    {
                        using var context = this.contextFactory.Create();

                        var account = context.Accounts.Single(x => x.Id == accountUpdated.Id);

                        account.Username = accountUpdated.NewUsername;
                        account.ProfilePicture = accountUpdated.NewProfilePicture;

                        context.SaveChanges();
                        break;
                    }
                case TweetDeleted tweetDeleted:
                    {
                        using var context = this.contextFactory.Create();

                        var tweet = context.Tweets.Single(x => x.Id == tweetDeleted.Id);
                        context.Tweets.Remove(tweet);
                        context.SaveChanges();
                        break;
                    }
                case TweetPosted tweetPosted:
                    {
                        using var context = this.contextFactory.Create();

                        context.Tweets.Add(tweetPosted.ToEntity());
                        context.SaveChanges();
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(@event));
            }

            return Task.CompletedTask;
        }
    }
}
