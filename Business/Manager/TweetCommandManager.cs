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

        public async Task<Option<TweetDto>> Post(PostTweetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new BadTweetException("The content cannot be whitespace.");
            }

            if (request.Content.Trim().Length > TweetEntity.MaxLength)
            {
                return Option<TweetDto>.FromError($"The content cannot be more than {TweetEntity.MaxLength} characters.");
            }

            using var context = this.contextFactory.Create();

            var author = context.AccountById(request.Author).Select(x => x.ToDto());
            if (author.HasFailed)
            {
                return author.CastError<TweetDto>();
            }

            var tweetCreated = new TweetPosted(Guid.NewGuid(), request.Author, request.Content.Trim(), request.Parent, DateTime.UtcNow);

            await this.eventPublisher.Publish(tweetCreated, Topic.Tweet);

            return tweetCreated.ToDto(author.Value!);
        }

        public async Task<Option> Delete(DeleteTweetRequest request)
        {
            using var context = this.contextFactory.Create();

            var tweet = context.TweetById(request.Tweet);
            if (tweet.HasFailed)
            {
                return tweet;
            }

            if (tweet.Value!.Author != request.Actor)
            {
                var author = context.AccountById(request.Actor).Select(x => x.ToDto());
                if (author.HasFailed)
                {
                    return author.CastError<TweetDto>();
                }

                if (author.Value!.Role == AccountRole.User)
                {
                    return Option.FromError("Cannot delete someone else's tweet if you are not moderator or administrator.");
                }
            }

            var tweetDeleted = new TweetDeleted(request.Tweet, request.Actor);

            await this.eventPublisher.Publish(tweetDeleted, Topic.Tweet);
            return Option.Success;
        }
    }
}