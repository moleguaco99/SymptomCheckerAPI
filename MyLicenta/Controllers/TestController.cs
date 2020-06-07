using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyLicenta.DataMining;
using MyLicenta.DataMining.PerformanceMetrics;
using MyLicenta.FileProcessing;

namespace MyLicenta.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "";
        }
    }
}
