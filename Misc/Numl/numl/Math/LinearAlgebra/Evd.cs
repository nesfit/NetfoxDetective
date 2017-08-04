// file:	Math\LinearAlgebra\Evd.cs
//
// summary:	Implements the evd class
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace numl.Math.LinearAlgebra
{
    /// <summary>An evd.</summary>
    public class Evd
    {
        /// <summary>The Matrix to process.</summary>
        private Matrix A;
        /// <summary>The Matrix to process.</summary>
        private Matrix V;
        /// <summary>Gets the eigenvectors.</summary>
        /// <value>The eigenvectors.</value>
        public Matrix Eigenvectors
        {
            get
            {
                return this.V;
            }
        }
        /// <summary>Gets or sets the eigenvalues.</summary>
        /// <value>The eigenvalues.</value>
        public Vector Eigenvalues { get; private set; }
        /// <summary>Constructor.</summary>
        /// <param name="a">The int to process.</param>
        public Evd(Matrix a)
        {
            this.A = a.Copy();
            this.V = Matrix.Identity(this.A.Rows);
        }
        /// <summary>Offs the given a.</summary>
        /// <param name="a">The int to process.</param>
        /// <returns>A double.</returns>
        public double off(Matrix a)
        {
            double sum = 0;
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < a.Cols; j++)
                    if (i != j)
                        sum += this.sqr(a[i, j]);
            return this.sqrt(sum);
        }
        /// <summary>Schurs.</summary>
        /// <param name="a">The int to process.</param>
        /// <param name="p">The int to process.</param>
        /// <param name="q">The int to process.</param>
        /// <returns>A Tuple&lt;double,double&gt;</returns>
        private Tuple<double, double> schur(Matrix a, int p, int q)
        {
            double c, s = 0;
            if (a[p, q] != 0)
            {
                var tau = (a[q, q] - a[p, p]) / (2 * a[p, q]);
                var t = 0d;
                if (tau >= 0)
                    t = 1 / (tau + this.sqrt(tau + this.sqr(tau)));
                else
                    t = -1 / (-tau + this.sqrt(1 + this.sqr(tau)));

                c = 1 / this.sqrt(1 + this.sqr(t));
                s = t * c;
            }
            else
            {
                c = 1;
                s = 0;
            }

            return new Tuple<double, double>(c, s);
        }
        /// <summary>Sweeps.</summary>
        /// <param name="p">The int to process.</param>
        /// <param name="q">The int to process.</param>
        private void sweep(int p, int q)
        {
            // set jacobi rotation matrix
            var cs = this.schur(this.A, p, q);
            double c = cs.Item1;
            double s = cs.Item2;

            if (c != 1 || s != 0) // if rotation
            {

                /*************************
                 * perform jacobi rotation
                 *************************/
                // calculating intermediate J.T * A
                var pV = Vector.Create(this.A.Cols, i => this.A[p, i] * c + this.A[q, i] * -s);
                var qV = Vector.Create(this.A.Cols, i => this.A[q, i] * c + this.A[p, i] * s);

                // calculating A * J for inner p, q square
                var App = pV[p] * c + pV[q] * -s;
                var Apq = pV[q] * c + pV[p] * s;
                var Aqq = qV[q] * c + qV[p] * s;

                // fill in changes along box
                pV[p] = App;
                pV[q] = qV[p] = Apq;
                qV[q] = Aqq;

                /***************************
                 * store accumulated results
                 ***************************/
                var pE = Vector.Create(this.V.Rows, i => this.V[i, p] * c + this.V[i, q] * -s);
                var qE = Vector.Create(this.V.Rows, i => this.V[i, q] * c + this.V[i, p] * s);

                /****************
                 * matrix updates
                 ****************/
                // Update A
                this.A[p, VectorType.Col] = pV;
                this.A[p, VectorType.Row] = pV;
                this.A[q, VectorType.Col] = qV;
                this.A[q, VectorType.Row] = qV;

                // Update V - not critical 
                this.V[p, VectorType.Col] = pE;
                this.V[q, VectorType.Col] = qE;
            }
        }

        /// <summary>Parallels this object.</summary>
        public void parallel()
        {
            Console.WriteLine("Starting new sweep!");
            int N = this.A.Cols;
            // make even pairings
            int n = N % 2 == 0 ? N : N + 1;

            // queue up round-iness of the robin
            Queue<int> queue = new Queue<int>(n - 1);

            // fill queue
            for (int i = 1; i < N; i++) queue.Enqueue(i);
            // add extra for odd pairings
            if (N % 2 == 1) queue.Enqueue(-1);

            for (int i = 0; i < n - 1; i++)
            {
                Parallel.For(0, n / 2, j =>
                {
                    int p, q, k = n - 1 - j;

                    int eK = queue.ElementAt(k - 1);
                    int eJ = j == 0 ? 0 : queue.ElementAt(j - 1);

                    p = this.min(eJ, eK);
                    q = this.max(eJ, eK);

                    // are we in a buy week?
                    if (p >= 0) this.sweep(p, q);

                    Console.WriteLine("({0}, {1}) [{2}] {3}", p, q, Thread.CurrentThread.ManagedThreadId, p < 0 ? "buy" : "");

                });

                Console.WriteLine("----------[{0}]----------", Thread.CurrentThread.ManagedThreadId);
                // move stuff around
                queue.Enqueue(queue.Dequeue());
            }

        }

        /// <summary>Factorizes this object.</summary>
        private void factorize()
        {
            int N = this.A.Cols;
            for (int p = 0; p < N - 1; p++)
                for (int q = p + 1; q < N; q++) this.sweep(p, q);
        }
        /// <summary>Computes the given tolerance.</summary>
        /// <param name="tol">(Optional) the tolerance.</param>
        public void compute(double tol = 1.0e-10)
        {
            int s = 0;
            do
            {
                s++;
                this.factorize();
                // TODO: Fix parallelization
                //if (A.Cols <= 300) // small enough
                //    factorize();
                //else          // parallelize
                //    parallel();

            } while (this.off(this.A) > tol);

            this.sort();
        }

        /// <summary>Sorts this object.</summary>
        private void sort()
        {
            //ordering
            var eigs = this.A.Diag()
                        .Select((d, i) => new Tuple<int, double>(i, d))
                        .OrderByDescending(j => j.Item2)
                        .ToArray();

            // sort eigenvectors
            var copy = this.V.Copy();
            for (int i = 0; i < eigs.Length; i++)
                copy[i, VectorType.Col] = this.V[eigs[i].Item1, VectorType.Col];

            // normalize eigenvectors
            copy.Normalize(VectorType.Col);
            this.V = copy;

            this.Eigenvalues = eigs.Select(t => t.Item2).ToArray();
        }

        #region for brevity...
        /// <summary>Sqrts.</summary>
        /// <param name="x">The x coordinate.</param>
        /// <returns>A double.</returns>
        private double sqrt(double x)
        {
            return System.Math.Sqrt(x);
        }
        /// <summary>Sqrs.</summary>
        /// <param name="x">The x coordinate.</param>
        /// <returns>A double.</returns>
        private double sqr(double x)
        {
            return System.Math.Pow(x, 2);
        }
        /// <summary>Determines the minimum of the given parameters.</summary>
        /// <param name="a">The int to process.</param>
        /// <param name="b">The int to process.</param>
        /// <returns>The minimum value.</returns>
        private int min(int a, int b)
        {
            return System.Math.Min(a, b);
        }
        /// <summary>Determines the maximum of the given parameters.</summary>
        /// <param name="a">The int to process.</param>
        /// <param name="b">The int to process.</param>
        /// <returns>The maximum value.</returns>
        private int max(int a, int b)
        {
            return System.Math.Max(a, b);
        }
        #endregion
    }
}
