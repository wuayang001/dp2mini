using DigitalPlatform.CirculationClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2mini
{
    public class EntityDB : DbContext
    {
        string _dir = "";
        public EntityDB(string dir) {
            _dir = dir;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Set the filename of the database to be created
            string filePath = this._dir+ "/db.sqlite";
            optionsBuilder.UseSqlite("Data Source="+ filePath);
        }


        public DbSet<Entity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entity>(b =>
            {
                b.HasKey(e => e.path);
                b.ToTable("entity");
            });


        }
    }

}
