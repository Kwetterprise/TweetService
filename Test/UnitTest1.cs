using Xunit;

namespace Test
{
    using Business;
    using Business.EventProcessor;
    using Business.Manager;
    using Kwetterprise.TweetService.Business;
    using Test.Mock;

    public class UnitTest1 : TestBase
    {
        private MockEventManager mockEventManager = new MockEventManager();
        private TweetEventProcessor tweetEventProcessor;
        private TweetCommandManager tweetCommandManager;
        private TweetQueryManager tweetQueryManager;

        [Fact]
        public void Test1()
        {
            var contextFactory = this.CreateInMemoryContextFactory();

            this.tweetEventProcessor = new TweetEventProcessor(this.CreateLogger<TweetEventProcessor>(), contextFactory);
            this.tweetQueryManager = new TweetQueryManager();
            this.tweetCommandManager = new TweetCommandManager(contextFactory, this.mockEventManager);
        }
    }
}
