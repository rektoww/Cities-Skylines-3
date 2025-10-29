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

        /// <summary>
        /// Обновляет счастье жителя в зависимости от окружающей инфраструктуры
        /// </summary>
        public void UpdateHappinessBasedOnInfrastructure()
        {
            // Проверяем координаты и получаем тайл напрямую из Tiles[,]
            if (GameMap?.Tiles == null) return;
            if (X < 0 || X >= GameMap.Width || Y < 0 || Y >= GameMap.Height) return;

            var currentTile = GameMap.Tiles[X, Y];

            if (currentTile == null) return;

            if (currentTile.HasPark)
                Happiness = Math.Min(100, Happiness + 0.1f);

            if (currentTile.HasBikeLane)
                Happiness = Math.Min(100, Happiness + 0.05f);

            if (currentTile.HasPedestrianPath)
                Happiness = Math.Min(100, Happiness + 0.03f);

            if (Home?.IsOperational == true)
                Happiness = Math.Min(100, Happiness + 0.02f);

            if (Workplace?.IsOperational == true)
                Happiness = Math.Min(100, Happiness + 0.01f);

            Happiness = Math.Max(0, Happiness - 0.01f);
        }

        /// <summary>
        /// Влияние счастья на здоровье и продуктивность
        /// </summary>
        public void ApplyHappinessEffects()
        {
            // Счастье влияет на здоровье
            if (Happiness > 70f)
                Health = Math.Min(100, Health + 0.05f);
            else if (Happiness < 30f)
                Health = Math.Max(0, Health - 0.1f);

            // Счастье влияет на шанс размножения
            if (Happiness > 80f && CanReproduce)
            {
                var random = new Random();
                if (random.NextDouble() < 0.15) // Увеличенный шанс при высоком счастье
                    TryReproduce();
            }
        }

        public override void Move()
        {
            // мув
        }

        /// <summary>
        /// ПОРНО!!!! НЕ СМОТРЕТЬ 18+ 
        /// </summary>
        /// <returns></returns>
        public bool TryReproduce()
        {
            if (!CanReproduce || Partner == null) return false;
        
            var random = new Random();
            if (random.NextDouble() < 0.1)
                return true;

            return false;
        }

        public void Work()
        {
            if (!IsEmployed || Workplace == null) return;

            // ворк ворк ворк ворк ворк ворк
        }

        public void Study()
        {
            if (!IsStudying || School == null) return;

            if (Age >= 18 && Education == EducationLevel.School)
                Education = EducationLevel.University;

            // добить дальше похуй
        }
    }
}
