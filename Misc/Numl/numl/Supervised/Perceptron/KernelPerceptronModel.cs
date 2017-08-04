// file:	Supervised\Perceptron\KernelPerceptronModel.cs
//
// summary:	Implements the kernel perceptron model class
using System;
using numl.Math.Kernels;
using numl.Math.LinearAlgebra;
using System.Xml;
using numl.Model;
using numl.Utils;

namespace numl.Supervised.Perceptron
{
    /// <summary>A data Model for the kernel perceptron.</summary>
    [Serializable]
    public class KernelPerceptronModel : Model
    {
        /// <summary>Gets or sets the kernel.</summary>
        /// <value>The kernel.</value>
        public IKernel Kernel { get; set; }
        /// <summary>Gets or sets the y coordinate.</summary>
        /// <value>The y coordinate.</value>
        public Vector Y { get; set; }
        /// <summary>Gets or sets a.</summary>
        /// <value>a.</value>
        public Vector A { get; set; }
        /// <summary>Gets or sets the x coordinate.</summary>
        /// <value>The x coordinate.</value>
        public Matrix X { get; set; }

        /// <inheritdoc />
        public override Tuple<double, double> Predict(Vector y, bool p)
        {
            throw new NotImplementedException();
        }

        /// <summary>Predicts the given o.</summary>
        /// <param name="y">The Vector to process.</param>
        /// <returns>An object.</returns>
        public override double Predict(Vector y)
        {
            var K = this.Kernel.Project(this.X, y);
            double v = 0;
            for (int i = 0; i < this.A.Length; i++)
                v += this.A[i] * this.Y[i] * K[i];

            return v;
        }
        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is
        /// serialized.</param>
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Kernel", this.Kernel.GetType().Name);
            Xml.Write<Descriptor>(writer, this.Descriptor);
            Xml.Write<Vector>(writer, this.Y);
            Xml.Write<Vector>(writer, this.A);
            Xml.Write<Matrix>(writer, this.X);
        }
        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var type = Ject.FindType(reader.GetAttribute("Kernel"));
            this.Kernel = (IKernel)Activator.CreateInstance(type);
            reader.ReadStartElement();

            this.Descriptor = Xml.Read<Descriptor>(reader);
            this.Y = Xml.Read<Vector>(reader);
            this.A = Xml.Read<Vector>(reader);
            this.X = Xml.Read<Matrix>(reader);
        }
    }
}
