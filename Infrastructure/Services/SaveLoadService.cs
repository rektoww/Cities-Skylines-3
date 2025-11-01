using System.Text.Json;
using System.IO;
using Core.Models.Map;

namespace Infrastructure.Services
{
    /// <summary>
    /// Сервис для сохранения и загрузки данных карты
    /// </summary>
    public class SaveLoadService
    {
        /// <summary>
        /// Сохраняет карту в файл
        /// </summary>
        public void SaveMap(GameMap map, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(map);
            File.WriteAllText(filePath, jsonString);
        }

        /// <summary>
        /// Загружает карту из файла
        /// </summary>
        public GameMap LoadMap(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameMap>(jsonString);
        }
    }
}