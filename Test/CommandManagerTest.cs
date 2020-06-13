using System;
using System.Threading.Tasks;
using Data.Context;
using Kwetterprise.EventSourcing.Client.Models.DataTransfer;
using Kwetterprise.EventSourcing.Client.Models.Event.Account;
using Kwetterprise.EventSourcing.Client.Models.Event.Tweet;
using Kwetterprise.TweetService.Common.DataTransfer;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces;
using Xunit;

namespace Test
{
    using Business;
    using Business.EventProcessor;
    using Business.Manager;
    using Kwetterprise.TweetService.Business;
    using Test.Mock;

    public class CommandManagerTest : TestBase
    {
        private MockEventManager mockEventManager = new MockEventManager();
        private TweetCommandManager tweetCommandManager;
        private TweetEventProcessor tweetEventProcessor;
        private ITweetContextFactory contextFactory;

        public CommandManagerTest()
        {
            this.contextFactory = this.CreateInMemoryContextFactory();

            this.tweetCommandManager = new TweetCommandManager(contextFactory, this.mockEventManager);
            this.tweetEventProcessor = new TweetEventProcessor(this.CreateLogger<TweetEventProcessor>(), contextFactory);
        }

        [Fact]
        public async Task PostTweetWithUpdatedAccount()
        {
            var user = new AccountCreated(Guid.NewGuid(), "SomeUsername", "unused", "unused", DateTime.Now, AccountRole.User, string.Empty, null);

            await this.tweetEventProcessor.Process(user);

            var newProfilePicture = new byte[] { 1, 2, 3, 4, };
            var userChanged = new AccountUpdated(user.Id, "NewUsername", "unused", "unused", newProfilePicture, "My new bio.");

            await this.tweetEventProcessor.Process(userChanged);

            var roleChanged = new AccountRoleChanged(user.Id, user.Id, AccountRole.Administrator);

            await this.tweetEventProcessor.Process(roleChanged);

            var tweetRequest = new PostTweetRequest(user.Id, "Some content", null);

            var eventAwaitable = this.mockEventManager.WaitOne<TweetPosted>(TimeSpan.FromSeconds(1));

            var tweetDto = await this.tweetCommandManager.Post(tweetRequest);

            Assert.False(tweetDto.HasFailed);
            Assert.Equal(tweetRequest.Content, tweetDto.Value!.Content);
            Assert.Null(tweetDto.Value.ParentTweet);
            Assert.Equal(roleChanged.NewRole, tweetDto.Value!.Author.Role);
            Assert.Equal(userChanged.NewUsername, tweetDto.Value!.Author.Username);
            Assert.Equal(newProfilePicture, tweetDto.Value!.Author.ProfilePicture);

            var @event = await eventAwaitable;

            Assert.Equal(tweetDto.Value!.Id, @event.Id);
            Assert.Equal(tweetDto.Value!.Content, @event.Content);
            Assert.Equal(user.Id, @event.Author);
            Assert.Null(@event.ParentTweet);
        }
    }
}
