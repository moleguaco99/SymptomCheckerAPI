using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.DataMining.PerformanceMetrics
{
    public class InformationRetrieval
    {
        public double TruePositive { get; set; }
        public double TrueNegative { get; set; }
        public double FalsePositive { get; set; }
        public double FalseNegative { get; set; }

        public InformationRetrieval()
        {
            TruePositive = 0d;
            TrueNegative = 0d;
            FalsePositive = 0d;
            FalseNegative = 0d;
        }

        public double ComputePrecision()
        {
            return TruePositive / (TruePositive + FalsePositive);
        }

        public double ComputeRecall()
        {
            return TruePositive / (TruePositive + FalseNegative);
        }

        public double ComputeF1Score()
        {
            double precision = ComputePrecision();
            double recall = ComputeRecall();

            return 2 * precision * recall / (precision + recall);
        }
    }
}
