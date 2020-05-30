using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("[controller]")]
    public class TweetQueryController : Controller
    {
        private readonly ILogger<TweetQueryController> _logger;

        public TweetQueryController(ILogger<TweetQueryController> logger)
        {
            this._logger = logger;
        }

        [HttpGet]
        public void Get()
        {

        }
    }
}