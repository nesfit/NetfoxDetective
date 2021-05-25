using System.Data.Entity;
using Netfox.Web.DAL.Entities;

namespace Netfox.Web.DAL
{
    public class NetfoxWebDbContext : DbContext
    {
        public NetfoxWebDbContext() : base("NetfoxWebDbContext"){}
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<CaptureStats> CapturesStats { get; set; }
        public DbSet<Investigation> Investigations { get; set; }
        public DbSet<UserInvestigation> UserInvestigation { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(e => e.Id);
            modelBuilder.Entity<User>().HasRequired(e => e.Role);
            modelBuilder.Entity<Investigation>().HasKey(q => q.Id);
            modelBuilder.Entity<Investigation>().HasRequired(q => q.ExportStats); 


            modelBuilder.Entity<UserInvestigation>().HasKey(q => new { q.UserId, q.InvestigationId });
            modelBuilder.Entity<UserInvestigation>()
                .HasRequired(t => t.User)
                .WithMany(t => t.UserInvestigations)
                .HasForeignKey(t => t.UserId)
                .WillCascadeOnDelete(true); 
            modelBuilder.Entity<UserInvestigation>()
                .HasRequired(t => t.Investigation)
                .WithMany(t => t.UserInvestigations)
                .HasForeignKey(t => t.InvestigationId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CaptureStats>()
                .HasRequired(t => t.Investigation)
                .WithMany(t => t.Stats)
                .HasForeignKey(t => t.InvestigationId)
                .WillCascadeOnDelete(true);


        }
    }
}