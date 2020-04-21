using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining.PerformanceMetrics
{
    public interface IAprioriTest
    {
        public double AprioriAccuracy();
    }
    public class AprioriTest : IAprioriTest
    {
        private readonly MedicalDBContext _context;
        private IApriori _apriori;

        public AprioriTest(MedicalDBContext context, IApriori apriori)
        {
            _context = context;
            _apriori = apriori;
        }

        public double AprioriAccuracy()
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

                for (int index = 0; index < values.Length; index += 1)
                {
                    if (values[index].Equals("1"))
                    {
                        var symptomName = _context.Symptoms.Where(symptom => symptom.Id == index + 1).First().SymptomName;
                        symptoms = symptoms + symptomName + ";";
                    }
                }

                var dictionary = _apriori.AssociateDiseases(symptoms).OrderByDescending(i => i.Value);
                if(dictionary.Count() > 0)
                    if (dictionary.ElementAt(0).Key.Equals(values[^1]))
                        numberOfMatches += 1;
                
            }

            return numberOfMatches;
        }
    }
}
