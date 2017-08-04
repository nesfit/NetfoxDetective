// file:	Math\Functions\Tanh.cs
//
// summary:	Implements the hyperbolic tangent class
namespace numl.Math.Functions
{
    /// <summary>A hyperbolic tangent.</summary>
    public class Tanh : Function
    {
        /// <summary>Computes the given x coordinate.</summary>
        /// <param name="x">The Vector to process.</param>
        /// <returns>A Vector.</returns>
        public override double Compute(double x)
        {
            return (this.exp(x) - this.exp(-x)) / (this.exp(x) + this.exp(-x));
        }
        /// <summary>Derivatives the given x coordinate.</summary>
        /// <param name="x">The Vector to process.</param>
        /// <returns>A Vector.</returns>
        public override double Derivative(double x)
        {
            return 1 - this.pow(this.Compute(x), 2);
        }
    }
}
