using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Mobs;
using Core.Models.Buildings;
using Core.Models.Map;
using Core.Enums;
using System.Linq;

namespace Tests.UnitTests
{
    [TestClass]
    public sealed class EducationTests
    {
        [TestMethod]
        public void Study_IncreasesAcademicProgress_WhenSchoolOperational()
        {
            // Arrange
            var citizen = new Citizen(0, 0, null);
            citizen.Education = EducationLevel.School;
            citizen.IsStudying = true;

            var school = new SchoolBuilding();
            // Включаем все коммуникации, чтобы школа работала
            school.HasElectricity = true;
            school.HasWater = true;
            school.HasGas = true;
            school.HasSewage = true;

            // Записываем в школу
            bool enrolled = school.TryAddClient(citizen);
            citizen.School = school;

            // Act
            for (int i = 0; i < 5; i++)
                citizen.Study();

            // Assert
            Assert.IsTrue(citizen.AcademicProgress > 0f, "Прогресс должен увеличиться при рабочей школе");
        }

        [TestMethod]
        public void Graduate_OnFullProgress_RemovesFromSchoolAndUpdatesEducation()
        {
            // Arrange
            var citizen = new Citizen(0, 0, null);
            citizen.Education = EducationLevel.School;
            citizen.IsStudying = true;
            citizen.AcademicPerformance = 100f; // максимальная успеваемость для быстрой проверки

            var school = new SchoolBuilding();
            school.HasElectricity = true;
            school.HasWater = true;
            school.HasGas = true;
            school.HasSewage = true;

            school.TryAddClient(citizen);
            citizen.School = school;

            // Act - симулируем достаточно тактов, чтобы накопить прогресс
            int maxTicks = 200;
            int ticks = 0;
            while (citizen.IsStudying && ticks++ < maxTicks)
            {
                citizen.Study();
            }

            // Assert
            Assert.IsFalse(citizen.IsStudying, "После выпуска IsStudying должен стать false");
            Assert.AreEqual(EducationLevel.College, citizen.Education, "После завершения уровня Education должен повыситься");
            Assert.IsFalse(school.Clients.Contains(citizen), "Гражданин должен быть удалён из clients школы после выпуска");
        }

        [TestMethod]
        public void Study_DoesNotProgress_WhenSchoolNotOperational()
        {
            // Arrange
            var citizen = new Citizen(0, 0, null);
            citizen.Education = EducationLevel.School;
            citizen.IsStudying = true;

            var school = new SchoolBuilding();
            // По умолчанию коммуникации выключены — школа неоперабельна

            school.TryAddClient(citizen);
            citizen.School = school;

            // Act
            for (int i = 0; i < 5; i++)
                citizen.Study();

            // Assert
            Assert.AreEqual(0f, citizen.AcademicProgress, "Прогресс не должен накапливаться, если школа не работает");
            Assert.IsTrue(citizen.IsStudying, "Гражданин всё ещё должен быть в состоянии учиться, просто прогресс не растёт");
        }
    }
}