using System;
using System.Linq;
using System.Runtime.InteropServices;
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

    public class QueryManagerTest : TestBase
    {
        private MockEventManager mockEventManager = new MockEventManager();
        private TweetQueryManager tweetQueryManager;
        private TweetEventProcessor tweetEventProcessor;
        private ITweetContextFactory contextFactory;

        public QueryManagerTest()
        {
            this.contextFactory = this.CreateInMemoryContextFactory();

            this.tweetQueryManager = new TweetQueryManager(contextFactory);
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

            var tweetPosted = new TweetPosted(Guid.NewGuid(), user.Id, GetRandomAlphaString(30), null, DateTime.UtcNow);

            await this.tweetEventProcessor.Process(tweetPosted);

            var tweetPosted2 = new TweetPosted(Guid.NewGuid(), user.Id, GetRandomAlphaString(30), null, DateTime.UtcNow.AddMinutes(1));

            await this.tweetEventProcessor.Process(tweetPosted2);

            var tweetPosted3 = new TweetPosted(Guid.NewGuid(), user.Id, GetRandomAlphaString(30), null, DateTime.UtcNow.AddMinutes(2));

            await this.tweetEventProcessor.Process(tweetPosted3);

            var timedData = this.tweetQueryManager.GetFromUser(user.Id, null, false, 2).Value!;

            Assert.NotNull(timedData.Next);
            Assert.Equal(2, timedData.Data.Count);
            var firstTweet = timedData.Data.First();
            var secondTweet = timedData.Data.Skip(1).Single();

            Assert.Equal(tweetPosted3.Id, firstTweet.Id);
            Assert.Equal(tweetPosted2.Id, secondTweet.Id);
            Assert.Equal(tweetPosted3.Content, firstTweet.Content);
            Assert.Null(firstTweet.ParentTweet);
            Assert.Equal(roleChanged.NewRole, firstTweet.Author.Role);
            Assert.Equal(userChanged.NewUsername, firstTweet.Author.Username);
            Assert.Equal(newProfilePicture, firstTweet.Author.ProfilePicture);

            var timedData2 = this.tweetQueryManager.GetFromUser(user.Id, timedData.Next, false, 2).Value!;

            Assert.Null(timedData2.Next);
            Assert.Single(timedData2.Data);
        }
    }
}
