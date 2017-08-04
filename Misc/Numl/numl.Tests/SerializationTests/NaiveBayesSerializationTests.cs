using numl.Model;
using numl.Tests.Data;
using NUnit.Framework;
using numl.Supervised.NaiveBayes;

namespace numl.Tests.SerializationTests
{
    [TestFixture, Category("Serialization")]
    public class NaiveBayesSerializationTests : BaseSerialization
    {
        [Test][Explicit][Category("Explicit")]
        public void Tennis_Naive_Bayes_Save_And_Load_Test()
        {
            var data = Tennis.GetData();
            var description = Descriptor.Create<Tennis>();
            var generator = new NaiveBayesGenerator(2);
            var model = generator.Generate(description, data) as NaiveBayesModel;

            Serialize(model);

            var lmodel = Deserialize<NaiveBayesModel>();
            Assert.AreEqual(model.Root, lmodel.Root);
        }

        [Test][Explicit][Category("Explicit")]
        public void Iris_Naive_Bayes_Save_And_Load_Test()
        {
            var data = Iris.Load();
            var description = Descriptor.Create<Iris>();
            var generator = new NaiveBayesGenerator(2);
            var model = generator.Generate(description, data) as NaiveBayesModel;

            Serialize(model);

            var lmodel = Deserialize<NaiveBayesModel>();
            Assert.AreEqual(model.Root, lmodel.Root);
        }
    }
}
