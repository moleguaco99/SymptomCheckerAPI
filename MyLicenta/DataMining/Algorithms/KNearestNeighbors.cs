using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining.Algorithms
{
    public interface IKNearestNeighbors
    {
        public IList<KeyValuePair<string, double>> AdjoinDiseases(string symptoms);
    }
    public class KNearestNeighbors : IKNearestNeighbors
    {
        private readonly MedicalDBContext _context;

        public KNearestNeighbors(MedicalDBContext context)
        {
            _context = context;
        }

        public IList<KeyValuePair<string, double>> AdjoinDiseases(string symptoms)
        {
            int vectorDimension = _context.Symptoms.Count();
            string[] uniqueSymptoms = symptoms.Split(";");
            IList<KeyValuePair<string, double>> diseaseProbability = new List<KeyValuePair<string, double>>();

            double[] X = new double[vectorDimension];
            foreach(string symptom in uniqueSymptoms)
            {
                if (symptom.Equals(""))
                    continue;

                int symptomID = _context.Symptoms.Where(sym => sym.SymptomName.Equals(symptom)).First().Id;
                X[symptomID - 1] = 1d;
            }

            StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] trainingSymptoms = line.Split(",");
                string disease = trainingSymptoms[^1];

                double[] Y = new double[vectorDimension];

                for(int index = 0; index < vectorDimension-1; index += 1)
                {
                    if(trainingSymptoms[index].Equals("1"))
                        Y[index] = 1d;
                }

                double distance = 0d;
                for (int feature = 0; feature < vectorDimension; feature += 1)
                {
                    double difference = Math.Abs(Y[feature] - X[feature]);
                    distance += difference;
                }

                diseaseProbability.Add(new KeyValuePair<string, double>(disease, Math.Pow(distance, 1d / 2)));
            }

            return diseaseProbability.OrderBy(item => item.Value).ToList();
        }
    }
}
