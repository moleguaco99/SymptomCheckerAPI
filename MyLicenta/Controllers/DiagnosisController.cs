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

        public DiagnosisController(IApriori apriori, INaiveBayes naiveBayes)
        {
            _apriori = apriori;
            _naiveBayes = naiveBayes;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult<IList<IDictionary<string, double>>> GetDiagnosis([FromBody] DiagnosisProperties properties)
        {
            string symptoms = properties.Symptoms;
            IList<IDictionary<string, double>> diagnostics = new List<IDictionary<string, double>>();
            
            diagnostics.Add(_apriori.AssociateDiseases(symptoms));
            
            if (properties.DisplayAlgorithms)
            {
                diagnostics.Add(_naiveBayes.PredictDiseases(symptoms));
                //diagnostics.Add(_kMeans.PredictDiseases(symptoms));
            }
            
            return new ActionResult<IList<IDictionary<string, double>>>(diagnostics);            
        }
    }

    public class DiagnosisProperties
    {
        public string Symptoms { get; set; }
        public bool DisplayAlgorithms { get; set; }
    }
}