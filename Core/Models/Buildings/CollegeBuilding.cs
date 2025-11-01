using Core.Enums;

namespace Core.Models.Buildings;

public class CollegeBuilding : ServiceBuilding
{
    /// <summary>
    /// Конструктор здания колледжа
    /// </summary>
    /// <param name="capacity">Опциональная вместимость здания</param>
    public CollegeBuilding(int capacity = 0) 
        : base(ServiceBuildingType.College, capacity)
    {
        // Здесь можно добавить любую специфичную для школы логику или свойства
    }
}