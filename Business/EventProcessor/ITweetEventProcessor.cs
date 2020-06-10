using System;
using System.Collections.Generic;
using System.Text;

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
        private ILogger<TweetEventProcessor> logger;
        private ITweetContextFactory contextFactory;

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

                    break;
                }
                case AccountDeleted accountDeleted:
                    break;
                case AccountRoleChanged accountRoleChanged:
                    break;
                case AccountUpdated accountUpdated:
                    break;
                case TweetDeleted tweetDeleted:
                    break;
                case TweetPosted tweetPosted:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@event));
            }

            return Task.CompletedTask;
        }
    }
}
