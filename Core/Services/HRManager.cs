using Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.HR;
using Core.Models.Map;
using Core.Models.Mobs;

namespace Core.Services
{
    /// <summary>
    /// Управление персоналом организации. Обрабатывает создание отделов, найм и увольнение сотрудников.
    /// </summary>
    public class HRManager
    {
        // Константы для определения грейдов по профессиям
        private const EmployeeGrade SENIOR_GRADE = EmployeeGrade.Senior;
        private const EmployeeGrade SPECIALIST_GRADE = EmployeeGrade.Specialist;
        private const EmployeeGrade JUNIOR_GRADE = EmployeeGrade.Junior;

        // Профессии для старшего грейда
        private static readonly JobType[] SENIOR_PROFESSIONS = new[]
        {
            JobType.Manager,
            JobType.Doctor,
            JobType.Engineer
        };

        // Профессии для специалиста
        private static readonly JobType[] SPECIALIST_PROFESSIONS = new[]
        {
            JobType.Teacher,
            JobType.Pharmacist
        };

        private List<Department> _departments = new();
        private List<Employee> _employees = new();

        /// <summary>Список всех отделов организации.</summary>
        public IReadOnlyList<Department> Departments => _departments;

        /// <summary>Список всех сотрудников организации.</summary>
        public IReadOnlyList<Employee> Employees => _employees;

        /// <summary>
        /// Создание нового отдела с указанными параметрами.
        /// </summary>
        /// <param name="name">Название отдела.</param>
        /// <param name="type">Тип отдела.</param>
        /// <param name="location">Местоположение отдела.</param>
        /// <returns>Созданный отдел.</returns>
        public Department CreateDepartment(string name, DepartmentType type, Building location)
        {
            var department = new Department
            {
                Name = name,
                Type = type,
                Location = location
            };

            _departments.Add(department);
            return department;
        }

        /// <summary>
        /// Наем гражданина в указанный отдел с заданной зарплатой.
        /// </summary>
        /// <param name="citizen">Гражданин для найма.</param>
        /// <param name="department">Отдел для трудоустройства.</param>
        /// <param name="salary">Зарплата сотрудника.</param>
        /// <returns>Созданный сотрудник или null если найм невозможен.</returns>
        public Employee HireCitizen(Citizen citizen, Department department, decimal salary)
        {
            if (!department.CanHire(citizen.Profession) || !department.CanAffordSalary(salary))
                return null;

            // Автоматически определяем грейд по профессии
            var grade = DetermineGradeByProfession(citizen.Profession);

            var employee = new Employee(citizen, grade, department, salary);

            department.Employees.Add(employee);
            _employees.Add(employee);

            // Вычитаем зарплату из бюджета
            department.Budget -= salary;

            return employee;
        }

        /// <summary>
        /// Определение грейда сотрудника по профессии.
        /// </summary>
        /// <param name="profession">Профессия сотрудника.</param>
        /// <returns>Соответствующий грейд.</returns>
        private EmployeeGrade DetermineGradeByProfession(JobType profession)
        {
            if (SENIOR_PROFESSIONS.Contains(profession))
                return SENIOR_GRADE;

            if (SPECIALIST_PROFESSIONS.Contains(profession))
                return SPECIALIST_GRADE;

            return JUNIOR_GRADE;
        }

        /// <summary>
        /// Увольнение сотрудника из организации.
        /// </summary>
        /// <param name="employee">Сотрудник для увольнения.</param>
        /// <returns>True если увольнение прошло успешно, иначе False.</returns>
        public bool FireEmployee(Employee employee)
        {
            if (employee.Department == null) return false;

            employee.Department.Employees.Remove(employee);
            employee.Department.Budget += employee.Salary; // Возвращаем бюджет
            _employees.Remove(employee);

            return true;
        }

        /// <summary>
        /// Получение списка сотрудников по указанной профессии.
        /// </summary>
        /// <param name="profession">Профессия для фильтрации.</param>
        /// <returns>Список сотрудников с указанной профессией.</returns>
        public List<Employee> GetEmployeesByProfession(JobType profession)
        {
            return _employees.Where(e => e.Citizen.Profession == profession).ToList();
        }

        /// <summary>
        /// Получение общего количества сотрудников в организации.
        /// </summary>
        /// <returns>Количество сотрудников.</returns>
        public int GetTotalEmployees() => _employees.Count;

        /// <summary>
        /// Получение общей суммы зарплат всех сотрудников.
        /// </summary>
        /// <returns>Сумма всех зарплат.</returns>
        public decimal GetTotalSalaries() => _employees.Sum(e => e.Salary);
    }
}