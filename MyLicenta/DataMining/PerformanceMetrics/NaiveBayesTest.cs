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
        public KeyValuePair<double, IDictionary<string, InformationRetrieval>> NaiveBayesMetrics();
    }
    public class NaiveBayesTest : INaiveBayesTest
    {
        IDictionary<string, InformationRetrieval> _performanceMeter;
        private readonly MedicalDBContext _context;
        private INaiveBayes _naiveBayes;

        public NaiveBayesTest(MedicalDBContext context, INaiveBayes naiveBayes)
        {
            _context = context;
            _naiveBayes = naiveBayes;
            _performanceMeter = new Dictionary<string, InformationRetrieval>();

            InitializeMeter();
        }

        private void InitializeMeter()
        {
            IList<Disease> diseases = _context.Diseases.ToList();

            foreach (Disease disease in diseases)
            {
                _performanceMeter.Add(disease.DiseaseName, new InformationRetrieval());
            }
        }

        public KeyValuePair<double, IDictionary<string, InformationRetrieval>> NaiveBayesMetrics()
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
                string disease = values[^1];

                for (int index = 0; index < values.Length; index += 1 )
                {
                    if(values[index].Equals("1"))
                    {
                        var symptomName = _context.Symptoms.Where(symptom => symptom.Id == index + 1).First().SymptomName;
                        symptoms = symptoms + symptomName + ";";
                    }
                }

                var differentialDiagnosis = _naiveBayes.PredictDiseases(symptoms).OrderByDescending(i => i.Value);

                if (differentialDiagnosis.Count() > 0)
                {
                    if (differentialDiagnosis.ElementAt(0).Key.Equals(disease))
                    {
                        numberOfMatches += 1;
                        _performanceMeter[disease].TruePositive += 1;
                    }
                    else
                    {
                        _performanceMeter[differentialDiagnosis.ElementAt(0).Key].FalsePositive += 1;
                        _performanceMeter[disease].FalseNegative += 1;
                    }
                }
                else
                {
                    _performanceMeter[disease].FalseNegative += 1;
                }
            }

            return new KeyValuePair<double, IDictionary<string, InformationRetrieval>>(numberOfMatches, _performanceMeter);
        }
    }
}
