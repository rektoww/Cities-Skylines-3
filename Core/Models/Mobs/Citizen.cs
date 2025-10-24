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
