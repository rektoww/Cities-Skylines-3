using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Buildings.SocialBuildings;
using Core.Models.Map;
using Core.Enums;

namespace UnitTests
{
    [TestClass]
    public class ParkTests
    {
        [TestMethod]
        public void Park_Constructor_ShouldSetDefaultProperties()
        {
            // Arrange & Act
            var park = new Park();

            // Assert
            Assert.AreEqual("Парк", park.Name);
            Assert.AreEqual(2, park.Width);
            Assert.AreEqual(2, park.Height);
            Assert.AreEqual(0, park.Floors);
            Assert.AreEqual(30, park.MaxOccupancy);
            Assert.AreEqual(8, park.TreeCount);
            Assert.AreEqual(4, park.BenchCount);
        }

        [TestMethod]
        public void Park_Constructor_ShouldNotRequireUtilities()
        {
            // Arrange & Act
            var park = new Park();

            // Assert - парк не требует коммунальных услуг
            Assert.IsFalse(park.HasWater);
            Assert.IsFalse(park.HasGas);
            Assert.IsFalse(park.HasSewage);
            Assert.IsFalse(park.HasElectricity);
            Assert.IsFalse(park.IsOperational); // Не работоспособен без коммуникаций
        }
    }
}