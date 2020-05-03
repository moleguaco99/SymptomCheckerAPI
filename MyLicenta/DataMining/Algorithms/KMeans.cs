using MyLicenta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining
{
    public interface IKMeans
    {
        IDictionary<string, double> PredictDiseases(string symptoms);
    }
    public class KMeans : IKMeans
    {
        private readonly MedicalDBContext _context;
        
        private readonly int spaceDimension;
        private readonly int numberOfCentroids;

        private IDictionary<string, double[]> centroids;
        private IDictionary<string, double[]> oldCentroids;
        private IList<KeyValuePair<string,double[]>> _dataPoints;
        
        public KMeans(MedicalDBContext context)
        {
            _context = context;
            spaceDimension = context.Symptoms.Count();
            numberOfCentroids = context.Diseases.Count();

            GetDataPoints();
            while (true)
            {
                try
                {
                    AssignCentroids();
                    break;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            TrainCentroids();
        }

        private void GetDataPoints()
        {
            _dataPoints = new List<KeyValuePair<string,double[]>>();
            StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] trainingSymptoms = line.Split(",");
                string diseaseName = trainingSymptoms[^1];

                double[] dataPoint = new double[spaceDimension];

                for (int index = 0; index < spaceDimension - 1; index += 1)
                {
                    if (trainingSymptoms[index].Equals("1"))
                        dataPoint[index] = 1d;
                }

                _dataPoints.Add(new KeyValuePair<string, double[]>(diseaseName, dataPoint));
            }
        }

        private void AssignCentroids()
        {
            centroids = new Dictionary<string, double[]>();
            oldCentroids = new Dictionary<string, double[]>();

            double[] distanceToClosestCentroid = new double[_dataPoints.Count()];
            double[] weightedDistribution = new double[_dataPoints.Count()];

            Random random = new Random();
            int choose = 0;

            for(int centroid = 0; centroid < numberOfCentroids; centroid += 1)
            {
                if (centroid == 0)
                {
                    choose = random.Next(_dataPoints.Count());
                }
                else
                {
                    for(int point = 0; point < _dataPoints.Count(); point += 1)
                    {
                        double tempDistance = EuclideanDistance(centroids.ElementAt(centroid-1).Value, _dataPoints.ElementAt(point).Value);

                        if(centroid == 1)
                        {
                            distanceToClosestCentroid[point] = tempDistance;
                        }
                        else
                        {
                            if(tempDistance < distanceToClosestCentroid[point])
                            {
                                distanceToClosestCentroid[point] = tempDistance;
                            }
                        }

                        if(point == 0)
                        {
                            weightedDistribution[0] = distanceToClosestCentroid[0];
                        }
                        else
                        {
                            weightedDistribution[point] = weightedDistribution[point - 1] + distanceToClosestCentroid[point];
                        }
                    }

                    double probability = random.NextDouble();
                    for(int nextCentroid = _dataPoints.Count() - 1; nextCentroid > 0; nextCentroid -= 1)
                    {
                        if(probability > weightedDistribution[nextCentroid - 1] / weightedDistribution[_dataPoints.Count() - 1])
                        {
                            choose = nextCentroid;
                            if(!centroids.ContainsKey(_dataPoints.ElementAt(choose).Key))
                                break;
                        }
                        else
                        {
                            choose = 0;
                        }
                    }
                }

                centroids.Add(_dataPoints.ElementAt(choose).Key, _dataPoints.ElementAt(choose).Value);
            }
        }
       
        private void TrainCentroids()
        {

            IList<string> diseases = _context.Diseases.Select(disease => disease.DiseaseName).ToList();
            foreach(string disease in diseases)
            {
                oldCentroids.Add(disease, new double[spaceDimension]);
            }

            while (CentroidsUpdated())
            {
                IDictionary<string, IList<double[]>> dataPoints = new Dictionary<string, IList<double[]>>();

                foreach(string disease in diseases)
                {
                    dataPoints.Add(disease, new List<double[]>());
                    oldCentroids[disease] = centroids[disease].ToArray();
                }

                StreamReader reader = new StreamReader("./FileProcessing/Datasets/Training.csv");
                reader.ReadLine();
                
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] trainingSymptoms = line.Split(",");
                    string diseaseName = "";
                    double minDistance = double.MaxValue;

                    double[] dataPoint = new double[spaceDimension];

                    for (int index = 0; index < spaceDimension - 1; index += 1)
                    {
                        if (trainingSymptoms[index].Equals("1"))
                            dataPoint[index] = 1d;
                    }

                    foreach(string label in centroids.Keys)
                    {
                        double[] X = centroids[label];
                        double distance = 0d;

                        for (int feature = 0; feature < spaceDimension; feature += 1)
                        {
                            double difference = Math.Abs(dataPoint[feature] - X[feature]);
                            distance += Math.Pow(difference, 2);
                        }

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            diseaseName = label;
                        }
                    }

                    dataPoints[diseaseName].Add(dataPoint);
                }

                foreach(string label in dataPoints.Keys)
                {
                    IList<double[]> points = dataPoints[label];
                    double[] centroid = new double[spaceDimension];

                    foreach(double[] point in points)
                    {
                        for(int index = 0; index < spaceDimension; index += 1)
                        {
                            centroid[index] += point[index] / points.Count();
                        }
                    }
                }
            }
        }

        private bool CentroidsUpdated()
        {
            foreach(string key in centroids.Keys)
            {
                if (centroids[key].Except(oldCentroids[key]).Count() != 0)
                    return true;
            }
            return false;
        }

        public IDictionary<string, double> PredictDiseases(string symptoms)
        {
            IDictionary<string, double> diseaseDistances = new Dictionary<string, double>();
            IList<Disease> diseases = _context.Diseases.ToList();

            foreach(Disease disease in diseases)
            {
                diseaseDistances.Add(disease.DiseaseName, EuclideanDistance(disease, symptoms));
            }

            return diseaseDistances;
        }

        public double EuclideanDistance(Disease disease, string symptoms)
        {
            int vectorDimension = _context.Symptoms.Count();
            double[] X = new double[vectorDimension];
            double[] Y = centroids[disease.DiseaseName];

            string[] uniqueSymptoms = symptoms.Split(";");

            foreach(string symptom in uniqueSymptoms)
            {
                if (symptom.Equals(""))
                    continue;

                int symptomID = _context.Symptoms.Where(sym => sym.SymptomName.Equals(symptom)).First().Id;
                X[symptomID - 1] = 1d;
            }

            double distance = 0d;
            for(int feature = 0; feature < vectorDimension; feature += 1)
            {
                double difference = Math.Abs(Y[feature] - X[feature]);
                distance += Math.Pow(difference, 2);
            }

            return Math.Pow(distance, 1d/2);
        }

        public double EuclideanDistance(double[] X, double[] Y)
        {
            double distance = 0d;
            for (int feature = 0; feature < spaceDimension; feature += 1)
            {
                double difference = Math.Abs(Y[feature] - X[feature]);
                distance += Math.Pow(difference, 2);
            }

            return Math.Pow(distance, 1d / 2);
        }
    }
}