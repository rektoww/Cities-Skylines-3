﻿using Core.Enums;
using Core.Models.Base;

namespace Core.Models.HR
{
    /// <summary>
    /// Представляет отдел в организации. Содержит информацию о сотрудниках, бюджете и местоположении.
    /// </summary>
    public class Department
    {
        // Константы для ограничений отдела
        private const decimal DEFAULT_BUDGET = 100000m;
        private const int MAX_EMPLOYEES_PER_DEPARTMENT = 10;

        /// <summary>Название отдела.</summary>
        public string Name { get; set; }

        /// <summary>Тип отдела.</summary>
        public DepartmentType Type { get; set; }

        /// <summary>Здание, в котором расположен отдел.</summary>
        public Building Location { get; set; }

        /// <summary>Список сотрудников отдела.</summary>
        public List<Employee> Employees { get; set; } = new();

        /// <summary>Бюджет отдела для выплаты зарплат.</summary>
        public decimal Budget { get; set; } = DEFAULT_BUDGET;

        /// <summary>
        /// Проверка возможности найма сотрудника указанной профессии.
        /// </summary>
        /// <param name="profession">Профессия для проверки.</param>
        /// <returns>True если отдел может нанять сотрудника, иначе False.</returns>
        public bool CanHire(JobType profession)
        {
            return Employees.Count < MAX_EMPLOYEES_PER_DEPARTMENT;
        }

        /// <summary>
        /// Проверка возможности выплаты указанной зарплаты в рамках бюджета.
        /// </summary>
        /// <param name="salary">Зарплата для проверки.</param>
        /// <returns>True если бюджет позволяет выплачивать зарплату, иначе False.</returns>
        public bool CanAffordSalary(decimal salary)
        {
            return Budget >= salary;
        }
    }
}