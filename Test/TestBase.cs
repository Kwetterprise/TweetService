namespace Test
{
    using System;
    using System.Linq;
    using Data.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Test.Mock;

    public abstract class TestBase
    {
        private static readonly Random random = new Random();

        protected MockEventManager MockEventManager { get; } = new MockEventManager();

        // public (string Password, string HashedPassword) PasswordTuple = ("SomePassword", "$2a$15$JNeDCqp7YZ0005eQhmNm6e2CzPYqodM2apgQm/Et9fQN/XsHbEztu");

        protected ITweetContextFactory CreateInMemoryContextFactory()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("Tweet");

            return new TweetContextFactory(optionsBuilder.Options);
        }

        protected ILogger<T> CreateLogger<T>()
        {
            return new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<T>();
        }

        protected static string GetRandomAlphaString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(
                Enumerable.Repeat(chars, length).Select(s => s[TestBase.random.Next(s.Length)]).ToArray());
        }
    }
}