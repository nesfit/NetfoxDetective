using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
#if EF4
using System.ComponentModel.DataAnnotations;
#endif
#if EF6 || EF5
#endif

namespace EntityFramework.BulkInsert.Test.Issue1369
{
    public class LoanEntity
    {
        public Guid Id { get; set; }
        public byte[] Timestamp { get; set; }

        public string MarketplaceLoanId { get; set; }
        public string MarketplaceMemberId { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal FundedAmount { get; set; }
        public int Term { get; set; } //36 or 60
        public decimal InterestRate { get; set; }
        public decimal ExpectedDefaultRate { get; set; }
        public decimal ServiceFeeRate { get; set; }
        public decimal? Installment { get; set; }
        public string Grade { get; set; }
        public string Subgrade { get; set; }
        public string EmploymentTitle { get; set; }
        public int? EmploymentLength { get; set; }
        public string HomeOwnership { get; set; }
        public string OtherHomeOwnership { get; set; }
        public decimal AnnualIncome { get; set; }
        public string IncomeVerification { get; set; }
        public DateTime? AcceptDate { get; set; } //date borrower accepted the offer
        public DateTime ListDate { get; set; } //date loan was listed
        public DateTime CreditPullDate { get; set; }
        public DateTime? ReviewStatusDate { get; set; }
        public string ReviewStatus { get; set; }
        public string Url { get; set; }
        public string BorrowerLoanDescription { get; set; }
        public string Purpose { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Msa { get; set; } //metropolitan statisical area of the borrower
        public int? InvestorCount { get; set; }
        public DateTime? InitialListingExpirationDate { get; set; }
        public string InitialListingStatus { get; set; } //W or F for whole or fractional

        public DateTime EffectiveDate { get; set; }

        //public MarketplaceEntity Marketplace { get; set; }

        public IList<CreditReportEntity> CreditReports { get; set; }

        public LoanEntity()
        {
            this.CreditReports = new List<CreditReportEntity>();
        }
    }

    public class LoanConfig : EntityTypeConfiguration<LoanEntity>
    {
        public LoanConfig()
        {
            this.ToTable("Loans");

            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(x => x.Timestamp).IsRowVersion();

            this.Property(x => x.MarketplaceLoanId).HasMaxLength(20).HasColumnType("varchar").IsRequired();
            this.Property(x => x.MarketplaceMemberId).HasMaxLength(20).HasColumnType("varchar").IsRequired();
            this.Property(x => x.LoanAmount).HasColumnType("money").IsRequired();
            this.Property(x => x.FundedAmount).HasColumnType("money").IsRequired();
            this.Property(x => x.InterestRate).HasColumnType("money").IsRequired();
            this.Property(x => x.ExpectedDefaultRate).HasColumnType("money").IsRequired();
            this.Property(x => x.ServiceFeeRate).HasColumnType("money").IsRequired();
            this.Property(x => x.Installment).HasColumnType("money").IsOptional();
            this.Property(x => x.Grade).HasMaxLength(2).HasColumnType("varchar").IsRequired();
            this.Property(x => x.Subgrade).HasMaxLength(2).HasColumnType("varchar").IsRequired();
            this.Property(x => x.EmploymentTitle).HasMaxLength(50).HasColumnType("varchar").IsOptional();
            this.Property(x => x.EmploymentLength).IsOptional();
            this.Property(x => x.HomeOwnership).HasMaxLength(20).HasColumnType("varchar").IsRequired();
            this.Property(x => x.OtherHomeOwnership).HasMaxLength(50).HasColumnType("varchar").IsOptional();
            this.Property(x => x.AnnualIncome).HasColumnType("money").IsRequired();
            this.Property(x => x.AcceptDate).IsOptional();
            this.Property(x => x.ReviewStatusDate).IsOptional();
            this.Property(x => x.ReviewStatus).HasMaxLength(20).HasColumnType("varchar").IsRequired();
            this.Property(x => x.Url).HasMaxLength(100).HasColumnType("varchar").IsRequired();
            this.Property(x => x.BorrowerLoanDescription).HasMaxLength(1000).HasColumnType("varchar").IsOptional();
            this.Property(x => x.Purpose).HasMaxLength(20).HasColumnType("varchar").IsRequired();
            this.Property(x => x.Title).HasMaxLength(50).HasColumnType("varchar").IsRequired();
            this.Property(x => x.City).HasMaxLength(50).HasColumnType("varchar").IsRequired();
            this.Property(x => x.State).HasMaxLength(2).HasColumnType("varchar").IsRequired();
            this.Property(x => x.Msa).HasMaxLength(2).HasColumnType("varchar").IsOptional();
            this.Property(x => x.InvestorCount).IsOptional();
            this.Property(x => x.InitialListingExpirationDate).IsOptional();
            this.Property(x => x.InitialListingStatus).HasMaxLength(1).HasColumnType("varchar").IsRequired();
        }
    }
}