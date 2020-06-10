using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Context
{
    using Data.Entity;
    using Kwetterprise.TweetService.Data.Entity;
    using Microsoft.EntityFrameworkCore;

    public interface ITweetContextFactory
    {
        ITweetContext Create();
    }

    public interface ITweetContext : IDisposable
    {
        DbSet<TweetEntity> Tweets { get; }
        DbSet<AccountEntity> Accounts { get; }

        int SaveChanges();
    }

    public class TweetContextFactory : ITweetContextFactory
    {
        private readonly DbContextOptions options;

        public TweetContextFactory(DbContextOptions options)
        {
            this.options = options;
        }

        public ITweetContext Create()
        {
            return new TweetContext(this.options);
        }
    }

    public class TweetContext : DbContext, ITweetContext
    {
        public TweetContext(DbContextOptions options)
            : base(options)
        {
            this.Tweets = this.Set<TweetEntity>();
            this.Accounts = this.Set<AccountEntity>();
        }

        public DbSet<TweetEntity> Tweets { get; }

        public DbSet<AccountEntity> Accounts { get; }
    }
}
