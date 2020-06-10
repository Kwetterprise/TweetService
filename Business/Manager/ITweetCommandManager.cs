using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Manager
{
    using System.Threading.Tasks;
    using Kwetterprise.TweetService.Common.DataTransfer;

    public interface ITweetCommandManager
    {
        Task<TweetDto> Post(PostTweetRequest request);

        Task Delete(DeleteTweetRequest request);
    }

    public interface ITweetQueryManager
    {
        List<TweetDto> GetFromUser(Guid user, int pageSize, int pageNumber);

        List<TweetDto> GetForFriends(Guid user, int pageSize, int pageNumber);
    }

    public class TweetQueryManager : ITweetQueryManager
    {
        public List<TweetDto> GetFromUser(Guid user, int pageSize, int pageNumber)
        {
            throw new NotImplementedException();
        }

        public List<TweetDto> GetForFriends(Guid user, int pageSize, int pageNumber)
        {
            throw new NotImplementedException();
        }
    }
}
