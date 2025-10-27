using System.Text.Json;
using System.IO;
using Core.Models.Map;

namespace Infrastructure.Services
{
    public class SaveLoadService
    {
        public void SaveMap(GameMap map, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(map);
            File.WriteAllText(filePath, jsonString);
        }

        public GameMap LoadMap(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameMap>(jsonString);
        }
    }
}