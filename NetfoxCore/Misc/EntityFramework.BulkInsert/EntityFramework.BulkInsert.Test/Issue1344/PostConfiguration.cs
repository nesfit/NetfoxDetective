using System.Data.Entity.ModelConfiguration;

namespace EntityFramework.BulkInsert.Test.Issue1344
{
    public class PostConfiguration : EntityTypeConfiguration<Post>
    {
        public PostConfiguration()
        {
            this.ToTable("Accrual");

            this.Property(p => p.ResourceId)
                .HasColumnName("PersonalAccount");

            this.Property(p => p.StartDate)
                .HasColumnName("DateFrom");

            this.Property(p => p.EndDate)
                .HasColumnName("DateTo");

            this.Property(p => p.WageTypeId)
                .HasColumnName("Surcharges");

            this.Property(p => p.SubdivisionId)
                .HasColumnName("CostDepartment");

            this.Property(p => p.JobCategoryId)
                .HasColumnName("Category");

            this.Property(p => p.CalculationMonth)
                .HasColumnName("MonthInWhichTheAccrued");

            this.Property(p => p.CalculationYear)
                .HasColumnName("YearInWhichTheAccrued");

            this.Property(p => p.ActivityMonth)
                .HasColumnName("MonthForWhichAccrued");

            this.Property(p => p.ActivityYear)
                .HasColumnName("YearForWhichAccrued");

            this.Property(p => p.PlannedDays)
                .HasColumnName("DaysGr");

            this.Property(p => p.PlannedHours)
                .HasColumnName("HoursGr");

            this.Property(p => p.WorkedDays)
                .HasColumnName("DaysFact");

            this.Property(p => p.WorkedHours)
                .HasColumnName("HoursFact");

            this.Property(p => p.AppliedPercentage)
                .HasColumnName("Percent");

            this.Property(p => p.Amount)
                .HasColumnName("Amount");

            this.Property(p => p.PaymentMonth)
                .HasColumnName("MonthInWhichPaid");

            this.Property(p => p.PaymentYear)
                .HasColumnName("YearInWhichPaid");

            this.Property(p => p.TaxDeductionTypeId)
                .HasColumnName("Deduction");

            this.Property(p => p.TaxDeductionAmount)
                .HasColumnName("DeductionSumma");

            this.Property(p => p.AppliedTaxDeductionPercentage)
                .HasColumnName("DeductionPercent");
        }


    }
}
