using System;
using System.Xml;

using numl.Utils;
using numl.Model;
using numl.Math.Functions;
using numl.Math.LinearAlgebra;

namespace numl.Supervised.Regression
{
    /// <summary>
    /// A Logistic Regression Model object
    /// </summary>
    [Serializable]
    public class LogisticRegressionModel : Model
    {
        /// <summary>
        /// Theta parameters vector mapping X to y.
        /// </summary>
        public Vector Theta { get; set; }

        /// <summary>
        /// Logistic function
        /// </summary>
        public IFunction LogisticFunction { get; set; }

        /// <summary>
        /// The additional number of quadratic features to create as used in generating the model
        /// </summary>
        public int PolynomialFeatures { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LogisticRegressionModel()
        {
            this.PolynomialFeatures = 0;
        }

        /// <inheritdoc />
        public override Tuple<double, double> Predict(Vector y, bool p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a prediction based on the learned Theta values and the supplied test item.
        /// </summary>
        /// <param name="y">Training record</param>
        /// <returns></returns>
        public override double Predict(Vector y)
        {
            var tempy = this.PolynomialFeatures > 0 ? PreProcessing.FeatureDimensions.IncreaseDimensions(y, this.PolynomialFeatures) : y;
            tempy = tempy.Insert(0, 1.0);
            return this.LogisticFunction.Compute((tempy * this.Theta).ToDouble()) >= 0.5 ? 1d : 0d;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            var sigmoid = Ject.FindType(reader.GetAttribute("LogisticFunction"));
            this.LogisticFunction = (IFunction)Activator.CreateInstance(sigmoid);

            reader.ReadStartElement();

            this.Descriptor = Xml.Read<Descriptor>(reader);
            this.Theta = Xml.Read<Vector>(reader);
            this.PolynomialFeatures = Xml.Read<int>(reader);
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is
        /// serialized.</param>
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("LogisticFunction", this.LogisticFunction.GetType().Name);

            Xml.Write<Descriptor>(writer, this.Descriptor);
            Xml.Write<Vector>(writer, this.Theta);
            Xml.Write<int>(writer, this.PolynomialFeatures);
        }
    }
}
