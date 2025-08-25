using Microsoft.AspNetCore.Mvc;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public string Get() => "Hello s from C# backend!";
    }

}
