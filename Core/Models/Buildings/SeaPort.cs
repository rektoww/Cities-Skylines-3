using Core.Models.Base;
using Core.Resourses;

namespace Core.Models.Buildings
{
    /// <summary>
    /// Морской порт.
    /// </summary>
    public class SeaPort : Port
    {
        // Количество юнитов: теперь просто реализуем абстрактное свойство
        protected override int MaxUnits => 10;

        // Параметры юнитов
        protected override string ResourceType => ""; // Базовый тип ресурса
        protected override int UnitCapacity => 200; // Вместимость корабля
        protected override int UnitCooldown => 5; // Время одного цикла продажи (в тиках)
        protected override int UnitRevenue => 300; // Доход за один цикл продажи

        public SeaPort(string name, PlayerResources playerResources)
            : base(name, playerResources)
        {
        }

        protected override PortUnit CreateUnit()
        {
            return new PortUnit(UnitCapacity, UnitCooldown, UnitRevenue);
        }
    }
}