using Core.Models.Base;
using Core.Resourses;

namespace Core.Models.Buildings
{
    /// <summary>
    /// Воздушный порт.
    /// </summary>
    public class AirPort : Port
    {
        // Количество юнитов: теперь просто реализуем абстрактное свойство
        protected override int MaxUnits => 5;

        protected override string ResourceType => ""; 
        protected override int UnitCapacity => 100; // Вместимость самолета
        protected override int UnitCooldown => 3; // Время одного цикла продажи (в тиках)
        protected override int UnitRevenue => 200; // Доход за один цикл продажи

        public AirPort(string name, PlayerResources playerResources)
            : base(name, playerResources)
        {
        }

        protected override PortUnit CreateUnit()
        {
            return new PortUnit(UnitCapacity, UnitCooldown, UnitRevenue);
        }
    }
}