// file:	Supervised\KNN\KNNModel.cs
//
// summary:	Implements the knn model class
using System;
using System.Linq;
using System.Threading.Tasks;
using numl.Math.LinearAlgebra;
using System.Xml;
using numl.Model;
using numl.Utils;

namespace numl.Supervised.KNN
{
    /// <summary>A data Model for the knn.</summary>
    [Serializable]
    public class KNNModel : Model
    {
        /// <summary>Gets or sets the k.</summary>
        /// <value>The k.</value>
        public int K { get; set; }
        /// <summary>Gets or sets the x coordinate.</summary>
        /// <value>The x coordinate.</value>
        public Matrix X { get; set; }
        /// <summary>Gets or sets the y coordinate.</summary>
        /// <value>The y coordinate.</value>
        public Vector Y { get; set; }

        /// <inheritdoc />
        public override Tuple<double, double> Predict(Vector y , bool p)
        {
            throw new NotImplementedException();
        }

        /// <summary>Predicts the given o.</summary>
        /// <param name="y">The Vector to process.</param>
        /// <returns>An object.</returns>
        public override double Predict(Vector y)
        {
            Tuple<int, double>[] distances = new Tuple<int, double>[this.X.Rows];

            // happens per slot so we are good to parallelize
            Parallel.For(0, this.X.Rows, i => distances[i] = new Tuple<int, double>(i, (y - this.X.Row(i)).Norm(2)));

            var slice = distances
                            .OrderBy(t => t.Item2)
                            .Take(this.K)
                            .Select(i => i.Item1);

            return this.Y.Slice(slice).Mode();
        }
        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is
        /// serialized.</param>
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("K", this.K.ToString("r"));
            Xml.Write<Descriptor>(writer, this.Descriptor);
            Xml.Write<Matrix>(writer, this.X);
            Xml.Write<Vector>(writer, this.Y);

        }
        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            this.K = int.Parse(reader.GetAttribute("K"));
            reader.ReadStartElement();

            this.Descriptor = Xml.Read<Descriptor>(reader);
            this.X = Xml.Read<Matrix>(reader);
            this.Y = Xml.Read<Vector>(reader);
        }

        
    }
}
