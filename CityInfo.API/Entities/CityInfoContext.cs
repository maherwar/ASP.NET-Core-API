using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Entities
{
    public class CityInfoContext : DbContext
    {
        //overloading the constructor of DbContext
        //good approach for multiple services and dependencies
        public CityInfoContext(DbContextOptions<CityInfoContext> options)
            : base(options)
        {
            Database.EnsureCreated();
            //to migrate the data
            //Database.Migrate();
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<PointOfInterest> PointsOfInterest { get; set; }

        //this is tightly couple - Bad choice
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("connectionstring");

        //    base.OnConfiguring(optionsBuilder);
        //}
    }
}
