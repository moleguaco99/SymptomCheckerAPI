using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining
{
    public interface IApriori
    {
        public IDictionary<string, double> AssociateDiseases(string symptoms);
    }
    public class Apriori : IApriori
    {
        private readonly MedicalDBContext _context;
        private IDictionary<string, IList<Itemsets>> diseaseItemsets;

        public Apriori(MedicalDBContext context)
        {
            _context = context;
            CreateAssociationRules();
        }

        private void CreateAssociationRules()
        {
            diseaseItemsets = new Dictionary<string, IList<Itemsets>>();
            IList<Disease> diseases = _context.Diseases.ToList();

            foreach(Disease disease in diseases)
            {
                diseaseItemsets.Add(disease.DiseaseName, GenerateLikelihoods(2, disease));
            }
        }

        public IDictionary<string, double> AssociateDiseases(string symptoms)
        {
            string[] uniqueSymptoms = symptoms.Split(";");
            IDictionary<string, double> predictedDiseases = new Dictionary<string, double>();
            IList<Disease> diseases = _context.Diseases.ToList();

            ISet<int> setOfSymptoms = new SortedSet<int>();

            foreach(string symptom in uniqueSymptoms)
            {
                if (symptom.Equals(""))
                    continue;

                int symptomID = _context.Symptoms.Where(sym => sym.SymptomName.Equals(symptom)).First().Id;
                setOfSymptoms.Add(symptomID);
            }

            foreach(Disease disease in diseases)
            {
                double likelihood = DetermineLikelihood(disease, setOfSymptoms);
                if(likelihood != 0)
                    predictedDiseases.Add(disease.DiseaseName, 1/likelihood);
            }

            return predictedDiseases.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private double DetermineLikelihood(Disease disease, ISet<int> setOfSymptoms)
        {
            double likelihood = 0d;
            IList<Itemsets> symptomFrequencies = diseaseItemsets[disease.DiseaseName];
            
            if(setOfSymptoms.Count() <= symptomFrequencies.Count())
            {
                Itemsets searchingItemsets = symptomFrequencies[setOfSymptoms.Count() - 1];
                foreach(ISet<int> items in searchingItemsets.SetsFrequency.Keys)
                {
                    if (setOfSymptoms.SetEquals(items))
                    {
                        likelihood = searchingItemsets.SetsFrequency[items];
                    }
                }
            }

            ISet<int> diseaseSymptoms = _context.SymptomDiseases.Where(symDis => symDis.DiseaseID == disease.Id)
                                           .Select(symDis => symDis.SymptomID).ToHashSet();

            if(likelihood == 0d)
            {
                if (setOfSymptoms.IsSubsetOf(diseaseSymptoms) || setOfSymptoms.SetEquals(diseaseSymptoms))
                {
                    likelihood = 1;
                }
            }

            return likelihood;
        }

        public IList<Itemsets> GenerateLikelihoods(int supportCount, Disease disease)
        {
            IList<SymptomDisease> symptomDiseases = _context.SymptomDiseases.ToList();

            IList<ISet<int>> initialItemsets = CreateItemsets();

            IList<Itemsets> frequentItemsets = new List<Itemsets>();
            Itemsets L1 = new Itemsets();

            IList<SymptomDisease> symptoms = _context.SymptomDiseases.Where(symDis => symDis.DiseaseID == disease.Id).ToList();

            foreach (SymptomDisease symptom in symptoms) 
            {
                int count = symptomDiseases
                                .Where(symDis => symDis.SymptomID.Equals(symptom.SymptomID))
                                .Count();

                L1.AddItemset(new SortedSet<int>() { symptom.SymptomID }, count);
            }

            RemoveUnsupportedSets(L1, supportCount);
            
            int itemsetSize = 2;
            bool emptyCandidate = L1.SetsFrequency.Count() == 0;

            frequentItemsets.Add(L1);

            while (!emptyCandidate)        
            {
                Itemsets Lk_1 = frequentItemsets[^1];
                Itemsets Lk = GroupItemsets(Lk_1, initialItemsets, itemsetSize);

                RemoveUnsupportedSets(Lk, supportCount);
                emptyCandidate = Lk.SetsFrequency.Count() == 0;

                if (!emptyCandidate)
                {
                    frequentItemsets.Add(Lk);
                    itemsetSize += 1;
                }
            }

            return frequentItemsets;
        }

        private IList<ISet<int>> CreateItemsets()
        {
            IList<ISet<int>> itemsets = new List<ISet<int>>();

            for(int index = 0; index < _context.Diseases.Count(); index += 1)
            {
                itemsets.Add(new SortedSet<int>());
            }

            foreach(SymptomDisease symDis in _context.SymptomDiseases)
            {
                itemsets[symDis.DiseaseID - 1].Add(symDis.SymptomID);
            }

            return itemsets;
        }

        private Itemsets GroupItemsets(Itemsets Lk_1, IList<ISet<int>> initialItemsets, int itemsetSize)
        {
            Itemsets Lk = new Itemsets();
            List<ISet<int>> groupingSets = Lk_1.SetsFrequency.Keys.ToList();

            for(int i = 0; i < groupingSets.Count(); i += 1)
            {
                for(int j = i + 1; j < groupingSets.Count(); j += 1)
                {
                    HashSet<int> itemset = groupingSets[i].ToHashSet();
                    itemset.UnionWith(groupingSets[j].ToHashSet());

                    SortedSet<int> sortedItemset = new SortedSet<int>(itemset);
                    if (sortedItemset.Count() == itemsetSize)
                        Lk.AddItemset(sortedItemset, 0);
                }
            }   

            foreach(KeyValuePair<ISet<int>, int> itemset in Lk.SetsFrequency.ToList())
            {
                int frequence = 0;
                foreach(ISet<int> initialItemset in initialItemsets)
                {
                    if (itemset.Key.IsSubsetOf(initialItemset))
                        frequence += 1;
                }
                Lk.SetsFrequency[itemset.Key] = frequence;
            }
                 
            return Lk;
        }

        public void RemoveUnsupportedSets(Itemsets Lk, int supportCount)
        {
            foreach (KeyValuePair<ISet<int>, int> itemset in Lk.SetsFrequency)
            {
                if (itemset.Value < supportCount)
                {
                    Lk.RemoveItemset(itemset.Key);
                }
            }
        }
    }

    public class Itemsets
    {
        public IDictionary<ISet<int>, int> SetsFrequency { get; set; }

        public Itemsets()
        {
            SetsFrequency = new Dictionary<ISet<int>, int>();
        }

        public void AddItemset(ISet<int> set, int count)
        {
            if(!ContainsSet(set))
                SetsFrequency.Add(set, count);
        }

        public void RemoveItemset(ISet<int> set)
        {
            SetsFrequency.Remove(set);
        }

        private bool ContainsSet(ISet<int> set)
        {
            return SetsFrequency.Where(x => x.Key.SetEquals(set)).Count() > 0;
        }

        public override string ToString()
        {
            string frequencies = "";
            foreach(ISet<int> itemset in SetsFrequency.Keys)
            {
                string items = "Items:";
                foreach (int item in itemset)
                {
                    items = items + item + ", ";
                }
                items = items + "Frequency: " + SetsFrequency[itemset] + "\n";

                frequencies += items;
            }
            return frequencies;
        }
    }
}