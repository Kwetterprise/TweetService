namespace Kwetterprise.TweetService.Common.DataTransfer
{
    public class Tweet
    {
        public User Author { get; set; } = null!;
    }

    public class User
    {
        public int Id { get; set; }

        public string DisplayName { get; set; } = null!;
    }
}
