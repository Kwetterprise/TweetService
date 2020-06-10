namespace Business.Manager
{
    using System;
    using System.Linq;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Data.Context;
    using Data.Entity;
    using Kwetterprise.EventSourcing.Client.Interface;
    using Kwetterprise.EventSourcing.Client.Models.DataTransfer;
    using Kwetterprise.EventSourcing.Client.Models.Event;
    using Kwetterprise.EventSourcing.Client.Models.Event.Tweet;
    using Kwetterprise.TweetService.Business;
    using Kwetterprise.TweetService.Common.DataTransfer;
    using Kwetterprise.TweetService.Common.Exception;
    using Kwetterprise.TweetService.Data;

    public class TweetCommandManager : ITweetCommandManager
    {
        private ITweetContextFactory contextFactory;
        private readonly IEventPublisher eventPublisher;

        public TweetCommandManager(ITweetContextFactory contextFactory, IEventPublisher eventPublisher)
        {
            this.contextFactory = contextFactory;
            this.eventPublisher = eventPublisher;
        }

        public async Task<TweetDto> Post(PostTweetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new BadTweetException("The content cannot be whitespace.");
            }

            if (request.Content.Trim().Length > TweetEntity.MaxLength)
            {
                throw new BadTweetException($"The content cannot be more than {TweetEntity.MaxLength} characters.");
            }

            using var context = this.contextFactory.Create();

            var author = context.Accounts.SingleOrDefault(x => x.Id == request.Author)
                .ThrowIfNull(() => new DoesNotExistException("User", request.Author));

            var tweetCreated = new TweetPosted(Guid.NewGuid(), request.Author, request.Content.Trim(), request.Parent);

            await this.eventPublisher.Publish(tweetCreated, Topic.Tweet);

            return tweetCreated.ToDto(author);
        }

        public async Task Delete(DeleteTweetRequest request)
        {
            using var context = this.contextFactory.Create();

            var tweet = context.Tweets.SingleOrDefault(x => x.Id == request.Tweet)
                .ThrowIfNull(() => new DoesNotExistException("Tweet", request.Tweet));

            if (tweet.Author != request.Actor)
            {
                var author = context.Accounts.SingleOrDefault(x => x.Id == request.Actor)
                    .ThrowIfNull(() => new DoesNotExistException("User", request.Actor));

                if (author.Role == AccountRole.User)
                {
                    throw new AuthenticationException("Cannot delete someone else's tweet if you are not moderator or administrator.");
                }
            }

            var tweetDeleted = new TweetDeleted(request.Tweet, request.Actor);

            await this.eventPublisher.Publish(tweetDeleted, Topic.Tweet);
        }
    }
}