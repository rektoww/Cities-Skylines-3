using Core.Enums;
using Core.Models.Mobs;

namespace Core.Models.Police
{
    /// <summary>
    /// Представляет преступление в городе.
    /// Используется для моделирования криминальной активности и реакции полиции.
    /// </summary>
    public class Crime
    {
        public int Id { get; set; }
        public CrimeType Type { get; set; }
        public int LocationX { get; set; }
        public int LocationY { get; set; }
        public Citizen Criminal { get; set; }
        public Citizen Victim { get; set; }
        public int TimeCommitted { get; set; }
        public bool IsSolved { get; set; }
        public bool PoliceDispatched { get; set; }
        public int SeverityLevel { get; set; }

        /// <summary>
        /// Конструктор преступления
        /// </summary>
        public Crime(int id, CrimeType type, int x, int y, int gameTime)
        {
            Id = id;
            Type = type;
            LocationX = x;
            LocationY = y;
            TimeCommitted = gameTime;
            IsSolved = false;
            PoliceDispatched = false;

            // Устанавливаем уровень серьёзности в зависимости от типа
            SetSeverityLevel();
        }

        /// <summary>
        /// Устанавливает уровень серьёзности в зависимости от типа преступления
        /// </summary>
        private void SetSeverityLevel()
        {
            SeverityLevel = Type switch
            {
                CrimeType.Disturbance => 2,
                CrimeType.Theft => 4,
                CrimeType.Vandalism => 3,
                CrimeType.Robbery => 7,
                CrimeType.Assault => 8,
                _ => 5
            };
        }

        /// <summary>
        /// Помечает преступление как раскрытое
        /// </summary>
        public void Solve()
        {
            IsSolved = true;
        }

        /// <summary>
        /// Получает описание преступления
        /// </summary>
        public string GetDescription()
        {
            return $"{Type} at ({LocationX}, {LocationY}) - Severity: {SeverityLevel}";
        }
    }
}
