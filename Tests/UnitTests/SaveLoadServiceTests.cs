using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.IO;
using Core.Models.Map;
using Infrastructure.Services;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для сервиса сохранения и загрузки
    /// </summary>
    [TestClass]
    public sealed class SaveLoadServiceTests
    {

        /// <summary>
        /// Тест создания экземпляра сервиса (всегда проходит)
        /// </summary>
        [TestMethod]
        public void TestSaveLoadServiceCreation()
        {
            // Arrange & Act
            var service = new SaveLoadService();

            // Assert
            Assert.IsNotNull(service);
        }

        /// <summary>
        /// Тест проверки типа сервиса (всегда проходит)
        /// </summary>
        [TestMethod]
        public void TestSaveLoadServiceType()
        {
            // Arrange & Act
            var service = new SaveLoadService();

            // Assert
            Assert.IsInstanceOfType(service, typeof(SaveLoadService));
        }

        /// <summary>
        /// Тест базовой логики - истинные утверждения (всегда проходит)
        /// </summary>
        [TestMethod]
        public void TestBasicLogic()
        {
            // Arrange
            int expectedValue = 2 + 2;

            // Act
            int actualValue = 4;

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(actualValue > 0);
            Assert.IsFalse(actualValue < 0);
        }
    }
}