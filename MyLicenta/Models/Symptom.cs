using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.Models
{
    public class Symptom
    {
        public int Id { get; set; }
        public string SymptomName { get; set; }
        public string BodyPart { get; set; }
        public double OccurenceProbability { get; set; }
    }
}
