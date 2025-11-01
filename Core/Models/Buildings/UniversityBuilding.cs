using Core.Enums;
namespace Core.Models.Buildings;

public class UniversityBuilding : ServiceBuilding
{
    /// <summary>
    /// Конструктор для университетского здания
    /// Устанавливает тип здания как University и использует дефолтную или переданную вместимость
    /// </summary>
    /// <param name="capacity">Опциональная вместимость здания.</param>
    public UniversityBuilding(int capacity = 0) 
        : base(ServiceBuildingType.University, capacity) 
    {
        // Здесь можно добавить любую специфичную для университета логику или свойства.
        // Например, влияние на научный прогресс или требование к квалификации преподавателей.
    }
}