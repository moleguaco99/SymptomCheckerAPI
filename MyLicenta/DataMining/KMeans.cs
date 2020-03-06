using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining
{
    public interface IKMeans
    {
        IDictionary<string, double> PredictDiseases(string symptoms);
    }
    public class KMeans : IKMeans
    {
        private readonly MedicalDBContext _context;

        public KMeans(MedicalDBContext context)
        {
            _context = context;
        }

        public IDictionary<string, double> PredictDiseases(string symptoms)
        {
            IDictionary<string, double> diseaseDistances = new Dictionary<string, double>();
            IList<Disease> diseases = _context.Diseases.ToList();

            foreach(var disease in diseases)
            {
                diseaseDistances.Add(disease.DiseaseName, MinkowskiDistance(disease, symptoms));
            }
                
            return diseaseDistances;
        }

        public double MinkowskiDistance(Disease disease, string symptoms)
        {
            int vectorDimension = _context.Symptoms.Count();
            double[] X = new double[vectorDimension];
            double[] Y = new double[vectorDimension];
            string[] uniqueSymptoms = symptoms.Split(";");

            IList<SymptomDisease> symptomsX = _context.SymptomDiseases.Where(symDis => symDis.DiseaseID == disease.Id).ToList();
            
            foreach(var symptom in symptomsX)
            {
                Y[symptom.Id - 1] = 1d;
            }

            foreach(var symptom in uniqueSymptoms)
            {
                int symptomID = _context.Symptoms.Where(sym => sym.SymptomName.Equals(symptom)).First().Id;
                X[symptomID - 1] = 1d;
            }

            double distance = 0d;
            for(int feature = 0; feature < vectorDimension; feature += 1)
            {
                double difference = Math.Abs(Y[feature] - X[feature]);
                distance += Math.Pow(difference, vectorDimension);
            }

            return Math.Pow(distance, 1/(double)(vectorDimension));
        }
    }
}
