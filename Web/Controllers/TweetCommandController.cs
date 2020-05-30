using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TweetCommandController : ControllerBase
    {
        private readonly ILogger<TweetCommandController> _logger;

        public TweetCommandController(ILogger<TweetCommandController> logger)
        {
            this._logger = logger;
        }

        [HttpGet]
        public void Get()
        {

        }
    }
}
