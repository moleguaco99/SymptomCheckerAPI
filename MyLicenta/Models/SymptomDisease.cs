using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.Models
{
    public class SymptomDisease
    {
        public int Id { get; set; }
        public int SymptomID { get; set; }
        public int DiseaseID { get; set; }
        public double OccurenceProbability { get; set; }

        public override bool Equals(object obj)
        {
            return obj is SymptomDisease disease &&
                   SymptomID == disease.SymptomID &&
                   DiseaseID == disease.DiseaseID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SymptomID, DiseaseID);
        }
    }
}
