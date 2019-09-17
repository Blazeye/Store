using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Store.Domain.Entities;

namespace Store.Domain.Concrete
{
    public class EFDbContext : DbContext
    {

        public EFDbContext() : base("tiny") // Look in Web.config for a connectionString with this name
        {
            Database.SetInitializer<EFDbContext>(null); // Disables "code-first" database creation
        }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           // modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
