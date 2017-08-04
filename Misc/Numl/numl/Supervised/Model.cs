// file:	Supervised\Model.cs
//
// summary:	Implements the model class
using System;
using System.IO;
using numl.Utils;
using numl.Model;
using numl.Math.LinearAlgebra;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace numl.Supervised
{
    /// <summary>A model.</summary>
    public abstract class Model : IModel, IXmlSerializable
    {
        /// <summary>Gets or sets the descriptor.</summary>
        /// <value>The descriptor.</value>
        public Descriptor Descriptor { get; set; }
        /// <summary>Predicts the given o.</summary>
        /// <param name="y">The Vector to process.</param>
        /// <returns>An object.</returns>
        public abstract double Predict(Vector y);
        /// <summary>Predicts the given o.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        /// <param name="o">The object to process.</param>
        /// <returns>An object.</returns>
        public object Predict(object o)
        {
            if (this.Descriptor.Label == null)
                throw new InvalidOperationException("Empty label precludes prediction!");

            var y = this.Descriptor.Convert(o, false).ToVector();
            var val = this.Predict(y);
            var result = this.Descriptor.Label.Convert(val);
            Ject.Set(o, this.Descriptor.Label.Name, result);
            return o;
        }
        /// <summary>Predicts the given o.</summary>
        /// <tparam name="T">Generic type parameter.</tparam>
        /// <param name="o">The object to process.</param>
        /// <returns>A T.</returns>
        public T Predict<T>(T o)
        {
            return (T) this.Predict((object)o);
        }

        /// <inheritdoc />
        public abstract Tuple<double, double> Predict(Vector y, bool p);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public object Predict(object o, bool p)
        {
            if (this.Descriptor.Label == null)
                throw new InvalidOperationException("Empty label precludes prediction!");

            var y = this.Descriptor.Convert(o, false).ToVector();
            var val = this.Predict(y, p);
            var result = this.Descriptor.Label.Convert(val.Item1);
            Ject.Set(o, this.Descriptor.Label.Name, result);
            Ject.Set(o, "Precision", val.Item2);
            
            return o;
        }

        /// <inheritdoc />
        public T Predict<T>(T o, bool p)
        {
            return (T) this.Predict((object)o, p);
        }

        // ----- saving stuff
        /// <summary>Model persistance.</summary>
        /// <param name="file">The file to load.</param>
        public virtual void Save(string file)
        {
            Xml.Save(file, this, this.GetType());
        }
        /// <summary>Saves the given stream.</summary>
        /// <param name="stream">The stream to load.</param>
        public virtual void Save(Stream stream)
        {
            Xml.Save(stream, this, this.GetType());
        }
        /// <summary>Converts this object to an XML.</summary>
        /// <returns>This object as a string.</returns>
        public virtual string ToXml()
        {
            return Xml.ToXmlString(this, this.GetType());
        }
        /// <summary>Loads the given stream.</summary>
        /// <param name="file">The file to load.</param>
        /// <returns>An IModel.</returns>
        public virtual IModel Load(string file)
        {
            return (IModel)Xml.Load(file, this.GetType());
        }
        /// <summary>Loads the given stream.</summary>
        /// <param name="stream">The stream to load.</param>
        /// <returns>An IModel.</returns>
        public virtual IModel Load(Stream stream)
        {
            return (IModel)Xml.Load(stream, this.GetType());
        }
        /// <summary>Loads an XML.</summary>
        /// <param name="xml">The XML.</param>
        /// <returns>The XML.</returns>
        public virtual IModel LoadXml(string xml)
        {
            return (IModel)Xml.LoadXmlString(xml, this.GetType());
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
        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is
        /// serialized.</param>
        public abstract void WriteXml(XmlWriter writer);
        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is
        /// deserialized.</param>
        public abstract void ReadXml(XmlReader reader);

    }
}
