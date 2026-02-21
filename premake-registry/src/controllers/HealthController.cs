using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace premake.controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {

        public HealthController() {
        }
        [HttpHead]
        [EnableCors("PublicApiPolicy")]
        public IActionResult Get() => Ok(new { status = "ok" });
    }

}