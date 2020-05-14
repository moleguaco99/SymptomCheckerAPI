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

        private IList<double[]> centroids;
        private IList<double[]> oldCentroids;

        private IList<KeyValuePair<string, double[]>> _dataPoints;
        private IDictionary<int, IList<int>> correspondingPoints;
        
        private IDictionary<string, double[]> clusters;

        public KMeans(MedicalDBContext context)
        {
            _context = context;
            spaceDimension = context.Symptoms.Count();
            numberOfCentroids = context.Diseases.Count();
            clusters = new Dictionary<string, double[]>();

            GetDataPoints();
            InitializeCentroids();
            TrainCentroids();
            AssignClasses();
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

        private void InitializeCentroids()
        {
            centroids = new List<double[]>();
            oldCentroids = new List<double[]>();

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
                        double tempDistance = EuclideanDistance(centroids.ElementAt(centroid - 1), _dataPoints.ElementAt(point).Value);

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
                        }
                        else
                        {
                            choose = 0;
                        }
                    }
                }

                centroids.Add(_dataPoints.ElementAt(choose).Value);
            }
        }

        private void TrainCentroids()
        {
            Random random = new Random();
            int maxNumberIterations = 500, iterationCounter = 0;

            for(int index = 0; index < numberOfCentroids; index += 1)
            {
                oldCentroids.Add(new double[spaceDimension]);
            }
            
            while (CentroidsUpdated() || iterationCounter < maxNumberIterations)
            {
                iterationCounter += 1;
                correspondingPoints = new Dictionary<int, IList<int>>();

                for(int index = 0; index < numberOfCentroids; index += 1)
                {
                    oldCentroids[index] = centroids[index].ToArray();
                    correspondingPoints.Add(index, new List<int>());
                }

                for(int index = 0; index < _dataPoints.Count(); index += 1)
                {
                    int minCentroid = int.MaxValue;
                    double minDistance = double.MaxValue;

                    for(int centroidIndex = 0; centroidIndex < numberOfCentroids; centroidIndex += 1)
                    {
                        double distance = EuclideanDistance(centroids[centroidIndex], _dataPoints[index].Value);
                        if(distance <= minDistance)
                        {
                            minCentroid = centroidIndex;
                            minDistance = distance;
                        }
                    }

                    correspondingPoints[minCentroid].Add(index);
                }

                for(int centroidIndex = 0; centroidIndex < numberOfCentroids; centroidIndex += 1)
                {
                    if(correspondingPoints[centroidIndex].Count() == 0)
                    {
                        centroids[centroidIndex] = _dataPoints.ElementAt(random.Next(_dataPoints.Count())).Value;
                    }
                    else
                    {
                        IList<int> points = correspondingPoints[centroidIndex];
                        double numberOfPoints = points.Count();
                        double[] meanCentroid = new double[spaceDimension];

                        foreach(int point in points)
                        {
                            double[] pointCoordinates = _dataPoints[point].Value;
                            for(int index = 0; index < spaceDimension; index += 1)
                            {
                                meanCentroid[index] += pointCoordinates[index] / numberOfPoints;
                            }
                        }
                    }
                }
            }
        }

        private void AssignClasses()
        {
            for(int centroidIndex = 0; centroidIndex < numberOfCentroids; centroidIndex += 1)
            {
                IList<string> classes = new List<string>();
                IList<int> containedPoints = correspondingPoints[centroidIndex];
                if (containedPoints.Count() == 0)
                    break;
                foreach(int point in containedPoints)
                {
                    classes.Add(_dataPoints.ElementAt(point).Key);
                }

                string cluster = classes.GroupBy(i => i).OrderByDescending(grp => grp.Count())
                                .Select(grp => grp.Key).First();

                if (!clusters.ContainsKey(cluster))
                {
                    clusters.Add(cluster, centroids[centroidIndex]);
                }
            }

            IList<Disease> diseases = _context.Diseases.ToList();
            
            foreach(Disease disease in diseases)
            {
                if (!clusters.ContainsKey(disease.DiseaseName))
                {
                    double[] centroid = _dataPoints.Where(pair => pair.Key.Equals(disease.DiseaseName)).First().Value;
                    clusters.Add(disease.DiseaseName, centroid);
                }
            }
        }

        private bool CentroidsUpdated()
        {
            for (int index = 0; index < numberOfCentroids; index += 1)
            {
                if (centroids[index].Except(oldCentroids[index]).Count() != 0)
                    return true;
            }

            return false;
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


        public IDictionary<string, double> PredictDiseases(string symptoms)
        {
            IDictionary<string, double> diseaseDistances = new Dictionary<string, double>();
            IList<Disease> diseases = _context.Diseases.ToList();

            foreach (Disease disease in diseases)
            {
                diseaseDistances.Add(disease.DiseaseName, EuclideanDistance(disease, symptoms));
            }

            return diseaseDistances.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public double EuclideanDistance(Disease disease, string symptoms)
        {
            int vectorDimension = _context.Symptoms.Count();
            double[] X = new double[vectorDimension];
            double[] Y = clusters[disease.DiseaseName];

            string[] uniqueSymptoms = symptoms.Split(";");

            foreach (string symptom in uniqueSymptoms)
            {
                if (symptom.Equals(""))
                    continue;

                int symptomID = _context.Symptoms.Where(sym => sym.SymptomName.Equals(symptom)).First().Id;
                X[symptomID - 1] = 1d;
            }

            double distance = 0d;
            for (int feature = 0; feature < vectorDimension; feature += 1)
            {
                double difference = Math.Abs(Y[feature] - X[feature]);
                distance += Math.Pow(difference, 2);
            }

            return Math.Pow(distance, 1d / 2);
        }
    }
}