namespace Core.Models.Buildings;
using Core.Enums;

public class SchoolBuilding : ServiceBuilding
{
    /// <summary>
    /// Конструктор для школьного здания
    /// Устанавливает тип здания как School и использует дефолтную или переданную вместимость
    /// </summary>
    /// <param name="capacity">Опциональная вместимость здания.</param>
    public SchoolBuilding(int capacity = 0)
        : base(ServiceBuildingType.School, capacity) 
    {
        // Здесь можно добавить любую специфичную для школы логику или свойства
    }
}