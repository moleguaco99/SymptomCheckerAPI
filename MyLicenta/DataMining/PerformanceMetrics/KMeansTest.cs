using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining.PerformanceMetrics
{
    public interface IKMeansTest
    {
        public double KMeansAccuracy();
    }

    public class KMeansTest : IKMeansTest
    {
        private readonly MedicalDBContext _context;
        private IKMeans _kMeans;

        public KMeansTest(MedicalDBContext context, IKMeans kMeans)
        {
            _context = context;
            _kMeans = kMeans;
        }

        public double KMeansAccuracy()
        {
            using var reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            var s = reader.ReadLine();

            double numberOfMatches = 0d;
            int numberOfLines = 0;

            while (!reader.EndOfStream)
            {
                numberOfLines += 1;
                var line = reader.ReadLine();
                var values = line.Split(',');
                string symptoms = "";

                for (int index = 0; index < values.Length; index += 1)
                {
                    if (values[index].Equals("1"))
                    {
                        var symptomName = _context.Symptoms.Where(symptom => symptom.Id == index + 1).First().SymptomName;
                        symptoms = symptoms + symptomName + ";";
                    }
                }
            
                var dictionary = _kMeans.PredictDiseases(symptoms).OrderBy(i => i.Value);
                if (dictionary.ElementAt(0).Key.Equals(values[^1]))
                    numberOfMatches += 1;
            }

            return numberOfMatches;
        }
    }
}
