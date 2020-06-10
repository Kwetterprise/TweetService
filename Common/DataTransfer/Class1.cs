namespace Kwetterprise.TweetService.Common.DataTransfer
{
    using System;
    using Kwetterprise.EventSourcing.Client.Models.DataTransfer;

    public class PostTweetRequest
    {
        public PostTweetRequest()
        {
            
        }

        public PostTweetRequest(Guid author, string content, Guid? parent)
        {
            this.Author = author;
            this.Content = content;
            this.Parent = parent;
        }

        public Guid Author { get; set; }

        public string Content { get; set; } = null!;

        public Guid? Parent { get; set; }
    }

    public class DeleteTweetRequest
    {
        public DeleteTweetRequest()
        {
            
        }

        public DeleteTweetRequest(Guid tweet, Guid actor)
        {
            this.Tweet = tweet;
            this.Actor = actor;
        }

        public Guid Tweet { get; set; }

        public Guid Actor { get; set; }
    }

    public class TweetDto
    {
        public TweetDto()
        {
            
        }

        public TweetDto(Guid id, AccountDto author, string content, Guid? parentTweet)
        {
            this.Id = id;
            this.Author = author;
            this.Content = content;
            this.ParentTweet = parentTweet;
        }

        public Guid Id { get; set; }

        public AccountDto Author { get; set; } = null!;

        public string Content { get; set; } = null!;

        public Guid? ParentTweet { get; }
    }

    public class AccountDto
    {
        public AccountDto()
        {

        }

        public AccountDto(Guid id, string username, AccountRole role, byte[]? profilePicture)
        {
            this.Id = id;
            this.Username = username;
            this.Role = role;
            this.ProfilePicture = profilePicture;
        }

        public Guid Id { get; set; }

        public string Username { get; set; } = null!;

        public AccountRole Role { get; }

        public byte[]? ProfilePicture { get; set; }
    }
}
