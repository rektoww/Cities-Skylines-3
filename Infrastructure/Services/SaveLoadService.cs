using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Buildings.IndustrialBuildings;
using Core.Models.Map;
using Core.Models.Police;
using Core.Models.Roads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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
                var buildingInfo = new
                {
                    Type = building.GetType().Name,
                    BuildingType = building.BuildingType.ToString(),
                    X = building.X,
                    Y = building.Y,
                    Name = building.Name
                };

                // Добавляем специфические данные для разных типов зданий
                switch (building)
                {
                    case ResidentialBuilding residential:
                        buildingData.Add(new
                        {
                            buildingInfo.Type,
                            buildingInfo.BuildingType,
                            buildingInfo.X,
                            buildingInfo.Y,
                            buildingInfo.Name,
                            ResidentialType = residential.Type.ToString(),
                            Capacity = residential.Capacity,
                            CurrentResidents = residential.CurrentResidents.Count
                        });
                        break;

                    case CommercialBuilding commercial:
                        buildingData.Add(new
                        {
                            buildingInfo.Type,
                            buildingInfo.BuildingType,
                            buildingInfo.X,
                            buildingInfo.Y,
                            buildingInfo.Name,
                            CommercialType = commercial.Type.ToString(),
                            Capacity = commercial.Capacity,
                            EmployeeCount = commercial.EmployeeCount
                        });
                        break;

                    case ServiceBuilding service:
                        buildingData.Add(new
                        {
                            buildingInfo.Type,
                            buildingInfo.BuildingType,
                            buildingInfo.X,
                            buildingInfo.Y,
                            buildingInfo.Name,
                            ServiceType = service.Type.ToString(),
                            Capacity = service.Capacity,
                            ClientsCount = service.Clients.Count
                        });
                        break;

                    case IndustrialBuilding industrial:
                        buildingData.Add(new
                        {
                            buildingInfo.Type,
                            buildingInfo.BuildingType,
                            buildingInfo.X,
                            buildingInfo.Y,
                            buildingInfo.Name,
                            IndustrialType = industrial.Type.ToString(),
                            ProducedMaterial = industrial.ProducedMaterial.ToString(),
                            StoredResources = industrial.StoredResources
                        });
                        break;

                    case PoliceStation police:
                        buildingData.Add(new
                        {
                            buildingInfo.Type,
                            buildingInfo.BuildingType,
                            buildingInfo.X,
                            buildingInfo.Y,
                            buildingInfo.Name,
                            OfficersCount = police.Officers.Count,
                            PatrolCarsCount = police.PatrolCars.Count
                        });
                        break;

                    default:
                        buildingData.Add(buildingInfo);
                        break;
                }
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
                Roads = roadData,
                MapWidth = map.Width,
                MapHeight = map.Height,
                SaveDate = DateTime.Now
            };

            string jsonString = JsonSerializer.Serialize(saveData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
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
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;

            // Очищаем карту
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
            if (root.TryGetProperty("buildings", out var buildingsElement))
            {
                foreach (var buildingElement in buildingsElement.EnumerateArray())
                {
                    string type = buildingElement.GetProperty("type").GetString();
                    int x = buildingElement.GetProperty("x").GetInt32();
                    int y = buildingElement.GetProperty("y").GetInt32();

                    var newBuilding = CreateBuilding(buildingElement);
                    if (newBuilding != null && x >= 0 && x < map.Width && y >= 0 && y < map.Height)
                    {
                        if (newBuilding.TryPlace(x, y, map))
                        {
                            map.Buildings.Add(newBuilding);
                        }
                    }
                }
            }

            // Восстанавливаем дороги
            if (root.TryGetProperty("roads", out var roadsElement))
            {
                foreach (var roadElement in roadsElement.EnumerateArray())
                {
                    int startX = roadElement.GetProperty("startX").GetInt32();
                    int startY = roadElement.GetProperty("startY").GetInt32();
                    int endX = roadElement.GetProperty("endX").GetInt32();
                    int endY = roadElement.GetProperty("endY").GetInt32();
                    string roadTypeString = roadElement.GetProperty("roadType").GetString();

                    if (Enum.TryParse<RoadType>(roadTypeString, out var roadType))
                    {
                        var newRoad = new RoadSegment(startX, startY, endX, endY, roadType);
                        map.AddRoadSegment(newRoad);
                    }
                }
            }
        }

        /// <summary>
        /// Создает здание по данным из JSON
        /// </summary>
        /// <param name="buildingElement">JSON элемент с данными здания</param>
        /// <returns>Созданное здание</returns>
        private Building CreateBuilding(JsonElement buildingElement)
        {
            string type = buildingElement.GetProperty("type").GetString();
            int x = buildingElement.GetProperty("x").GetInt32();
            int y = buildingElement.GetProperty("y").GetInt32();

            Building building = type switch
            {
                // Жилые здания
                "ResidentialBuilding" => CreateResidentialBuilding(buildingElement),
                "CommercialBuilding" => CreateCommercialBuilding(buildingElement),
                "ServiceBuilding" => CreateServiceBuilding(buildingElement),
                "IndustrialBuilding" => CreateIndustrialBuilding(buildingElement),

                // Полиция
                "PoliceStation" => new PoliceStation(),

                // Старые коммерческие здания (для обратной совместимости)
                "Shop" => new CommercialBuilding(CommercialBuildingType.Shop),
                "Supermarket" => new CommercialBuilding(CommercialBuildingType.Supermarket),
                "Cafe" => new CommercialBuilding(CommercialBuildingType.Cafe),
                "Restaurant" => new CommercialBuilding(CommercialBuildingType.Restaurant),
                "GasStation" => new CommercialBuilding(CommercialBuildingType.GasStation),
                "Pharmacy" => new CommercialBuilding(CommercialBuildingType.Pharmacy),

                // Промышленные здания
                "Mine" => new IndustrialBuilding(IndustrialBuildingType.Mine),
                "Factory" => new IndustrialBuilding(IndustrialBuildingType.Factory),
                "Farm" => new IndustrialBuilding(IndustrialBuildingType.Farm),
                "PowerPlant" => new IndustrialBuilding(IndustrialBuildingType.PowerPlant),

                // По умолчанию - магазин
                _ => new CommercialBuilding(CommercialBuildingType.Shop)
            };

            building.X = x;
            building.Y = y;
            return building;
        }

        private ResidentialBuilding CreateResidentialBuilding(JsonElement element)
        {
            if (element.TryGetProperty("residentialType", out var typeElement))
            {
                var typeString = typeElement.GetString();
                if (Enum.TryParse<ResidentialType>(typeString, out var residentialType))
                {
                    return new ResidentialBuilding(residentialType);
                }
            }
            return new ResidentialBuilding(ResidentialType.Apartment);
        }

        private CommercialBuilding CreateCommercialBuilding(JsonElement element)
        {
            if (element.TryGetProperty("commercialType", out var typeElement))
            {
                var typeString = typeElement.GetString();
                if (Enum.TryParse<CommercialBuildingType>(typeString, out var commercialType))
                {
                    return new CommercialBuilding(commercialType);
                }
            }
            return new CommercialBuilding(CommercialBuildingType.Shop);
        }

        private ServiceBuilding CreateServiceBuilding(JsonElement element)
        {
            if (element.TryGetProperty("serviceType", out var typeElement))
            {
                var typeString = typeElement.GetString();
                if (Enum.TryParse<ServiceBuildingType>(typeString, out var serviceType))
                {
                    return new ServiceBuilding(serviceType);
                }
            }
            return new ServiceBuilding(ServiceBuildingType.School);
        }

        private IndustrialBuilding CreateIndustrialBuilding(JsonElement element)
        {
            if (element.TryGetProperty("industrialType", out var typeElement))
            {
                var typeString = typeElement.GetString();
                if (Enum.TryParse<IndustrialBuildingType>(typeString, out var industrialType))
                {
                    return new IndustrialBuilding(industrialType);
                }
            }
            return new IndustrialBuilding(IndustrialBuildingType.Factory);
        }
    }
}