using Core.Models.Base;
using Core.Models.Map;

namespace Core.Models.Police
{
    /// <summary>
    /// Представляет полицейского - сотрудника полиции.
    /// Работает в полицейском участке и выезжает на вызовы.
    /// </summary>
    public class PoliceOfficer : Mob
    {
        public string Name { get; set; }
        public string Rank { get; set; }
        public int ExperienceMonths { get; set; }
        public bool IsOnDuty { get; set; }
        public float EfficiencyLevel { get; set; }
        public PoliceStation HomeStation { get; set; }
        public Crime CurrentAssignment { get; set; }

        /// <summary>
        /// Конструктор полицейского
        /// </summary>
        public PoliceOfficer(int x, int y, GameMap map, string name, PoliceStation station) 
            : base(x, y, map)
        {
            Name = name;
            Rank = "Officer"; // Начальное звание
            ExperienceMonths = 0;
            IsOnDuty = true;
            EfficiencyLevel = 50f; // Средняя эффективность
            HomeStation = station;
            CurrentAssignment = null;
        }

        /// <summary>
        /// Движение полицейского
        /// </summary>
        public override void Move()
        {
            // Если есть текущее задание, двигаемся к месту преступления
            if (CurrentAssignment != null)
            {
                int targetX = CurrentAssignment.LocationX;
                int targetY = CurrentAssignment.LocationY;

                // Простое движение к цели
                if (X != targetX || Y != targetY)
                {
                    MoveTowards(targetX, targetY);
                }
                else
                {
                    // Прибыли на место - расследуем преступление
                    InvestigateCrime();
                }
            }
            else if (HomeStation != null)
            {
                // Если нет задания, возвращаемся на станцию
                if (X != HomeStation.X || Y != HomeStation.Y)
                {
                    MoveTowards(HomeStation.X, HomeStation.Y);
                }
            }
        }

        /// <summary>
        /// Простое движение к цели
        /// </summary>
        private void MoveTowards(int targetX, int targetY)
        {
            int deltaX = targetX - X;
            int deltaY = targetY - Y;

            if (Math.Abs(deltaX) > Math.Abs(deltaY))
            {
                MoveTo(X + Math.Sign(deltaX), Y);
            }
            else
            {
                MoveTo(X, Y + Math.Sign(deltaY));
            }
        }

        /// <summary>
        /// Расследует преступление
        /// </summary>
        private void InvestigateCrime()
        {
            if (CurrentAssignment == null || CurrentAssignment.IsSolved)
            {
                CurrentAssignment = null;
                return;
            }

            // Шанс раскрыть преступление зависит от эффективности
            Random random = new Random();
            float solveChance = EfficiencyLevel / 100f;

            if (random.NextDouble() < solveChance * 0.1) // 10% за тик при 100% эффективности
            {
                CurrentAssignment.Solve();
                GainExperience();
                CurrentAssignment = null;
            }
        }

        /// <summary>
        /// Назначает полицейскому преступление для расследования
        /// </summary>
        public void AssignCrime(Crime crime)
        {
            CurrentAssignment = crime;
            crime.PoliceDispatched = true;
        }

        /// <summary>
        /// Полицейский получает опыт
        /// </summary>
        private void GainExperience()
        {
            ExperienceMonths++;
            EfficiencyLevel = Math.Min(100f, EfficiencyLevel + 0.5f);

            // Повышение звания
            if (ExperienceMonths >= 24 && Rank == "Officer")
            {
                Rank = "Senior Officer";
            }
            else if (ExperienceMonths >= 60 && Rank == "Senior Officer")
            {
                Rank = "Detective";
            }
        }

        /// <summary>
        /// Проверяет, доступен ли полицейский для нового задания
        /// </summary>
        public bool IsAvailable()
        {
            return IsOnDuty && CurrentAssignment == null;
        }
    }
}
