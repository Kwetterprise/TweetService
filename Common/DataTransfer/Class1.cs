using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;

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

    public class Option
    {
        public Option()
        {

        }

        protected Option(string? error)
        {
            this.Error = error;
            this.HasFailed = error != null;
        }

        public string? Error { get; set; }

        public bool HasFailed { get; set; }

        public static Option Success => new Option(null);

        public static Option FromError(string error) => new Option(error);
    }

    public class Option<T>
        where T : class
    {
        public Option()
        {

        }

        private Option(T? value, string? error)
        {
            this.Value = value;
            this.Error = error;
            this.HasFailed = error != null;
        }

        public string? Error { get; set; }

        public bool HasFailed { get; set; }

        public T? Value { get; set; }

        public static Option<T> FromResult(T result) => new Option<T>(result, null);

        public static Option<T> FromError(string e) => new Option<T>(null, e);

        public Option<TOut> CastError<TOut>()
            where TOut : class
        {
            if (this.HasFailed)
            {
                return Option<TOut>.FromError(this.Error!);
            }

            throw new InvalidOperationException("Cannot cast Some option.");
        }

        public Option<TOut> Select<TOut>(Func<T, TOut> selector)
            where TOut : class
        {
            return this.HasFailed
                ? Option<TOut>.FromError(this.Error!)
                : Option<TOut>.FromResult(selector(this.Value!));
        }

        public static implicit operator Option<T>(T value) => Option<T>.FromResult(value);

        public static implicit operator Option(Option<T> op) =>
            op.HasFailed ? Option.FromError(op.Error!) : Option.Success;
    }

    public static class OptionExtensions
    {
        public static Option<T> OrElse<T>(this T? source, string error) where T : class =>
            source is null ? Option<T>.FromError(error) : Option<T>.FromResult(source);

        public static Option<T> ToErrorOption<T>(this string source) where T : class => Option<T>.FromError(source);
    }
}