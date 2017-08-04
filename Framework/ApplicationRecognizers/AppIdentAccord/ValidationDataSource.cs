namespace AppIdentAccord
{
    public partial class AccordAppIdent
    {
        public class ValidationDataSource
        {
            public ValidationDataSource(double[][] validationInputs, int[] validationOutputs)
            {
                this.ValidationOutputs = validationOutputs;
                this.ValidationInputs = validationInputs;
            }
            public double[][] ValidationInputs { get; set; }
            public int[] ValidationOutputs { get; set; }
        }
    }
}