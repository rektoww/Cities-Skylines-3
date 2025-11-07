using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using Core.Models.Map;
using Core.Models.Base;
using Core.Models.Roads;
using Core.Enums;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Buildings.IndustrialBuildings;
using Core.Models.Buildings.SocialBuildings;
using Core.Models.Police;

namespace Infrastructure.Services
{
    /// <summary>
    /// Сервис для сохранения и загрузки данных игры
    /// </summary>
    public class SaveLoadService
    {
        /// <summary>
        /// Сохраняет текущее состояние игры в файл
        /// </summary>
        /// <param name="map">Игровая карта для сохранения</param>
        /// <param name="filePath">Путь к файлу сохранения</param>
        public void SaveGame(GameMap map, string filePath)
        {
            var buildingData = new List<object>();
            foreach (var building in map.Buildings)
            {
                buildingData.Add(new
                {
                    Type = building.GetType().Name,
                    X = building.X,
                    Y = building.Y,
                    Name = building.Name
                });
            }

            var roadData = new List<object>();
            foreach (var road in map.RoadSegments)
            {
                roadData.Add(new
                {
                    StartX = road.StartX,
                    StartY = road.StartY,
                    EndX = road.EndX,
                    EndY = road.EndY,
                    RoadType = road.RoadType.ToString()
                });
            }

            var saveData = new
            {
                Buildings = buildingData,
                Roads = roadData
            };

            string jsonString = JsonSerializer.Serialize(saveData);
            File.WriteAllText(filePath, jsonString);
        }

        /// <summary>
        /// Загружает состояние игры из файла
        /// </summary>
        /// <param name="map">Игровая карта для загрузки данных</param>
        /// <param name="filePath">Путь к файлу сохранения</param>
        public void LoadGame(GameMap map, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл сохранения не найден");

            string jsonString = File.ReadAllText(filePath);
            var saveData = JsonSerializer.Deserialize<SaveData>(jsonString);


            map.Buildings.Clear();
            map.RoadSegments.Clear();

            // Очищаем тайлы от старых зданий и дорог
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    map.Tiles[x, y].Building = null;
                    map.Tiles[x, y].HasRoad = false;
                    map.Tiles[x, y].HasIntersection = false;
                }
            }

            // Восстанавливаем здания
            foreach (var building in saveData.Buildings)
            {
                var newBuilding = CreateBuilding(building.Type, building.X, building.Y);
                if (newBuilding != null && building.X < map.Width && building.Y < map.Height)
                {
                    map.Buildings.Add(newBuilding);

                    map.Tiles[building.X, building.Y].Building = newBuilding;
                }
            }

            // Восстанавливаем дороги
            foreach (var road in saveData.Roads)
            {
                var roadType = Enum.Parse<RoadType>(road.RoadType);
                var newRoad = new RoadSegment(road.StartX, road.StartY, road.EndX, road.EndY, roadType);
                map.RoadSegments.Add(newRoad);

                UpdateTilesWithRoad(map, newRoad);
            }
        }

        /// <summary>
        /// Обновляет тайлы карты для отображения дорог
        /// </summary>
        /// <param name="map">Игровая карта</param>
        /// <param name="road">Сегмент дороги</param>
        private void UpdateTilesWithRoad(GameMap map, RoadSegment road)
        {
            var points = GetPointsAlongSegment(road);
            foreach (var point in points)
            {
                if (point.X >= 0 && point.X < map.Width && point.Y >= 0 && point.Y < map.Height)
                {
                    map.Tiles[(int)point.X, (int)point.Y].HasRoad = true;
                    map.Tiles[(int)point.X, (int)point.Y].RoadType = road.RoadType;
                }
            }
        }

        /// <summary>
        /// Получает все точки вдоль сегмента дороги
        /// </summary>
        /// <param name="segment">Сегмент дороги</param>
        /// <returns>Список точек дороги</returns>
        private List<System.Drawing.Point> GetPointsAlongSegment(RoadSegment segment)
        {
            var points = new List<System.Drawing.Point>();
            int dx = Math.Abs(segment.EndX - segment.StartX);
            int dy = Math.Abs(segment.EndY - segment.StartY);
            int steps = Math.Max(dx, dy);

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                int x = (int)Math.Round(segment.StartX + t * (segment.EndX - segment.StartX));
                int y = (int)Math.Round(segment.StartY + t * (segment.EndY - segment.StartY));
                points.Add(new System.Drawing.Point(x, y));
            }

            return points;
        }

        /// <summary>
        /// Создает здание по типу и координатам
        /// </summary>
        /// <param name="type">Тип здания</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <returns>Созданное здание</returns>
        private Core.Models.Base.Building CreateBuilding(string type, int x, int y)
        {
            return type switch
            {
                "Shop" => new Shop { X = x, Y = y },
                "Park" => new Park { X = x, Y = y },
                "PoliceStation" => new PoliceStation { X = x, Y = y },
                "Mine" => new Mine { X = x, Y = y },
                "Supermarket" => new Supermarket { X = x, Y = y },
                "Pharmacy" => new Pharmacy { X = x, Y = y },
                "Cafe" => new Cafe { X = x, Y = y },
                "Restaurant" => new Restaurant { X = x, Y = y },
                "GasStation" => new GasStation { X = x, Y = y },
                _ => new Shop { X = x, Y = y }
            };
        }

        /// <summary>
        /// Данные сохранения игры
        /// </summary>
        private class SaveData
        {
            public List<BuildingData> Buildings { get; set; }
            public List<RoadData> Roads { get; set; }
        }

        /// <summary>
        /// Данные здания для сохранения
        /// </summary>
        private class BuildingData
        {
            public string Type { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Данные дороги для сохранения
        /// </summary>
        private class RoadData
        {
            public int StartX { get; set; }
            public int StartY { get; set; }
            public int EndX { get; set; }
            public int EndY { get; set; }
            public string RoadType { get; set; }
        }
    }
}