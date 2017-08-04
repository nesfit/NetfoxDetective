// file:	Supervised\NeuralNetwork\NeuralNetworkGenerator.cs
//
// summary:	Implements the neural network generator class
using System;
using numl.Math.LinearAlgebra;
using numl.Math.Functions;

namespace numl.Supervised.NeuralNetwork
{
    /// <summary>A neural network generator.</summary>
    public class NeuralNetworkGenerator : Generator
    {
        /// <summary>Gets or sets the learning rate.</summary>
        /// <value>The learning rate.</value>
        public double LearningRate { get; set; }
        /// <summary>Gets or sets the maximum iterations.</summary>
        /// <value>The maximum iterations.</value>
        public int MaxIterations { get; set; }
        /// <summary>Gets or sets the activation.</summary>
        /// <value>The activation.</value>
        public IFunction Activation { get; set; }

        /// <summary>Default constructor.</summary>
        public NeuralNetworkGenerator()
        {
            this.LearningRate = 0.9;
            this.MaxIterations = -1;
            this.Activation = new Tanh();
        }
        /// <summary>Generate model based on a set of examples.</summary>
        /// <param name="x">The Matrix to process.</param>
        /// <param name="y">The Vector to process.</param>
        /// <returns>Model.</returns>
        public override IModel Generate(Matrix x, Vector y)
        {
            // because I said so...
            if (this.MaxIterations == -1) this.MaxIterations = x.Rows * 1000;

            var network = Network.Default(this.Descriptor, x, y, this.Activation);
            var model = new NeuralNetworkModel { Descriptor = this.Descriptor, Network = network };
            this.OnModelChanged(this, ModelEventArgs.Make(model, "Initialized"));

            for (int i = 0; i < this.MaxIterations; i++)
            {
                int idx = i % x.Rows;
                network.Forward(x[idx, VectorType.Row]);
                //OnModelChanged(this, ModelEventArgs.Make(model, "Forward"));
                network.Back(y[idx], this.LearningRate);
                var output = String.Format("Run ({0}/{1})", i, this.MaxIterations);
                this.OnModelChanged(this, ModelEventArgs.Make(model, output));
            }

            return model;
        }
    }
}
