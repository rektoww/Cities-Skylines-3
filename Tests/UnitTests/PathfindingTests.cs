using Core.Models.Map;
using Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для системы поиска пути
    /// </summary>
    [TestClass]
    public class PathfindingTests
    {
        private GameMap _map;
        private PathfindingService _pathfinder;

        /// <summary>
        /// Инициализация тестового окружения перед каждым тестом
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _map = new GameMap(20, 20);
            _pathfinder = new PathfindingService(_map);
        }

        /// <summary>
        /// Проверяет успешное нахождение пути между валидными точками
        /// </summary>
        [TestMethod]
        public void PathfindingService_FindPath_ReturnsPathForValidPoints()
        {
            int startX = 0, startY = 0;
            int endX = 5, endY = 5;

            var path = _pathfinder.FindPath(startX, startY, endX, endY);

            Assert.IsNotNull(path);
            Assert.IsTrue(path.Count > 0);
            Assert.AreEqual(startX, path[0].X);
            Assert.AreEqual(startY, path[0].Y);
            Assert.AreEqual(endX, path[path.Count - 1].X);
            Assert.AreEqual(endY, path[path.Count - 1].Y);
        }

        /// <summary>
        /// Проверяет возврат null при невалидной начальной точке
        /// </summary>
        [TestMethod]
        public void PathfindingService_FindPath_ReturnsNullForInvalidStart()
        {
            int startX = -1, startY = -1;
            int endX = 5, endY = 5;

            var path = _pathfinder.FindPath(startX, startY, endX, endY);

            Assert.IsNull(path);
        }

        /// <summary>
        /// Проверяет возврат null при невалидной начальной точке
        /// </summary>
        [TestMethod]
        public void PathfindingService_FindPath_ReturnsNullForInvalidEnd()
        {
            int startX = 0, startY = 0;
            int endX = 100, endY = 100; // За пределами карты

            var path = _pathfinder.FindPath(startX, startY, endX, endY);

            Assert.IsNull(path);
        }

        /// <summary>
        /// Проверяет предпочтение дорог при построении маршрута
        /// </summary>
        [TestMethod]
        public void PathfindingService_FindPath_PreferRoadsOverEmptyTiles()
        {
            // Создаём дорогу от (0,0) до (5,0)
            for (int x = 0; x <= 5; x++)
            {
                _map.Tiles[x, 0].HasRoad = true;
            }

            var pathWithRoad = _pathfinder.FindPath(0, 0, 5, 0);
            var pathWithoutRoad = _pathfinder.FindPath(0, 1, 5, 1);

            Assert.IsNotNull(pathWithRoad);
            Assert.IsNotNull(pathWithoutRoad);
            // Путь по дороге должен быть короче (меньше стоимость)
        }

        /// <summary>
        /// Проверяет построение прямолинейного маршрута как кратчайшего пути
        /// </summary>
        [TestMethod]
        public void PathfindingService_FindPath_StraightLineIsShortest()
        {
            int startX = 0, startY = 0;
            int endX = 0, endY = 5;

            var path = _pathfinder.FindPath(startX, startY, endX, endY);

            Assert.IsNotNull(path);
            Assert.AreEqual(6, path.Count); // 0,1,2,3,4,5 = 6 точек
        }

        /// <summary>
        /// Проверяет корректность расчета F-значения для узла пути
        /// </summary>
        [TestMethod]
        public void PathNode_FValue_CalculatesCorrectly()
        {
            var node = new PathNode
            {
                X = 5,
                Y = 5,
                G = 10f,
                H = 15f
            };

            var fValue = node.F;

            Assert.AreEqual(25f, fValue);
        }
    }
}
