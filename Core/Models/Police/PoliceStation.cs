using Core.Models.Base;

namespace Core.Models.Police
{
    /// <summary>
    /// Представляет полицейский участок.
    /// Управляет полицейскими и патрульными машинами, реагирует на преступления.
    /// </summary>
    public class PoliceStation : Building
    {
        public List<PoliceOfficer> Officers { get; set; }
        public List<PoliceCar> PatrolCars { get; set; }
        public int CoverageRadius { get; set; }
        public int UnsolvedCrimesCount { get; set; }
        public float StationEfficiency { get; set; }
        public int MaxOfficers { get; set; }
        public int MaxPatrolCars { get; set; }
        public decimal BuildCost { get; }
        public decimal MaintenanceCost { get; }

        /// <summary>
        /// Конструктор полицейского участка
        /// </summary>
        public PoliceStation()
        {
            Width = 3;
            Height = 3;
            BuildCost = 25000m;
            MaintenanceCost = 500m;
            
            Officers = new List<PoliceOfficer>();
            PatrolCars = new List<PoliceCar>();
            CoverageRadius = 30;
            UnsolvedCrimesCount = 0;
            StationEfficiency = 70f;
            MaxOfficers = 10;
            MaxPatrolCars = 3;
        }

        /// <summary>
        /// Нанимает нового полицейского
        /// </summary>
        public bool HireOfficer(PoliceOfficer officer)
        {
            if (Officers.Count >= MaxOfficers)
                return false;

            Officers.Add(officer);
            officer.HomeStation = this;
            officer.X = X;
            officer.Y = Y;
            
            UpdateStationEfficiency();
            return true;
        }

        /// <summary>
        /// Увольняет полицейского
        /// </summary>
        public void FireOfficer(PoliceOfficer officer)
        {
            Officers.Remove(officer);
            UpdateStationEfficiency();
        }

        /// <summary>
        /// Добавляет патрульную машину
        /// </summary>
        public bool AddPatrolCar(PoliceCar car)
        {
            if (PatrolCars.Count >= MaxPatrolCars)
                return false;

            PatrolCars.Add(car);
            car.HomeStation = this;
            car.X = X;
            car.Y = Y;
            return true;
        }

        /// <summary>
        /// Удаляет патрульную машину
        /// </summary>
        public void RemovePatrolCar(PoliceCar car)
        {
            PatrolCars.Remove(car);
        }

        /// <summary>
        /// Отправляет ближайший доступный патруль на преступление
        /// </summary>
        public bool DispatchToCrime(Crime crime)
        {
            // Проверяем, в зоне ли действия участка
            if (!IsInCoverageArea(crime.LocationX, crime.LocationY))
                return false;

            // Ищем доступную патрульную машину
            var availableCar = PatrolCars.FirstOrDefault(car => car.IsAvailable());

            if (availableCar != null)
            {
                return availableCar.DispatchToCrime(crime);
            }

            // Если нет машин, отправляем пешего офицера
            var availableOfficer = Officers.FirstOrDefault(officer => officer.IsAvailable());

            if (availableOfficer != null)
            {
                availableOfficer.AssignCrime(crime);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет, находится ли точка в зоне действия участка
        /// </summary>
        public bool IsInCoverageArea(int x, int y)
        {
            int distance = Math.Abs(X - x) + Math.Abs(Y - y);
            return distance <= CoverageRadius;
        }

        /// <summary>
        /// Обновляет эффективность участка на основе количества офицеров и их опыта
        /// </summary>
        private void UpdateStationEfficiency()
        {
            if (Officers.Count == 0)
            {
                StationEfficiency = 0f;
                return;
            }

            // Базовая эффективность зависит от укомплектованности
            float staffingRatio = (float)Officers.Count / MaxOfficers;
            float baseEfficiency = staffingRatio * 50f;

            // Добавляем среднюю эффективность офицеров
            float avgOfficerEfficiency = Officers.Average(o => o.EfficiencyLevel) / 2f;

            StationEfficiency = Math.Min(100f, baseEfficiency + avgOfficerEfficiency);
        }

        /// <summary>
        /// Получает количество доступных офицеров
        /// </summary>
        public int GetAvailableOfficersCount()
        {
            return Officers.Count(o => o.IsAvailable());
        }

        /// <summary>
        /// Получает количество доступных патрульных машин
        /// </summary>
        public int GetAvailablePatrolCarsCount()
        {
            return PatrolCars.Count(c => c.IsAvailable());
        }

        /// <summary>
        /// Вызывается при размещении здания на карте
        /// </summary>
        public override void OnBuildingPlaced()
        {
            // Автоматически нанимаем стартовый персонал (2 офицера)
            for (int i = 0; i < 2; i++)
            {
                var officer = new PoliceOfficer(X, Y, GameMap, $"Officer #{i + 1}", this);
                HireOfficer(officer);
            }

            // Добавляем одну патрульную машину
            var patrolCar = new PoliceCar(X, Y, GameMap, this);
            
            // Добавляем офицеров в патрульную машину
            if (Officers.Count >= 2)
            {
                patrolCar.AddOfficer(Officers[0]);
                patrolCar.AddOfficer(Officers[1]);
            }
            
            AddPatrolCar(patrolCar);
            patrolCar.StartPatrol(); // Начинаем патрулирование
        }
    }
}
