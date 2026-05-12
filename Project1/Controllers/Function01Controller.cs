using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Project1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Function01Controller : ControllerBase
    {
        [HttpGet("{input}")]
        public string Get(string input)
        {
            return input.ToUpper();
        }
    }
}
