using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EfficientApiCalls.Api.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        public FileStreamResult Get()
        {
            var fileStream = System.IO.File.Open("E1IlXtbj4.json", System.IO.FileMode.Open);
            return File(fileStream, "application/json");
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]Model value)
        {
        }
    }
}
