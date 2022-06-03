using Microsoft.AspNetCore.Mvc;
using Models;

namespace Benchmark.Controllers
{
    public class BenchmarkController : Controller
    {
        [HttpGet("/controller")]
        public ActionResult<Droid> GetDroid()
        {
            return Ok(new Droid { Name = "R5-D4", PrimaryFunction = "Astromech" });
        }
    }
}
