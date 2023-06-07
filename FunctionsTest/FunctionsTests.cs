using Microsoft.VisualStudio.TestTools.UnitTesting;
using DreamTrip.Functions;

namespace FunctionsTest
{
    [TestClass]
    public class FunctionsTests
    {
        [TestMethod]
        public void Authorize_Positive()
        {
            //Arrange
            string login = "admin";
            string passwordHash = MainFunctions.GetHash("admin1234");
            //Act
            bool isLoginExists = MainFunctions.Authorize_LoginExists(login);
            bool isPasswordCorrect = MainFunctions.Authorize_CheckPassword(login, passwordHash);
            //Assert
            Assert.IsTrue(isLoginExists);
            Assert.IsTrue(isPasswordCorrect);

        }

        [TestMethod]
        public void Authorize_Negative()
        {
            //Arrange
            string login = "94ur''f{}'s93";
            string passwordHash = MainFunctions.GetHash("'\\@34'5{3}/");
            //Act
            bool isLoginExists = MainFunctions.Authorize_LoginExists(login);
            bool isPasswordCorrect = MainFunctions.Authorize_CheckPassword(login, passwordHash);
            //Assert
            Assert.IsFalse(isLoginExists);
            Assert.IsFalse(isPasswordCorrect);
        }

        [TestMethod]
        public void CheckDateValid_Positive()
        {
            //Arrange
            string dateText = "2023-10-15";
            //Act
            bool isValid = CommonFunctions.isTextDateValid(dateText);
            //Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void CheckDateValid_Negative()
        {
            //Arrange
            string dateText = "30.02.2023";
            //Act
            bool isValid = CommonFunctions.isTextDateValid(dateText);
            //Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidateName_Positive()
        {
            //Arrange
            string name = "Сергей Колесников-Светлов";
            //Act
            bool isValid = MainFunctions.ValidateString_RuEng(name);
            //Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void ValidateName_Negative()
        {
            //Arrange
            string name = "Пользователь №19";
            //Act
            bool isValid = MainFunctions.ValidateString_RuEng(name);
            //Assert
            Assert.IsFalse(isValid);
        }
    }
}
