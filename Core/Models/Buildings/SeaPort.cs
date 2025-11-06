using Core.Models.Base;
using Core.Resourses;

using Core.Models.Base;
using Core.Resourses;

namespace Core.Models.Buildings
{
    /// <summary>
    /// Морской порт.
    /// </summary>
    public class SeaPort : Port
    {
        // Параметры юнитов
        protected override string ResourceType => "Steel"; // Тип ресурса
        protected override int UnitCapacity => 100; // Вместимость корабля
        protected override int UnitCooldown => 30; // Время одного цикла продажи (в тиках)
        protected override int UnitRevenue => 500; // Доход за один цикл продажи

        public SeaPort(string name, int maxUnits, PlayerResources playerResources)
            : base(name, maxUnits, playerResources)
        {
        }

        protected override PortUnit CreateUnit()
        {
            return new PortUnit(ResourceType, UnitCapacity, UnitCooldown, UnitRevenue);
        }
    }
}