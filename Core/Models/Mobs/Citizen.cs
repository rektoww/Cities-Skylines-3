using Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Map;

namespace Core.Models.Mobs
{
    public class Citizen : Mob
    {
        public Citizen(int x, int y, GameMap map) : base(x, y, map) { }

        public int Age { get; set; }
        public bool IsMale { get; set; }

        public EducationLevel Education { get; set; }
        public bool IsStudying { get; set; }
        public Building School { get; set; }

        public JobType Profession { get; set; }
        public bool IsEmployed { get; set; }
        public Building Workplace { get; set; }
        public decimal Salary { get; set; }

        public bool IsMarried { get; set; }
        public Citizen Partner { get; set; }
        public List<Citizen> Children { get; set; } = new();
        public bool CanReproduce => Age >= 18 && Age <= 45 && IsMarried;
        public ResidentialBuilding Home { get; set; }

        public float Health { get; set; } = 100f;

        public float Happiness { get; set; } = 50f;

        public float AcademicProgress { get; set; } = 0f;
        public float AcademicPerformance { get; set; }

        public Building DestinationBuilding { get; set; }
        public TransitStation TargetTransitStation { get; set; }
        public Transport CurrentTransport { get; set; }

        public bool IsWaitingForTransport => TargetTransitStation != null && CurrentTransport == null && IsAtStationTile();
        public bool IsOnTransport => CurrentTransport != null;
        public bool IsMoving => DestinationBuilding != null || TargetTransitStation != null || IsOnTransport;


        public bool TryMoveTo(int toX, int toY)
        {
            return MoveTo(toX, toY);
        }

        public override void Move()
        {
        }

        private bool IsAtStationTile()
        {
            if (TargetTransitStation == null) return false;
            return X == TargetTransitStation.X && Y == TargetTransitStation.Y;
        }
    }
}
