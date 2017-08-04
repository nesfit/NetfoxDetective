using numl.Model;
using numl.Tests.Data;
using NUnit.Framework;
using numl.Math.Linkers;
using numl.Math.Metrics;
using numl.Unsupervised;

namespace numl.Tests.UnsupervisedTests
{
    [TestFixture, Category("Unsupervised")]
    public class HierarchicalClusteringTests
    {
        [Test][Explicit][Category("Explicit")]
        public void Cluster_Student()
        {
            Student[] students = Student.GetData();
            HClusterModel cluster = new HClusterModel();
            Descriptor descriptor = Descriptor.Create<Student>();
            CentroidLinker linker = new CentroidLinker(new EuclidianDistance());
            Cluster root = cluster.Generate(descriptor, students, linker);
        }
    }
}
