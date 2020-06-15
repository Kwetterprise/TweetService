using Microsoft.AspNetCore.Http;

namespace Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Business.Manager;
    using Kwetterprise.TweetService.Business;
    using Kwetterprise.TweetService.Common.DataTransfer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TweetCommandController : ControllerBase
    {
        private readonly ILogger<TweetCommandController> _logger;
        private readonly ITweetCommandManager tweetCommandManager;
        private readonly JwtManager jwtManager;

        public TweetCommandController(ILogger<TweetCommandController> logger, ITweetCommandManager tweetCommandManager, JwtManager jwtManager)
        {
            this._logger = logger;
            this.tweetCommandManager = tweetCommandManager;
            this.jwtManager = jwtManager;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Option<TweetDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Post(PostTweetRequest request)
        {
            var (id, _) = this.jwtManager.DeconstructClaims(this.HttpContext.User.Claims);
            if (id != request.Author)
            {
                return this.Unauthorized("The caller is not the author of this tweet.");
            }

            return this.Ok(await this.tweetCommandManager.Post(request));
        }

        [HttpDelete]
        [ProducesResponseType(typeof(Option), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(DeleteTweetRequest request)
        {
            var (id, _) = this.jwtManager.DeconstructClaims(this.HttpContext.User.Claims);
            if (request.Actor != id)
            {
                return this.Unauthorized("The supplied actor is different from the logged in user.");
            }

            return this.Ok(await this.tweetCommandManager.Delete(request));
        }
    }
}
