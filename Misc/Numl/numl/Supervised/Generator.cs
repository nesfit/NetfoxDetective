// file:	Supervised\Generator.cs
//
// summary:	Implements the generator class
using System;
using numl.Model;
using System.Linq;
using System.Collections.Generic;
using numl.Math.LinearAlgebra;


namespace numl.Supervised
{
    /// <summary>A generator.</summary>
    public abstract class Generator : IGenerator
    {
        /// <summary>Event queue for all listeners interested in ModelChanged events.</summary>
        public event EventHandler<ModelEventArgs> ModelChanged;
        /// <summary>Raises the model event.</summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnModelChanged(object sender, ModelEventArgs e)
        {
            EventHandler<ModelEventArgs> handler = this.ModelChanged;
            if (handler != null)
                handler(sender, e);
        }
        /// <summary>Gets or sets the descriptor.</summary>
        /// <value>The descriptor.</value>
        public Descriptor Descriptor { get; set; }
        /// <summary>Generate model based on a set of examples.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        /// <param name="examples">Example set.</param>
        /// <returns>Model.</returns>
        public IModel Generate(IEnumerable<object> examples)
        {
            if (examples.Count() == 0) throw new InvalidOperationException("Empty example set.");

            if (this.Descriptor == null) // try to generate the descriptor
                this.Descriptor = Descriptor.Create(examples.First().GetType());

            return this.Generate(this.Descriptor, examples);
        }
        /// <summary>Generate model based on a set of examples.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        /// <param name="description">The description.</param>
        /// <param name="examples">Example set.</param>
        /// <returns>Model.</returns>
        public IModel Generate(Descriptor description, IEnumerable<object> examples)
        {
            if (examples.Count() == 0) throw new InvalidOperationException("Empty example set.");

            this.Descriptor = description;
            if (this.Descriptor.Features == null || this.Descriptor.Features.Length == 0)
                throw new InvalidOperationException("Invalid descriptor: Empty feature set!");
            if (this.Descriptor.Label == null)
                throw new InvalidOperationException("Invalid descriptor: Empty label!");

            var doubles = this.Descriptor.Convert(examples);
            var tuple = doubles.ToExamples();

            return this.Generate(tuple.Item1, tuple.Item2);
        }
        /// <summary>Generates the given examples.</summary>
        /// <tparam name="T">Generic type parameter.</tparam>
        /// <param name="examples">Example set.</param>
        /// <returns>An IModel.</returns>
        public IModel Generate<T>(IEnumerable<T> examples)
             where T : class
        {
            var descriptor = Descriptor.Create<T>();
            return this.Generate(descriptor, examples);
        }
        /// <summary>Generate model based on a set of examples.</summary>
        /// <param name="x">The Matrix to process.</param>
        /// <param name="y">The Vector to process.</param>
        /// <returns>Model.</returns>
        public abstract IModel Generate(Matrix x, Vector y);
    }

    /// <summary>Additional information for model events.</summary>
    public class ModelEventArgs : EventArgs
    {
        /// <summary>Constructor.</summary>
        /// <param name="model">The model.</param>
        /// <param name="message">(Optional) the message.</param>
        public ModelEventArgs(IModel model, string message = "")
        {
            this.Message = message;
            this.Model = model;
        }
        /// <summary>Gets or sets the model.</summary>
        /// <value>The model.</value>
        public IModel Model { get; private set; }
        /// <summary>Gets or sets the message.</summary>
        /// <value>The message.</value>
        public string Message { get; private set; }
        /// <summary>Makes.</summary>
        /// <param name="model">The model.</param>
        /// <param name="message">(Optional) the message.</param>
        /// <returns>The ModelEventArgs.</returns>
        internal static ModelEventArgs Make(IModel model, string message = "")
        {
            return new ModelEventArgs(model, message);
        }
    }
}
