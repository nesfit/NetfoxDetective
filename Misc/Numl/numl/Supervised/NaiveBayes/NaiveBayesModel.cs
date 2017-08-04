// file:	Supervised\NaiveBayes\NaiveBayesModel.cs
//
// summary:	Implements the naive bayes model class
using System;
using numl.Model;
using System.Xml;
using numl.Math.LinearAlgebra;
using numl.Utils;

namespace numl.Supervised.NaiveBayes
{
    /// <summary>A data Model for the naive bayes.</summary>
    public class NaiveBayesModel : Model
    {
        /// <summary>Gets or sets the root.</summary>
        /// <value>The root.</value>
        public Measure Root { get; set; }
        /// <summary>Predicts the given o.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        /// <param name="y">The Vector to process.</param>
        /// <returns>An object.</returns>
        public override double Predict(Vector y)
        {
            if (this.Root == null || this.Descriptor == null)
                throw new InvalidOperationException("Invalid Model - Missing information");

            Vector lp = Vector.Zeros(this.Root.Probabilities.Length);
            for (int i = 0; i < this.Root.Probabilities.Length; i++)
            {
                Statistic stat = this.Root.Probabilities[i];
                lp[i] = System.Math.Log(stat.Probability);
                for (int j = 0; j < y.Length; j++)
                {
                    Measure conditional = stat.Conditionals[j];
                    var p = conditional.GetStatisticFor(y[j]);
                    // check for missing range, assign bad probability
                    lp[i] += System.Math.Log(p == null ? 10e-10 : p.Probability);
                }
            }
            var idx = lp.MaxIndex();
            return this.Root.Probabilities[idx].X.Min;
        }

        /// <inheritdoc />
        public override Tuple<double, double> Predict(Vector y, bool t)
        {
            if (this.Root == null || this.Descriptor == null)
                throw new InvalidOperationException("Invalid Model - Missing information");

            Vector lp = Vector.Zeros(this.Root.Probabilities.Length);
            Vector test = Vector.Zeros(this.Root.Probabilities.Length);
            Vector log = Vector.Zeros(this.Root.Probabilities.Length);
            for (int i = 0; i < this.Root.Probabilities.Length; i++)
            {
                Statistic stat = this.Root.Probabilities[i];
                lp[i] = (stat.Probability);
                test[i] = (stat.Probability);
                log[i] = System.Math.Log(stat.Probability);
                for (int j = 0; j < y.Length; j++)
                {
                    Measure conditional = stat.Conditionals[j];
                    var p = conditional.GetStatisticFor(y[j]);
                    // check for missing range, assign bad probability
                    lp[i] *= (p == null ? 10e-10 : p.Probability);
                    test[i] += (p == null ? 10e-10 : p.Probability);
                    log[i] += System.Math.Log(p == null ? 10e-10 : p.Probability);
                }
            }
            //for (int i = 0; i < lp.Length; i++)
            //    Console.WriteLine("Prob " +i+ "*: " + lp[i] + "\t +:" + test[i] + "\t Log +:" + log[i]);

            var idx = lp.MaxIndex();
            var precision = lp[idx]/lp.Sum();
            return new Tuple<double, double>(this.@Root.Probabilities[idx].X.Min, precision);
        }
        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            reader.ReadStartElement();
            this.Descriptor = Xml.Read<Descriptor>(reader);
            this.Root = Xml.Read<Measure>(reader);
        }
        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is
        /// serialized.</param>
        public override void WriteXml(XmlWriter writer)
        {
            Xml.Write<Descriptor>(writer, this.Descriptor);
            Xml.Write<Measure>(writer, this.Root);
        }
    }
}
