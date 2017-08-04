using System.Data.Entity;

namespace EntityFramework.BulkInsert.Test.Issue1369
{
    public class Issue1369Context : DbContext
    {
        public DbSet<LoanEntity> Loans { get; set; }
        public DbSet<CreditReportEntity> CreditReports { get; set; }

        public Issue1369Context()
            : base("Issue1369Context")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new LoanConfig());
            modelBuilder.Configurations.Add(new CreditReportConfig());
        }
    }
}
