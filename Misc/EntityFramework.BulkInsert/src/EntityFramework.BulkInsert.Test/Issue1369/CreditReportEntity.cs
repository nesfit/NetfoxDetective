using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
#if EF4
using System.ComponentModel.DataAnnotations;
#endif
#if EF6 || EF5
#endif

namespace EntityFramework.BulkInsert.Test.Issue1369
{
    public class CreditReportEntity
    {
        public Guid Id { get; set; }
        public byte[] Timestamp { get; set; }

        public int? AccountsNowDelinq { get; set; }
        public int? AccountsOpenedPast24Months { get; set; }
        public decimal? BankCardsOpenToBuy { get; set; }
        public decimal? PercentBankCardsOver75PercentOfLimit { get; set; }
        public decimal? BankCardUtiliziation { get; set; }
        public decimal? DebtToIncomeRatio { get; set; }
        public int? DelinqPast2Years { get; set; }
        public decimal? DelinqAmount { get; set; }
        public DateTime? EarliestCreditLine { get; set; }
        public int? FicoRangeLow { get; set; }
        public int? FicoRangeHigh { get; set; }
        public int? InquiriesLast6Months { get; set; }
        public int? MonthsSinceLastDelinq { get; set; }
        public int? MonthsSinceLastPublicRecord { get; set; }
        public int? MonthsSinceMostRecentInquiry { get; set; }
        public int? MonthsSinceMostRecentRevolvingDelinq { get; set; }
        public int? MonthsSinceMostRecentBankCardOpened { get; set; }
        public int? MortgageAccounts { get; set; }
        public int? OpenCreditLines { get; set; }
        public int? PublicRecords { get; set; }
        public decimal? TotalCreditBalanceExcludingMortgage { get; set; }
        public decimal? TotalCreditRevolvingBalance { get; set; }
        public decimal? RevolvingUtilizationRate { get; set; }
        public decimal? TotalBankCardCreditLimit { get; set; }
        public int? TotalCreditLines { get; set; }
        public decimal? TotalInstallmentCreditLimit { get; set; }
        public int? RevolvingAccounts { get; set; }
        public int? MonthsSinceMostRecentBankCardDelinq { get; set; }
        public int? PublicRecordBankruptcies { get; set; }
        public int? AccountsEver120DaysPastDue { get; set; }
        public int? ChargeOffsWithin12Months { get; set; }
        public int? CollectionsIn12MonthsExcludingMedical { get; set; }
        public int? TaxLiens { get; set; }
        public int? MonthsSinceLastMajorDerogatory { get; set; }
        public int? SatisfactoryAccounts { get; set; }
        public int? AccountsOpenedPast12Months { get; set; }
        public int? MonthsSinceMostRecentAccountOpened { get; set; }
        public decimal? TotalCreditLimit { get; set; }
        public decimal? TotalCurrentBalanceAllAccounts { get; set; }
        public decimal? AverageCurrentBalanceAllAccounts { get; set; }
        public int? BankCardAccounts { get; set; }
        public int? ActiveBankCardAccounts { get; set; }
        public int? SatisfactoryBankCardAccounts { get; set; }
        public decimal? PercentTradesNeverDelinq { get; set; }
        public int? Accounts90DaysPastDueLast24Months { get; set; }
        public int? Accounts30DaysPastDueLast2Months { get; set; }
        public int? Accounts120DaysPastDueLast2Months { get; set; }
        public int? InstallmentAccounts { get; set; }
        public int? MonthsSinceOldestInstallmentAccountOpened { get; set; }
        public int? CurrentlyActiveRevolvingTrades { get; set; }
        public int? MonthsSinceOldestRevolvingAccountOpened { get; set; }
        public int? MonthsSinceMostRecentRevolvingAccountOpened { get; set; }
        public decimal? TotalRevolvingCreditLimit { get; set; }
        public int? RevolvingTradesWithPositiveBalance { get; set; }
        public int? OpenRevolvingAccounts { get; set; }
        public decimal? TotalCollectionAmountsEverOwed { get; set; }

        public LoanEntity Loan { get; set; }

        public CreditReportEntity()
        {
        }
    }

