using Core.Models.Mobs;

namespace Core.Services.Population
{
    public class PopulationService : IPopulationService
    {
        private readonly List<Citizen> _citizens = new();
        private readonly Random _random = new();

        public void RegisterCitizen(Citizen citizen)
        {
            if (!_citizens.Contains(citizen)) _citizens.Add(citizen);
        }

        public void UnregisterCitizen(Citizen citizen)
        {
            if (_citizens.Contains(citizen)) _citizens.Remove(citizen);
        }

        public void UpdatePopulationTick()
        {
            // Placeholder: iterate citizens and handle lifecycle events via services
            foreach (var c in _citizens)
            {
                // e.g., aging, mortality, etc. Delegation to other services would happen here.
            }
        }

        public bool TryReproduce(Citizen parentA, Citizen parentB)
        {
            if (parentA == null || parentB == null) return false;
            if (!parentA.CanReproduce || !parentB.CanReproduce) return false;

            // base 10% chance, can be influenced by other services
            if (_random.NextDouble() < 0.1)
            {
                // create child - minimal initialization
                var child = new Citizen(parentA.X, parentA.Y, parentA.GameMap)
                {
                    Age = 0,
                    IsMale = _random.Next(0, 2) == 0,
                    Home = parentA.Home ?? parentB.Home
                };

                RegisterCitizen(child);
                parentA.Children.Add(child);
                parentB.Children.Add(child);
                return true;
            }

            return false;
        }
    }
}
