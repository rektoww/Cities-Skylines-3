using Core.Models.Mobs;

namespace Core.Services.Happiness
{
    public interface IHappinessService
    {
        void UpdateHappiness(Citizen citizen);
        void ApplyHappinessEffects(Citizen citizen);
    }
}