    public class CreditReportConfig : EntityTypeConfiguration<CreditReportEntity>
    {
        public CreditReportConfig()
        {
            this.ToTable("CreditReports");

            this.HasKey(x => x.Id);
            this.Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(x => x.Timestamp).IsRowVersion();

            this.Property(x => x.AccountsNowDelinq).IsOptional();
            this.Property(x => x.AccountsOpenedPast24Months).IsOptional();
            this.Property(x => x.BankCardsOpenToBuy).IsOptional();
            this.Property(x => x.PercentBankCardsOver75PercentOfLimit).IsOptional();
            this.Property(x => x.BankCardUtiliziation).IsOptional();
            this.Property(x => x.DebtToIncomeRatio).IsOptional();
            this.Property(x => x.DelinqPast2Years).IsOptional();
            this.Property(x => x.DelinqAmount).IsOptional();
            this.Property(x => x.EarliestCreditLine).IsOptional();
            this.Property(x => x.FicoRangeLow).IsOptional();
            this.Property(x => x.FicoRangeHigh).IsOptional();
            this.Property(x => x.InquiriesLast6Months).IsOptional();
            this.Property(x => x.MonthsSinceLastDelinq).IsOptional();
            this.Property(x => x.MonthsSinceLastPublicRecord).IsOptional();
            this.Property(x => x.MonthsSinceMostRecentInquiry).IsOptional();
            this.Property(x => x.MonthsSinceMostRecentRevolvingDelinq).IsOptional();
            this.Property(x => x.MonthsSinceMostRecentBankCardOpened).IsOptional();
            this.Property(x => x.MortgageAccounts).IsOptional();
            this.Property(x => x.OpenCreditLines).IsOptional();
            this.Property(x => x.PublicRecords).IsOptional();
            this.Property(x => x.TotalCreditBalanceExcludingMortgage).IsOptional();
            this.Property(x => x.TotalCreditRevolvingBalance).IsOptional();
            this.Property(x => x.RevolvingUtilizationRate).IsOptional();
            this.Property(x => x.TotalBankCardCreditLimit).IsOptional();
            this.Property(x => x.TotalCreditLines).IsOptional();
            this.Property(x => x.TotalInstallmentCreditLimit).IsOptional();
            this.Property(x => x.RevolvingAccounts).IsOptional();
            this.Property(x => x.MonthsSinceMostRecentBankCardDelinq).IsOptional();
            this.Property(x => x.PublicRecordBankruptcies).IsOptional();
            this.Property(x => x.AccountsEver120DaysPastDue).IsOptional();
            this.Property(x => x.ChargeOffsWithin12Months).IsOptional();
            this.Property(x => x.CollectionsIn12MonthsExcludingMedical).IsOptional();
            this.Property(x => x.TaxLiens).IsOptional();
            this.Property(x => x.MonthsSinceLastMajorDerogatory).IsOptional();
            this.Property(x => x.SatisfactoryAccounts).IsOptional();
            this.Property(x => x.AccountsOpenedPast12Months).IsOptional();
            this.Property(x => x.MonthsSinceMostRecentAccountOpened).IsOptional();
            this.Property(x => x.TotalCreditLimit).IsOptional();
            this.Property(x => x.TotalCurrentBalanceAllAccounts).IsOptional();
            this.Property(x => x.AverageCurrentBalanceAllAccounts).IsOptional();
            this.Property(x => x.BankCardAccounts).IsOptional();
            this.Property(x => x.ActiveBankCardAccounts).IsOptional();
            this.Property(x => x.SatisfactoryBankCardAccounts).IsOptional();
            this.Property(x => x.PercentTradesNeverDelinq).IsOptional();
            this.Property(x => x.Accounts90DaysPastDueLast24Months).IsOptional();
            this.Property(x => x.Accounts30DaysPastDueLast2Months).IsOptional();
            this.Property(x => x.Accounts120DaysPastDueLast2Months).IsOptional();
            this.Property(x => x.InstallmentAccounts).IsOptional();
            this.Property(x => x.MonthsSinceOldestInstallmentAccountOpened).IsOptional();
            this.Property(x => x.CurrentlyActiveRevolvingTrades).IsOptional();
            this.Property(x => x.MonthsSinceOldestRevolvingAccountOpened).IsOptional();
            this.Property(x => x.MonthsSinceMostRecentRevolvingAccountOpened).IsOptional();
            this.Property(x => x.TotalRevolvingCreditLimit).IsOptional();
            this.Property(x => x.RevolvingTradesWithPositiveBalance).IsOptional();
            this.Property(x => x.OpenRevolvingAccounts).IsOptional();
            this.Property(x => x.TotalCollectionAmountsEverOwed).IsOptional();
        }
    }
}
