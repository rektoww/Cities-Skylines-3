using Core.Models.Mobs;

namespace Core.Services.Population
{
    public interface IPopulationService
    {
        void UpdatePopulationTick();
        bool TryReproduce(Citizen parentA, Citizen parentB);
        void RegisterCitizen(Citizen citizen);
        void UnregisterCitizen(Citizen citizen);
    }
}
