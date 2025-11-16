using Core.Models.Mobs;

namespace Core.Services.Education
{
    public interface IEducationService
    {
        void StudyTick(Citizen citizen);
        void TryGraduate(Citizen citizen);
    }
}
