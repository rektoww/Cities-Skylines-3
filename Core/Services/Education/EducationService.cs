using Core.Models.Mobs;
using Core.Models.Buildings;

namespace Core.Services.Education
{
    public class EducationService : IEducationService
    {
        public void StudyTick(Citizen citizen)
        {
            if (citizen == null || !citizen.IsStudying || citizen.School == null) return;

            if (citizen.School is not ServiceBuilding serviceSchool)
            {
                citizen.IsStudying = false;
                citizen.School = null;
                return;
            }

            if (!serviceSchool.IsOperational)
            {
                citizen.Happiness = Math.Max(0, citizen.Happiness - 0.05f);
                citizen.AcademicProgress = Math.Max(0, citizen.AcademicProgress - 0.01f);
                return;
            }

            float baseIncrement = 1.0f;
            float perfMultiplier = Math.Clamp(citizen.AcademicPerformance / 100f, 0f, 2f);
            citizen.AcademicProgress = Math.Min(100f, citizen.AcademicProgress + baseIncrement * perfMultiplier);

            citizen.Health = Math.Min(100, citizen.Health + 0.01f);
            citizen.Happiness = Math.Min(100, citizen.Happiness + 0.01f);

            if (citizen.AcademicProgress >= 100f)
                TryGraduate(citizen);
        }

        public void TryGraduate(Citizen citizen)
        {
            if (citizen == null || !citizen.IsStudying || citizen.School == null) return;

            if (citizen.School is not ServiceBuilding serviceSchool) return;
            if (!serviceSchool.IsOperational) return;
            if (citizen.AcademicProgress < 100f) return;

            bool promoted = false;

            switch (citizen.Education)
            {
                case Core.Enums.EducationLevel.School:
                    citizen.Education = Core.Enums.EducationLevel.College;
                    promoted = true;
                    break;
                case Core.Enums.EducationLevel.College:
                    citizen.Education = Core.Enums.EducationLevel.University;
                    promoted = true;
                    break;
                case Core.Enums.EducationLevel.University:
                    promoted = true;
                    break;
            }

            if (promoted)
            {
                citizen.IsStudying = false;
                if (serviceSchool.Clients.Contains(citizen))
                    serviceSchool.Clients.Remove(citizen);
                citizen.School = null;
                citizen.AcademicProgress = 0f;
                citizen.Happiness = Math.Min(100, citizen.Happiness + 5f);
            }
        }
    }
}
