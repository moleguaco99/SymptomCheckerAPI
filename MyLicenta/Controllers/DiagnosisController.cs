using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyLicenta.DataMining;
using MyLicenta.DataMining.Algorithms;

namespace MyLicenta.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DiagnosisController : ControllerBase
    {
        private readonly IApriori _apriori;
        private readonly INaiveBayes _naiveBayes;
        private readonly IKMeans _kMeans;

        public DiagnosisController(IApriori apriori, INaiveBayes naiveBayes, IKMeans kMeans)
        {
            _apriori = apriori;
            _naiveBayes = naiveBayes;
            _kMeans = kMeans;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult<IList<IDictionary<string, double>>> GetDiagnosis([FromBody] DiagnosisProperties properties)
        {
            string symptoms = properties.Symptoms;
            IList<IDictionary<string, double>> diagnostics = new List<IDictionary<string, double>>();
            IDictionary<string, double> timestamps = new Dictionary<string, double>();

            double startTime = DateTime.Now.Millisecond;
            diagnostics.Add(_apriori.AssociateDiseases(symptoms));
            double finalTime = DateTime.Now.Millisecond;

            timestamps.Add("Apriori", Math.Abs(finalTime - startTime) / 1000);
            
            if (properties.DisplayAlgorithms)
            {
                startTime = DateTime.Now.Millisecond;
                diagnostics.Add(_naiveBayes.PredictDiseases(symptoms));
                finalTime = DateTime.Now.Millisecond;

                timestamps.Add("Naive Bayes", Math.Abs(finalTime - startTime) / 1000);

                startTime = DateTime.Now.Millisecond;
                diagnostics.Add(_kMeans.PredictDiseases(symptoms));
                finalTime = DateTime.Now.Millisecond;

                timestamps.Add("KMeans", Math.Abs(finalTime - startTime) / 1000);
            }

            diagnostics.Add(timestamps);

            return new ActionResult<IList<IDictionary<string, double>>>(diagnostics);            
        }
    }

    public class DiagnosisProperties
    {
        public string Symptoms { get; set; }
        public bool DisplayAlgorithms { get; set; }
    }
}