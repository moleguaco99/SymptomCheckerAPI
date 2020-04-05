using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyLicenta.DataMining;

namespace MyLicenta.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DiagnosisController : ControllerBase
    {
        private readonly IApriori _apriori;

        public DiagnosisController(IApriori apriori)
        {
            _apriori = apriori;
        }

        [HttpPost]
        public ActionResult<IDictionary<string, double>> GetDiagnosis(string symptoms)
        {
            return new ActionResult<IDictionary<string, double>>(_apriori.AssociateDiseases(symptoms));
        }
    }
}