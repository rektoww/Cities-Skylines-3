using Core.Enums;
using Core.Models.Mobs;

namespace Core.Models.HR
{
    /// <summary>
    /// Представляет сотрудника в системе управления персоналом. Содержит информацию о гражданине, грейде, отделе и зарплате.
    /// </summary>
    public class Employee
    {
        // Формат строкового представления
        private const string TO_STRING_FORMAT = "{0} - {1} ({2}) - {3:C}";

        // Гражданин, являющийся сотрудником
        public Citizen Citizen { get; set; }

        // Грейд сотрудника в организации
        public EmployeeGrade Grade { get; set; }

        // Отдел, в котором работает сотрудник
        public Department Department { get; set; }

        // Зарплата сотрудника
        public decimal Salary { get; set; }

        /// <summary>
        /// Создание сотрудника с указанными параметрами.
        /// </summary>
        /// <param name="citizen">Гражданин для найма.</param>
        /// <param name="grade">Грейд сотрудника.</param>
        /// <param name="department">Отдел трудоустройства.</param>
        /// <param name="salary">Размер зарплаты.</param>
        public Employee(Citizen citizen, EmployeeGrade grade, Department department, decimal salary)
        {
            Citizen = citizen;
            Grade = grade;
            Department = department;
            Salary = salary;
        }

        /// <summary>
        /// Получение строкового представления сотрудника.
        /// </summary>
        /// <returns>Строка с идентификатором, профессией, грейдом и зарплатой.</returns>
        public override string ToString()
        {
            return string.Format(TO_STRING_FORMAT, Citizen.Id, Citizen.Profession, Grade, Salary);
        }
    }
}