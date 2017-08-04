using numl.Supervised.NeuralNetwork;
using numl.Tests.Data;
using numl.Tests.SupervisedTests;
using NUnit.Framework;

namespace numl.Tests.SerializationTests
{
    [TestFixture, Category("Serialization")]
    public class NeuralNetworkSerializationTests : BaseSerialization
    {
        [Test][Explicit][Category("Explicit")]
        public void Save_Node_Test()
        {
            
            Tennis t = new Tennis
            {
                Humidity = Humidity.Normal,
                Outlook = Outlook.Overcast,
                Temperature = Temperature.Cool,
                Windy = true
            };

            var model = (NeuralNetworkModel)BaseSupervised.Prediction<Tennis>(
                new NeuralNetworkGenerator(),
                Tennis.GetData(),
                t,
                p => p.Play
            );

            var node = model.Network.In[0].Out[0].Target;

            Serialize(node);
            
        }
    }
}
