using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining
{
    public interface INaiveBayes
    {
       IDictionary<string, double> PredictDiseases(string symptoms);
    }
    public class NaiveBayes : INaiveBayes
    {
        private readonly MedicalDBContext _context;

        public NaiveBayes(MedicalDBContext context)
        {
            _context = context;
        }
        public IDictionary<string, double> PredictDiseases(string symptoms)
        {
            IDictionary<string, double> diseaseProbabilities = new Dictionary<string, double>();
            IList<Disease> diseases = _context.Diseases.ToList();

            foreach (Disease disease in diseases)
            {
                double conditionedDiseaseProbability = ConditionedSymptomsProbability(disease, symptoms) * DiseaseGeneralProbability(disease);
                double conditionedDiseaseRefusal = ConditionedSymptomsRefusal(disease, symptoms) * (1d - DiseaseGeneralProbability(disease));
                double symptomsIntersection = 1 / (conditionedDiseaseProbability + conditionedDiseaseRefusal);

                diseaseProbabilities.Add(disease.DiseaseName, conditionedDiseaseProbability * symptomsIntersection);
            }

            return diseaseProbabilities;
        }

        private double ConditionedSymptomsRefusal(Disease disease, string symptoms)
        {
            string[] uniqueSymptoms = symptoms.Split(";");
            double symptomsProbability = 1d;

            foreach (string symptom in uniqueSymptoms)
            {
                if (symptom.Equals(""))
                    continue;

                int symptomID = _context.Symptoms.Where(sym => sym.SymptomName.Equals(symptom)).First().Id;
                SymptomDisease symptomDisease = _context.SymptomDiseases.Where(symDis => symDis.DiseaseID.Equals(disease.Id) && symDis.SymptomID.Equals(symptomID)).FirstOrDefault();
                if (symptomDisease != null)
                {
                    symptomsProbability *= (1d - symptomDisease.OccurenceProbability);
                }
                else
                    return 0d;
            }

            return symptomsProbability;
        }

        private double DiseaseGeneralProbability(Disease disease)
        {
            return disease.GeneralProbability;
        }

        /*
         * There is still the question of keeping all the symptoms, including the ones that don't occur in the diseases specification
         */

        private double ConditionedSymptomsProbability(Disease disease, string symptoms)
        {
            string[] uniqueSymptoms = symptoms.Split(";");
            double symptomsProbability = 1d;

            foreach(string symptom in uniqueSymptoms)
            {
                if (symptom.Equals(""))
                    continue;

                int symptomID = _context.Symptoms.Where(sym => sym.SymptomName.Equals(symptom)).First().Id;
                SymptomDisease symptomDisease = _context.SymptomDiseases.Where(symDis => symDis.DiseaseID.Equals(disease.Id) && symDis.SymptomID.Equals(symptomID)).FirstOrDefault();
                if (symptomDisease != null)
                {
                    symptomsProbability *= symptomDisease.OccurenceProbability;
                }
                else
                    return 0d;
            }

            return symptomsProbability;
        }
    }
}
