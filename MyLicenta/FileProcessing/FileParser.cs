using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.FileProcessing
{
    public interface IFileParser
    {
        public void SeparateDS();
        public void CreateAssociations();
        public void DeduceDiseaseProbabilities();
        public void DeduceSymptomsProbabilities();
        public void DeduceCoocurrenceProbability();
        public void SymptomsConditionedProbability();
    }

    public class FileParser : IFileParser
    {
        private readonly MedicalDBContext _context;

        public FileParser(MedicalDBContext context)
        {
            _context = context;
        }

        public void SeparateDS()
        {
            using StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            IList<string> diseaseList = new List<string>();
            IList<string> symptomsList = new List<string>();

            bool firstIteration = true;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (firstIteration)
                {
                    foreach (string value in values)
                    {
                        if (!value.Equals("prognosis"))
                            symptomsList.Add(value);
                    }
                }

                if (!diseaseList.Contains(values[^1]) && !firstIteration)
                    diseaseList.Add(values[^1]);

                firstIteration = false;
            }

            foreach (string disease in diseaseList)
            {
                _context.Diseases.Add(new Disease
                {
                    DiseaseName = disease
                }
                );

                _context.SaveChanges();
            }

            foreach (string symptom in symptomsList)
            {
                _context.Symptoms.Add(new Symptom
                {
                    SymptomName = symptom
                }
                );
                _context.SaveChanges();
            }
        }

        public void CreateAssociations()
        {
            IList<Disease> diseases = _context.Diseases.ToList();
            IList<Symptom> symptoms = _context.Symptoms.ToList();
            IList<SymptomDisease> symptomDiseases = new List<SymptomDisease>();

            using StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                string diseaseName = values[^1];

                int diseaseID = diseases
                    .Where(disease => disease.DiseaseName.Equals(diseaseName)).First().Id;

                for (int index = 0; index < values.Length - 1; index += 1)
                {
                    if (values[index].Equals("1"))
                    {
                        int symptomID = index + 1;
                        SymptomDisease symptomDisease = new SymptomDisease()
                        {
                            DiseaseID = diseaseID,
                            SymptomID = symptomID
                        };

                        if (!symptomDiseases.Contains(symptomDisease))
                        {
                            symptomDiseases.Add(symptomDisease);
                        }
                    }
                }
            }

            foreach (SymptomDisease symDys in symptomDiseases)
            {
                _context.SymptomDiseases.Add(symDys);
                _context.SaveChanges();
            }
        }

        public void DeduceDiseaseProbabilities()
        {

            using StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            reader.ReadLine();

            int numberOfLines = 0;

            IDictionary<string, double> diseaseFrequence = new Dictionary<string, double>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                string diseaseName = values[^1];
                if (!diseaseFrequence.ContainsKey(diseaseName))
                {
                    diseaseFrequence.Add(diseaseName, 1d);
                }
                else
                {
                    diseaseFrequence[diseaseName] += 1;
                }
                numberOfLines += 1;
            }

            foreach (string key in diseaseFrequence.Keys)
            {
                Disease disease = _context.Diseases.Where(disease => disease.DiseaseName.Equals(key)).First();

                disease.GeneralProbability = diseaseFrequence[key] / numberOfLines;
                _context.SaveChanges();
            }
        }

        public void DeduceSymptomsProbabilities()
        {
            using StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            reader.ReadLine();

            int numberOfLines = 0;
            IDictionary<int, double> symptomFrequence = new Dictionary<int, double>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');
                for (int index = 0; index < values.Length - 1; index += 1)
                {
                    if (values[index].Equals("1"))
                    {
                        if (!symptomFrequence.ContainsKey(index))
                        {
                            symptomFrequence.Add(index, 1d);
                        }
                        else
                        {
                            symptomFrequence[index] += 1d;
                        }
                    }
                }
                numberOfLines += 1;
            }

            foreach (int key in symptomFrequence.Keys)
            {
                Symptom symptom = _context.Symptoms.Where(sympt => sympt.Id.Equals(key + 1)).First();

                symptom.OccurenceProbability = symptomFrequence[key] / numberOfLines;
                _context.SaveChanges();
            }
        }

        public void DeduceCoocurrenceProbability()
        {
            using StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            reader.ReadLine();

            IList<Disease> diseases = _context.Diseases.ToList();
            IDictionary<string, double[]> symDisFrequence = new Dictionary<string, double[]>();

            foreach (Disease disease in diseases)
            {
                symDisFrequence.Add(disease.DiseaseName, new double[132]);
            }

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                string diseaseName = values[^1];

                for (int index = 0; index < values.Length - 1; index += 1)
                {
                    if (values[index].Equals("1"))
                    {
                        symDisFrequence[diseaseName][index] += 1d;
                    }
                }
            }

            foreach (string key in symDisFrequence.Keys)
            {
                int diseaseID = _context.Diseases.Where(dis => dis.DiseaseName.Equals(key)).First().Id;
                for (int index = 0; index < symDisFrequence[key].Length; index += 1)
                {
                    if (symDisFrequence[key][index] > 0d)
                    {
                        SymptomDisease symDis = _context.SymptomDiseases.Where(disSym => disSym.DiseaseID == diseaseID && disSym.SymptomID == (index + 1)).First();
                        symDis.OccurenceProbability = symDisFrequence[key][index] / NumberOfCases(key);
                        _context.SaveChanges();
                    }
                }
            }
        }

        public void SymptomsConditionedProbability()
        {
            using StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            reader.ReadLine();

            int numberOfSymptoms = _context.Symptoms.Count();
            int numberOfDiseases = _context.Diseases.Count();
            IDictionary<int, double[]> symDisFrequence = new Dictionary<int, double[]>();

            for (int index = 0; index < numberOfSymptoms; index += 1)
            {
                symDisFrequence.Add(index + 1, new double[numberOfDiseases]);
            }

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(",");

                string diseaseName = values[^1];
                int diseaseID = _context.Diseases.Where(dis => dis.DiseaseName.Equals(diseaseName)).First().Id;

                for (int index = 0; index < values.Length - 1; index += 1)
                {
                    if (values[index].Equals("1"))
                    {
                        symDisFrequence[index + 1][diseaseID - 1] += 1d;
                    }
                }
            }

            foreach (int key in symDisFrequence.Keys)
            {
                double totalCases = NumberOfCases(symDisFrequence[key]);
                for(int index = 0; index < symDisFrequence[key].Length; index += 1)
                {
                    if(symDisFrequence[key][index] > 0)
                    {
                        SymptomDisease symptomDisease = _context.SymptomDiseases.Where(symDis => symDis.DiseaseID == index + 1 && symDis.SymptomID == key).First();
                        symptomDisease.OccurenceProbability = symDisFrequence[key][index] / totalCases;
                        _context.SaveChanges();
                    }
                }
            }
        }

        private double NumberOfCases(string key)
        {
            double cases = 0d;
            using StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            reader.ReadLine();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                string diseaseName = values[^1];
                if (diseaseName.Equals(key))
                {
                    cases += 1d;
                }
            }
            return cases;
        }

        private double NumberOfCases(double[] cases)
        {
            double sum = 0d;
            foreach(double num in cases)
            {
                sum += num;
            }
            return sum; 
        }
    }
}
