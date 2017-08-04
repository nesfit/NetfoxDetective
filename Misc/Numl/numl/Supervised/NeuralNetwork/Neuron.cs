// file:	Supervised\NeuralNetwork\Neuron.cs
//
// summary:	Implements the neuron class
using System;
using System.Linq;
using numl.Math.Functions;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml;
using numl.Utils;

namespace numl.Supervised.NeuralNetwork
{
    /// <summary>A node.</summary>
    [XmlRoot("Node"), Serializable]
    public class Node : IXmlSerializable
    {
        /// <summary>Default constructor.</summary>
        public Node()
        {
            // assume bias node unless
            // otherwise told through
            // links
            this.Output = 1d;
            this.Input = 1d;
            this.Delta = 0d;
            this.Label = String.Empty;
            this.Out = new List<Edge>();
            this.In = new List<Edge>();
            this.Id = Guid.NewGuid().ToString();
        }
        /// <summary>Gets or sets the output.</summary>
        /// <value>The output.</value>
        public double Output { get; set; }
        /// <summary>Gets or sets the input.</summary>
        /// <value>The input.</value>
        public double Input { get; set; }
        /// <summary>Gets or sets the delta.</summary>
        /// <value>The delta.</value>
        public double Delta { get; set; }
        /// <summary>Gets or sets the label.</summary>
        /// <value>The label.</value>
        public string Label { get; set; }
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public string Id { get; private set; }
        /// <summary>Gets or sets the out.</summary>
        /// <value>The out.</value>
        public List<Edge> Out { get; set; }
        /// <summary>Gets or sets the in.</summary>
        /// <value>The in.</value>
        public List<Edge> In { get; set; }
        /// <summary>Gets or sets the activation.</summary>
        /// <value>The activation.</value>
        public IFunction Activation { get; set; }
        /// <summary>Gets the evaluate.</summary>
        /// <returns>A double.</returns>
        public double Evaluate()
        {
            if (this.In.Count > 0)
            {
                this.Input = this.In.Select(e => e.Weight * e.Source.Evaluate()).Sum();
                this.Output = this.Activation.Compute(this.Input);
            }

            return this.Output;
        }
        /// <summary>Errors.</summary>
        /// <param name="t">The double to process.</param>
        /// <returns>A double.</returns>
        public double Error(double t)
        {
            // output node
            if (this.Out.Count == 0) this.Delta = this.Output - t;
            else // internal nodes
            {
                var hp = this.Activation.Derivative(this.Input);
                this.Delta = hp * this.Out.Select(e => e.Weight * e.Target.Error(t)).Sum();
            }

            return this.Delta;
        }
        /// <summary>Updates the given learningRate.</summary>
        /// <param name="learningRate">The learning rate.</param>
        public void Update(double learningRate)
        {
            foreach (Edge edge in this.In)
            {
                // for output nodes, the derivative is the Delta
                edge.Weight = learningRate * this.Delta * edge.Source.Output;
                edge.Source.Update(learningRate);
            }
        }
        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format("{0} ({1} | {2})", this.Label, this.Input, this.Output);
        }
        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable
        /// interface, you should return null (Nothing in Visual Basic) from this method, and instead, if
        /// specifying a custom schema is required, apply the
        /// <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the
        /// object that is produced by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" />
        /// method and consumed by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        /// method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }
        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is
        /// deserialized.</param>
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            this.Id = reader.GetAttribute("Id");
            this.Label = reader.GetAttribute("Label");
            this.Input = double.Parse(reader.GetAttribute("Input"));
            this.Output = double.Parse(reader.GetAttribute("Output"));
            this.Delta = double.Parse(reader.GetAttribute("Delta"));

            var activation = Ject.FindType(reader.GetAttribute("Activation"));
            this.Activation = (IFunction)Activator.CreateInstance(activation);
            this.In = new List<Edge>();
            this.Out = new List<Edge>();
        }
        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is
        /// serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Id", this.Id);
            writer.WriteAttributeString("Label", this.Label);
            writer.WriteAttributeString("Input", this.Input.ToString("r"));
            writer.WriteAttributeString("Output", this.Output.ToString("r"));
            writer.WriteAttributeString("Delta", this.Delta.ToString("r"));
            writer.WriteAttributeString("Activation", this.Activation.GetType().Name);
        }
    }
}