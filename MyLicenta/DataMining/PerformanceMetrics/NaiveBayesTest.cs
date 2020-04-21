using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining.PerformanceMetrics
{
    public interface INaiveBayesTest
    {
        public double NaiveBayesAccuracy();
    }
    public class NaiveBayesTest : INaiveBayesTest
    {
        private readonly MedicalDBContext _context;
        private INaiveBayes _naiveBayes;

        public NaiveBayesTest(MedicalDBContext context, INaiveBayes naiveBayes)
        {
            _context = context;
            _naiveBayes = naiveBayes;
        }

        public double NaiveBayesAccuracy()
        {
            using var reader = new StreamReader("./FileProcessing/Datasets/Testing.csv");
            var s = reader.ReadLine();

            double numberOfMatches = 0d;
            int numberOfLines = 0;

            while (!reader.EndOfStream)
            {
                numberOfLines += 1;
                var line = reader.ReadLine();
                var values = line.Split(',');
                string symptoms = "";

                for(int index = 0; index < values.Length; index += 1 )
                {
                    if(values[index].Equals("1"))
                    {
                        var symptomName = _context.Symptoms.Where(symptom => symptom.Id == index + 1).First().SymptomName;
                        symptoms = symptoms + symptomName + ";";
                    }
                }

                var dictionary = _naiveBayes.PredictDiseases(symptoms).OrderByDescending(i => i.Value);
                if(dictionary.Count() > 0)
                    if (dictionary.ElementAt(0).Key.Equals(values[^1]))
                        numberOfMatches += 1;
            }

            return numberOfMatches;
        }
    }
}
