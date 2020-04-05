using System;
using System.Collections.Generic;
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
        private readonly IFileParser _parser;
        private readonly INaiveBayesTest _naiveBayesTest;
        private readonly IKMeansTest _kMeansTest;
        private readonly IKNearestNeighborsTest _kNearestNeighborsTest;
        private readonly IAprioriTest _aprioriTest;

        public TestController(IFileParser parser, INaiveBayesTest naiveBayesTest, IKMeansTest kMeansTest, IKNearestNeighborsTest kNearestNeighborsTest, IAprioriTest aprioriTest)
        {
            _parser = parser;
            _naiveBayesTest = naiveBayesTest;
            _kMeansTest = kMeansTest;
            _kNearestNeighborsTest = kNearestNeighborsTest;
            _aprioriTest = aprioriTest;
        }

        [HttpGet]
        public string Get()
        {
            return "alabama";
        }
    }
}
