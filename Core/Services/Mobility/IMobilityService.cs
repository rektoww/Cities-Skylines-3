using Core.Models.Mobs;

namespace Core.Services.Mobility
{
    public interface IMobilityService
    {
        void MoveCitizen(Citizen citizen);
        Core.Models.Base.TransitStation FindNearestTransitStation(Citizen citizen);
        bool TryBoardTransport(Citizen citizen, Core.Models.Base.Transport transport);
        bool TryDisembarkTransport(Citizen citizen);
    }
}
