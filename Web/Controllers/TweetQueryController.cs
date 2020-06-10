using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    using System;
    using Business.Manager;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TweetQueryController : Controller
    {
        private readonly ILogger<TweetQueryController> _logger;
        private readonly ITweetQueryManager tweetQueryManager;

        public TweetQueryController(ILogger<TweetQueryController> logger, ITweetQueryManager tweetQueryManager)
        {
            this._logger = logger;
            this.tweetQueryManager = tweetQueryManager;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetFromUser")]
        public IActionResult GetFromUser(Guid id, int pageSize, int pageNumber)
        {
            return this.Ok(this.tweetQueryManager.GetFromUser(id, pageSize, pageNumber));
        }
    }
}