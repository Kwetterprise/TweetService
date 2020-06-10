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
        [Route("Post")]
        public async Task<IActionResult> Post(PostTweetRequest request)
        {
            return this.Ok(await this.tweetCommandManager.Post(request));
        }

        public async Task<IActionResult> Delete(DeleteTweetRequest request)
        {
            var (id, _) = this.jwtManager.DeconstructClaims(this.HttpContext.User.Claims);
            if (request.Actor != id)
            {
                return this.BadRequest("The supplied actor is different from the logged in user.");
            }

            await this.tweetCommandManager.Delete(request);
            return this.Ok();
        }
    }
}
