using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyLicenta.Models;

namespace MyLicenta.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SymptomsController : ControllerBase
    {
        private readonly MedicalDBContext _context;

        public SymptomsController(MedicalDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IList<Symptom>> GetSymptoms()
        {
            return new ActionResult<IList<Symptom>>(_context.Symptoms.ToList());
        }
    }
}