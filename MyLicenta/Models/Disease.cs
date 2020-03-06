using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.Models
{
    public class Disease
    {
        public int Id { get; set; }
        public string DiseaseName { get; set; }
        public double GeneralProbability { get; set; }
    }
}
