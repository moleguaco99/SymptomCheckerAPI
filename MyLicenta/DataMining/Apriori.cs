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

        public Apriori(MedicalDBContext context)
        {
            _context = context;
        }

        public IDictionary<string, double> AssociateDiseases(string symptoms)
        {
            IDictionary<string, double> predictedDiseases = new Dictionary<string, double>(); 

            int supportCount = 2;
            GenerateLikelihoods(supportCount);

            return predictedDiseases;
        }

        private void GenerateLikelihoods(int supportCount)
        {
            IList<Symptom> symptoms = new List<Symptom>();
            IList<SymptomDisease> symptomDiseases = _context.SymptomDiseases.ToList();
            IList<ISet<int>> initialItemsets = CreateItemsets();

            IList<Itemsets> frequentItemsets = new List<Itemsets>();
            IList<ISet<int>> infrequentItemsets = new List<ISet<int>>();

            Itemsets L1 = new Itemsets();

            foreach (var symptom in symptoms) 
            {
                int count = symptomDiseases
                                .Where(symDis => symDis.SymptomID.Equals(symptom.Id))
                                .Count();

                L1.AddItemset(new SortedSet<int>() { symptom.Id }, count);
            }

            RemoveUnsupportedSets(L1, infrequentItemsets, supportCount);
            
            int itemsetSize = 2;
            bool emptyCandidate = L1.SetsFrequency.Count() == 0;

            frequentItemsets.Add(L1);

            while (!emptyCandidate)        
            {
                Itemsets Lk_1 = frequentItemsets[^1];
                Itemsets Lk = GroupItemsets(Lk_1, infrequentItemsets, initialItemsets, itemsetSize);

                RemoveUnsupportedSets(Lk, infrequentItemsets, supportCount);
                emptyCandidate = Lk.SetsFrequency.Count() == 0;

                if (!emptyCandidate)
                {
                    frequentItemsets.Add(Lk);
                    itemsetSize += 1;
                }
            }
        }

        private IList<ISet<int>> CreateItemsets()
        {
            IList<ISet<int>> itemsets = new List<ISet<int>>(_context.Diseases.Count());

            for(int index = 0; index < itemsets.Count(); index += 1)
            {
                itemsets[index] = new SortedSet<int>();
            }

            foreach(var symDis in _context.SymptomDiseases)
            {
                itemsets[symDis.DiseaseID - 1].Add(symDis.SymptomID);
            }

            return itemsets;
        }

        private Itemsets GroupItemsets(Itemsets Lk_1, IList<ISet<int>> infrequentItemsets, IList<ISet<int>> initialItemsets, int itemsetSize)
        {
            Itemsets Lk = new Itemsets();
            List<ISet<int>> groupingSets = Lk_1.SetsFrequency.Keys.ToList();

            for(int i = 0; i < groupingSets.Count(); i += 1)
            {
                for(int j = i + 1; j < groupingSets.Count(); j += 1)
                {
                    SortedSet<int> itemset = (SortedSet<int>)groupingSets[i];
                    itemset.UnionWith(groupingSets[j]);
                    
                    if (itemset.Count() == itemsetSize)
                        Lk.AddItemset(itemset, 0);
                }
            }   

            foreach(KeyValuePair<ISet<int>, int> itemset in Lk.SetsFrequency)
            {
                foreach(ISet<int> rareSet in infrequentItemsets)
                {
                    if (itemset.Key.IsSupersetOf(rareSet))
                    {
                        infrequentItemsets.Add(itemset.Key);
                        Lk.RemoveItemset(itemset.Key);
                        break;
                    }
                }
            }
            
            foreach(KeyValuePair<ISet<int>, int> itemset in Lk.SetsFrequency)
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

        public void RemoveUnsupportedSets(Itemsets Lk, IList<ISet<int>> infrequentItemsets, int supportCount)
        {
            foreach (KeyValuePair<ISet<int>, int> itemset in Lk.SetsFrequency)
            {
                if (itemset.Value < supportCount)
                {
                    infrequentItemsets.Add(itemset.Key);
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
            SetsFrequency.Add(set, count);
        }

        public void RemoveItemset(ISet<int> set)
        {
            SetsFrequency.Remove(set);
        }
    }
}