using Kwetterprise.TweetService.Common.DataTransfer;
using Microsoft.AspNetCore.Http;
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
        [ProducesResponseType(typeof(Option<TimedData<TweetDto>>), StatusCodes.Status200OK)]
        public IActionResult GetFromUser(Guid id, Guid? from, bool ascending, int count)
        {
            return this.Ok(this.tweetQueryManager.GetFromUser(id, from, ascending, count));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAll")]
        [ProducesResponseType(typeof(TimedData<TweetDto>), StatusCodes.Status200OK)]
        public IActionResult GetAll(Guid? from, bool ascending, int count)
        {
            return this.Ok(this.tweetQueryManager.GetAll(from, ascending, count));
        }
    }
}