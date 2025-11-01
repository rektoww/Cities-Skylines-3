using Core.Interfaces;
using Core.Models.Base;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings
{
    /// <summary>
    /// Представляет строительную компанию, ответственную за строительные проекты. (офис строительной компании)
    /// </summary>
    public class ConstructionCompany : Building
    {
        private readonly List<(IConstructable project, ConstructionYard sourceYard)> _constructionProjects = new();

        /// <summary>
        /// Начало нового строительства.
        /// </summary>
        /// <param name="buildingToConstruct">Проект здания, реализующий IConstructable.</param>
        /// <param name="sourceYard">Строительная площадка, предоставляющая материалы.</param>
        /// <returns>True, если проект можно запустить, в противном случае — false.</returns>
        public bool StartNewProject(IConstructable buildingToConstruct, ConstructionYard sourceYard)
        {
            if (buildingToConstruct == null || sourceYard == null)
                return false;

            // Check if the source yard has enough materials to start
            foreach (var material in buildingToConstruct.RequiredMaterials)
            {
                if (!sourceYard.StoredMaterials.ContainsKey(material.Key) || sourceYard.StoredMaterials[material.Key] < material.Value)
                {
                    // Недостаточно материалов
                    return false;
                }
            }

            // Вывод материалов из строительной площадки
            foreach (var material in buildingToConstruct.RequiredMaterials)
            {
                sourceYard.TryRemoveMaterial(material.Key, material.Value, out int balance); // пока не используем balance
            }

            _constructionProjects.Add((buildingToConstruct, sourceYard));
            return true;
        }

        /// <summary>
        /// Этот метод следует вызывать при каждом такте моделирования для продвижения строительных проектов.
        /// </summary>
        public void UpdateProjects()
        {
            // Обработка проектов в обратном порядке, чтобы обеспечить безопасное удаление
            for (int i = _constructionProjects.Count - 1; i >= 0; i--)
            {
                var projectData = _constructionProjects[i];
                var project = projectData.project;

                if (!project.IsConstructed)
                {
                    // 1 единица прогресса за тик
                    // Можно позже привязать логику к доступным рабочим или оборудованию
                    double progressPerTick = 1.0 / project.ConstructionTime;
                    project.AdvanceConstruction(progressPerTick);
                }

                if (project.IsConstructed)
                {
                    // Удалить завершенный проект
                    _constructionProjects.RemoveAt(i);
                }
            }
        }

        public override void OnBuildingPlaced()
        {
            // Пока неясная логика
        }
    }
}