
using System;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandling.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get() => "Ok";

        [HttpGet("Exception")]
        public ActionResult Get(int id) => throw new InvalidOperationException("Throw Exception");
    }
}
