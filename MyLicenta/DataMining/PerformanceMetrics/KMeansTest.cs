﻿using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining.PerformanceMetrics
{
    public interface IKMeansTest
    {
        public KeyValuePair<double, IDictionary<string, InformationRetrieval>> KMeansMetrics();
    }

    public class KMeansTest : IKMeansTest
    {
        IDictionary<string, InformationRetrieval> _performanceMeter;

        private readonly MedicalDBContext _context;
        private IKMeans _kMeans;

        public KMeansTest(MedicalDBContext context, IKMeans kMeans)
        {
            _context = context;
            _kMeans = kMeans;
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

        public KeyValuePair<double, IDictionary<string, InformationRetrieval>> KMeansMetrics()
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

                for (int index = 0; index < values.Length; index += 1)
                {
                    if (values[index].Equals("1"))
                    {
                        var symptomName = _context.Symptoms.Where(symptom => symptom.Id == index + 1).First().SymptomName;
                        symptoms = symptoms + symptomName + ";";
                    }
                }

                var differentialDiagnosis = _kMeans.PredictDiseases(symptoms).OrderBy(i => i.Value);

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
