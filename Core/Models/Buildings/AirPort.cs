using Core.Models.Base;
using Core.Resourses;

csharp Core\Models\Buildings\AirPort.cs
using Core.Models.Base;
using Core.Resourses;

namespace Core.Models.Buildings
{
    /// <summary>
    /// Воздушный порт.
    /// </summary>
    public class AirPort : Port
    {
        // Параметры юнитов
        protected override string ResourceType => "Concrete"; // Тип ресурса
        protected override int UnitCapacity => 25; // Вместимость самолета
        protected override int UnitCooldown => 10; // Время одного цикла продажи (в тиках)
        protected override int UnitRevenue => 200; // Доход за один цикл продажи

        public AirPort(string name, int maxUnits, PlayerResources playerResources)
            : base(name, maxUnits, playerResources)
        {
        }

        protected override PortUnit CreateUnit()
        {
            return new PortUnit(ResourceType, UnitCapacity, UnitCooldown, UnitRevenue);
        }
    }
}