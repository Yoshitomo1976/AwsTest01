using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AwsTest01.Application;

namespace Project1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Function01Controller : ControllerBase
    {
        private readonly ITestInterface _testInterface;

        public Function01Controller(ITestInterface testInterface) => _testInterface = testInterface;

        [HttpGet("{input}")]
        public string Get(string input)
        {
            try
            {
                return _testInterface.EchoToUpper(input);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
