using Core.Enums;
using Core.Models.Roads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для системы дорог
    /// </summary>
    [TestClass]
    public class RoadSystemTests
    {
        /// <summary>
        /// Проверяет корректность инициализации свойств сегмента дороги
        /// </summary>
        [TestMethod]
        public void RoadSegment_Creation_SetsPropertiesCorrectly()
        {
            var segment = new RoadSegment(0, 0, 10, 0, RoadType.Street);

            Assert.AreEqual(0, segment.StartX);
            Assert.AreEqual(0, segment.StartY);
            Assert.AreEqual(10, segment.EndX);
            Assert.AreEqual(0, segment.EndY);
            Assert.AreEqual(RoadType.Street, segment.RoadType);
            Assert.AreEqual(40f, segment.SpeedLimit); // Городская улица имеет скорость 40
            Assert.AreEqual(500m, segment.BuildCost); // Городская улица стоит 500
        }

        /// <summary>
        /// Проверяет корректность расчета длины сегмента дороги
        /// </summary>
        [TestMethod]
        public void RoadSegment_GetLength_CalculatesCorrectly()
        {
            var segment = new RoadSegment(0, 0, 3, 4, RoadType.Street);

            var length = segment.GetLength();

            Assert.AreEqual(5.0, length, 0.01); // 3-4-5 треугольник
        }

        /// <summary>
        /// Проверяет определение принадлежности точки к сегменту дороги
        /// </summary>
        [TestMethod]
        public void RoadSegment_PassesThrough_ReturnsTrueForPointOnLine()
        {
            var segment = new RoadSegment(0, 0, 10, 0, RoadType.Street);

            var passes = segment.PassesThrough(5, 0);

            Assert.IsTrue(passes);
        }

        /// <summary>
        /// Проверяет различия характеристик для разных типов дорог
        /// </summary>
        [TestMethod]
        public void RoadSegment_DifferentTypes_HaveDifferentCharacteristics()
        {
            var dirt = new RoadSegment(0, 0, 10, 0, RoadType.Dirt);
            var highway = new RoadSegment(0, 0, 10, 0, RoadType.Highway);

            Assert.IsTrue(highway.SpeedLimit > dirt.SpeedLimit);
            Assert.IsTrue(highway.BuildCost > dirt.BuildCost);
        }

        /// <summary>
        /// Проверяет добавление сегмента в дорогу и обновление агрегированных данных
        /// </summary>
        [TestMethod]
        public void Road_AddSegment_IncreasesCount()
        {
            var road = new Road(1, "Main Street", RoadType.Street);
            var segment = new RoadSegment(0, 0, 10, 0, RoadType.Street);

            road.AddSegment(segment);

            Assert.AreEqual(1, road.Segments.Count);
            Assert.IsTrue(road.TotalLength > 0);
            Assert.IsTrue(road.TotalCost > 0);
        }

        /// <summary>
        /// Проверяет удаление сегмента из дороги и обновление агрегированных данных
        /// </summary>
        [TestMethod]
        public void Road_RemoveSegment_DecreasesCount()
        {
            var road = new Road(1, "Main Street", RoadType.Street);
            var segment = new RoadSegment(0, 0, 10, 0, RoadType.Street);
            road.AddSegment(segment);

            road.RemoveSegment(segment);

            Assert.AreEqual(0, road.Segments.Count);
            Assert.AreEqual(0, road.TotalLength);
            Assert.AreEqual(0, road.TotalCost);
        }

        /// <summary>
        /// Проверяет расчет средней скорости для дороги с разными типами сегментов
        /// </summary>
        [TestMethod]
        public void Road_GetAverageSpeed_CalculatesCorrectly()
        {
            var road = new Road(1, "Main Street", RoadType.Street);
            road.AddSegment(new RoadSegment(0, 0, 10, 0, RoadType.Street)); // 40
            road.AddSegment(new RoadSegment(10, 0, 20, 0, RoadType.Highway)); // 100

            var avgSpeed = road.GetAverageSpeed();

            Assert.AreEqual(70f, avgSpeed, 0.01); // (40 + 100) / 2
        }

        /// <summary>
        /// Проверяет корректность инициализации перекрестка
        /// </summary>
        [TestMethod]
        public void Intersection_Creation_InitializesCorrectly()
        {
            var intersection = new Intersection(5, 5, hasTrafficLight: true);

            Assert.AreEqual(5, intersection.X);
            Assert.AreEqual(5, intersection.Y);
            Assert.IsTrue(intersection.HasTrafficLight);
            Assert.AreEqual(0, intersection.ConnectedRoads.Count);
            Assert.AreEqual(0, intersection.CurrentVehicles.Count);
        }

        /// <summary>
        /// Проверяет добавление дороги к перекрестку
        /// </summary>
        [TestMethod]
        public void Intersection_AddRoad_IncreasesConnectedRoads()
        {
            var intersection = new Intersection(5, 5);
            var segment = new RoadSegment(0, 0, 10, 0, RoadType.Street);

            intersection.AddRoad(segment);

            Assert.AreEqual(1, intersection.ConnectedRoads.Count);
        }

        /// <summary>
        /// Проверяет возможность въезда на перекресток при наличии свободного места
        /// </summary>
        [TestMethod]
        public void Intersection_CanEnter_ReturnsTrueWhenNotFull()
        {
            var intersection = new Intersection(5, 5);

            Assert.IsTrue(intersection.CanEnter());
        }

        /// <summary>
        /// Проверяет подсчет количества дорог, подключенных к перекрестку
        /// </summary>
        [TestMethod]
        public void Intersection_GetRoadCount_ReturnsCorrectCount()
        {
            var intersection = new Intersection(5, 5);
            intersection.AddRoad(new RoadSegment(0, 0, 5, 5, RoadType.Street));
            intersection.AddRoad(new RoadSegment(5, 5, 10, 10, RoadType.Street));

            var count = intersection.GetRoadCount();

            Assert.AreEqual(2, count);
        }
    }
}
