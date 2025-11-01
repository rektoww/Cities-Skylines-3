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
        /// Тест сохранения карты в файл
        /// </summary>
        [TestMethod]
        public void TestSaveMap()
        {
            var service = new SaveLoadService();
            var map = new GameMap(10, 10);
            string testFilePath = "test_save.json";

            service.SaveMap(map, testFilePath);

            Assert.IsTrue(File.Exists(testFilePath));

            File.Delete(testFilePath);
        }

        /// <summary>
        /// Тест загрузки карты из файла
        /// </summary>
        [TestMethod]
        public void TestLoadMap()
        {
            var service = new SaveLoadService();
            var originalMap = new GameMap(5, 5);
            string testFilePath = "test_load.json";

            service.SaveMap(originalMap, testFilePath);
            var loadedMap = service.LoadMap(testFilePath);

            Assert.IsNotNull(loadedMap);
            Assert.AreEqual(originalMap.Width, loadedMap.Width);
            Assert.AreEqual(originalMap.Height, loadedMap.Height);

            File.Delete(testFilePath);
        }
    }
}
            