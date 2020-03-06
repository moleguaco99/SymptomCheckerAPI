using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLicenta.Models
{
    public class MedicalDBContext : DbContext
    {
        public MedicalDBContext(DbContextOptions<MedicalDBContext> dbContextOptions) : base(dbContextOptions) { }

        public DbSet<Disease> Diseases { get; set; }
        public DbSet<Symptom> Symptoms { get; set; }
        public DbSet<SymptomDisease> SymptomDiseases { get; set; }
    }
}
