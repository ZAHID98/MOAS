using Microsoft.EntityFrameworkCore;
using MOAS.Models;

namespace MOAS.Models
{
    public partial class MOASContext:DbContext
    {
        #region Setup
        public DbSet<Role> Role { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<DailySales> DailySales { get; set; }
        public DbSet<DailySalesTTY> DailySalesTTY { get; set; } 
        public DbSet<UOA_Header> UOA_Header { get; set; }
        public DbSet<UOA_Item> UOA_Item { get; set; }
        public DbSet<Equipment> Equipment { get; set; } 
        public DbSet<ResultRecord> ResultRecord { get; set; }
        public DbSet<CustomerBalance> CustomerBalance { get; set; }



        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
                
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("CoreTemplate"));
                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //for composite key model
            modelBuilder.Entity<DailySales>().HasKey(k=>new { k.vbeln,k.Posnr});
            modelBuilder.Entity<CustomerBalance>().HasKey(k => new { k.Kunnr });
            modelBuilder.Entity<UOA_Header>().HasKey(k => new { k.REQ_NO, k.INS_LOT });
            modelBuilder.Entity<UOA_Item>().HasKey(k => new { k.REQ_NO, k.SAMPLE_NO });
            modelBuilder.Entity<ResultRecord>().HasKey(k => new { k.REQ_NO, k.SAMPLE_NO,k.VERWMERKM });
           modelBuilder.Entity<DailySalesTTY>().HasKey(k => new { k.invdt, k.WorkArea,k.TP,k.ProductID,k.DeptCd });

            OnModelCreatingPartial(modelBuilder);

        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
