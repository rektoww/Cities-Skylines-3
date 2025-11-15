using Core.Enums;
using Core.Models.Base;
using Core.Resourses;
using System.Collections.Generic;

namespace Core.Models.Buildings
{
    public abstract class Port : TransitStation
    {
        protected PlayerResources PlayerResources { get; }
        protected List<PortUnit> PortUnits { get; } = new List<PortUnit>();

        protected Port(PlayerResources playerResources) : base()
        {
            PlayerResources = playerResources;
        }

        // Абстрактные свойства, которые должны быть реализованы в наследниках
        protected abstract int MaxUnits { get; }
        protected abstract string ResourceType { get; }
        protected abstract int UnitCapacity { get; }
        protected abstract int UnitCooldown { get; }
        protected abstract int UnitRevenue { get; }
        protected abstract int PassengersPerUnit { get; }
        protected abstract PortUnit CreateUnit();

        protected void AddPortUnit(PortUnit unit)
        {
            if (PortUnits.Count < MaxUnits)
            {
                PortUnits.Add(unit);
            }
        }

        public void ProcessPortCycle()
        {
            if (!IsOperational) return;

            foreach (var unit in PortUnits)
            {
                if (unit.TryProcessCycle())
                {
                    // Добавляем доход в ресурсы игрока
                    PlayerResources.Balance += unit.Revenue;
                }
            }
        }
    }

    public class PortUnit
    {
        public int Capacity { get; }
        public int Cooldown { get; }
        public int Revenue { get; }
        public int CurrentCooldown { get; private set; }

        public PortUnit(int capacity, int cooldown, int revenue)
        {
            Capacity = capacity;
            Cooldown = cooldown;
            Revenue = revenue;
            CurrentCooldown = 0;
        }

        public bool TryProcessCycle()
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown--;
                return false;
            }

            CurrentCooldown = Cooldown;
            return true;
        }
    }
}