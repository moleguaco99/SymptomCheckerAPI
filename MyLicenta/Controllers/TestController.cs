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
        /*
        private readonly IFileParser _parser;
        private readonly IKNearestNeighborsTest _kNearestNeighborsTest;
        private readonly IAprioriTest _aprioriTest;
        private readonly IKMeansTest _kMeansTest;
        */

        private readonly INaiveBayesTest _naiveBayesTest;

        public TestController(INaiveBayesTest naiveBayesTest /*IKNearestNeighborsTest kNearestNeighborsTest,
        IAprioriTest aprioriTest*/)
        {
            //_parser = parser;
            //_kMeansTest = kMeansTest;
            //_kNearestNeighborsTest = kNearestNeighborsTest;
            _naiveBayesTest = naiveBayesTest;
        }

        [HttpGet]
        public string Get()
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            KeyValuePair<double, IDictionary<string, InformationRetrieval>> naiveBayesMetrics = _naiveBayesTest.NaiveBayesMetrics();
            stopwatch.Stop();

            string elapsedTime = "Elapsed time: " + stopwatch.ElapsedMilliseconds + "\n";
            
            double accuracy = naiveBayesMetrics.Key / 542;
            string accuracyResult = "Accuracy: " + accuracy + "\n"; 
            IDictionary<string, InformationRetrieval> multiclassMetrics = naiveBayesMetrics.Value;

            double precision = 0d, recall = 0d, f1Score = 0d;
            foreach(string disease in multiclassMetrics.Keys)
            {
                double computedPrecision = multiclassMetrics[disease].ComputePrecision();
                if (double.IsNaN(computedPrecision))
                {
                    recall += multiclassMetrics[disease].ComputeRecall();
                }
                else
                {
                    precision += computedPrecision;
                    recall += multiclassMetrics[disease].ComputeRecall();
                    f1Score += multiclassMetrics[disease].ComputeF1Score();
                }
            }

            string averagePrecision = "Precision: " + precision / multiclassMetrics.Count() + "\n";
            string averageRecall = "Recall: " + recall / multiclassMetrics.Count() + "\n";
            string averageF1Score = "F1Score: " + f1Score / multiclassMetrics.Count() + "\n";

            return elapsedTime + accuracyResult + averagePrecision + averageRecall + averageF1Score;
        }

    }
}
