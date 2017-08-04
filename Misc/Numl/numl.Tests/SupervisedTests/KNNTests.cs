using NUnit.Framework;
using numl.Supervised.KNN;

namespace numl.Tests.SupervisedTests
{
    [TestFixture, Category("Supervised")]
    public class KNNTests : BaseSupervised
    {
        [Test][Explicit][Category("Explicit")]
        public void Tennis_Tests()
        {
            TennisPrediction(new KNNGenerator());
        }

        [Test][Explicit][Category("Explicit")]
        public void House_Tests()
        {
            HousePrediction(new KNNGenerator());
        }

        [Test][Explicit][Category("Explicit")]
        public void Iris_Tests()
        {
            IrisPrediction(new KNNGenerator());
        }

        [Test][Explicit][Category("Explicit")]
        public void Tennis_Learner_Tests()
        {
            TennisLearnerPrediction(new KNNGenerator());
        }

        [Test][Explicit][Category("Explicit")]
        public void House_Learner_Tests()
        {
            HouseLearnerPrediction(new KNNGenerator());
        }

        [Test][Explicit][Category("Explicit")]
        public void Iris_Learner_Tests()
        {
            IrisLearnerPrediction(new KNNGenerator());
        }
    }
}
