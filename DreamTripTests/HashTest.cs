using Microsoft.VisualStudio.TestTools.UnitTesting;
using DreamTrip.Functions;

namespace DreamTripTests
{
    [TestClass]
    public class HashTest
    {
        [TestMethod]
        public void HashTest_Positive()
        {
            //Arrange
            string word1 = "wOrD";
            string word2 = "W0rd";
            //Act
            string hash1 = MainFunctions.GetHash(word1);
            string hash2 = MainFunctions.GetHash(word2);
            //Assert
            Assert.AreNotEqual(hash1, hash2);
        }
    }
}
