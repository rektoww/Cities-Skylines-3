using Core.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Tests.UnitTests
{
    /// <summary>
    /// Набор тестов для проверки функциональности системы управления персоналом.
    /// </summary>
    [TestClass]
    public class SimpleHRManagerTests
    {
        // Константы для бюджетов
        private const decimal DEFAULT_DEPARTMENT_BUDGET = 1000000m;
        private const decimal SMALL_BUDGET = 10000m;
        private const decimal LOW_SALARY_THRESHOLD = 20000m;

        // Константы для зарплат
        private const decimal SALARY_35000 = 35000m;
        private const decimal SALARY_38000 = 38000m;
        private const decimal SALARY_50000 = 50000m;
        private const decimal SALARY_60000 = 60000m;
        private const decimal SALARY_70000 = 70000m;
        private const decimal SALARY_80000 = 80000m;
        private const decimal SALARY_30000 = 30000m;
        private const decimal SALARY_32000 = 32000m;

        // Константы для количества сотрудников
        private const int MAX_EMPLOYEES_PER_DEPARTMENT = 10;
        private const int EMPLOYEE_COUNT_FOR_LIMIT_TEST = 10;
        private const int EMPLOYEE_COUNT_FOR_UNDER_LIMIT_TEST = 5;
        private const int TOTAL_EMPLOYEES_FOR_COUNT_TEST = 5;
        private const int TOTAL_EMPLOYEES_FOR_SALARY_TEST = 3;

        // Константы для идентификаторов
        private const int DEFAULT_CITIZEN_ID = 1;
        private const int TEST_CITIZEN_ID = 123;

        // Сообщения для утверждений
        private const string MANAGER_SHOULD_BE_HIRED_MESSAGE = "Manager employee should not be null";
        private const string DOCTOR_SHOULD_BE_HIRED_MESSAGE = "Doctor employee should not be null";
        private const string TEACHER_SHOULD_BE_HIRED_MESSAGE = "Teacher employee should not be null";
        private const string WORKER_SHOULD_BE_HIRED_MESSAGE = "Worker employee should not be null";
        private const string SALESMAN1_SHOULD_BE_HIRED_MESSAGE = "Salesman 1 should be hired";
        private const string SALESMAN2_SHOULD_BE_HIRED_MESSAGE = "Salesman 2 should be hired";
        private const string ENGINEER_SHOULD_BE_HIRED_MESSAGE = "Engineer should be hired";
        private const string CANNOT_HIRE_AT_LIMIT_MESSAGE = "Should return false when department has 10 employees";
        private const string CAN_HIRE_UNDER_LIMIT_MESSAGE = "Should return true when department has less than 10 employees";

        // Форматы строк для сообщений
        private const string EXPECTED_EMPLOYEES_FORMAT = "Expected {0} employees, but got {1}";
        private const string EXPECTED_SALESMEN_FORMAT = "Expected {0} salesmen, but got {1}. Total employees: {2}";
        private const string EXPECTED_ENGINEERS_FORMAT = "Expected {0} engineer, but got {1}";
        private const string EMPLOYEE_HIRED_FORMAT = "Employee {0} should be hired";
        private const string TOTAL_SALARIES_FORMAT = "Expected {0}, but got {1}. Employees count: {2}";

        /// <summary>Тестовый класс здания для изоляции тестов.</summary>
        private class TestBuilding
        {
            public int X { get; set; } = 0;
            public int Y { get; set; } = 0;
        }

        /// <summary>Тестовый класс гражданина для изоляции тестов.</summary>
        private class TestCitizen
        {
            public int Id { get; set; }
            public JobType Profession { get; set; }
            public bool IsEmployed { get; set; }
        }

        /// <summary>Тестовый класс отдела для изоляции тестов.</summary>
        private class TestDepartment
        {
            public string Name { get; set; }
            public DepartmentType Type { get; set; }
            public TestBuilding Location { get; set; }
            public List<TestEmployee> Employees { get; set; } = new List<TestEmployee>();
            public decimal Budget { get; set; } = DEFAULT_DEPARTMENT_BUDGET;

            /// <summary>Проверка возможности найма сотрудника.</summary>
            public bool CanHire(JobType profession)
            {
                return Employees.Count < MAX_EMPLOYEES_PER_DEPARTMENT;
            }

            /// <summary>Проверка возможности выплаты зарплаты.</summary>
            public bool CanAffordSalary(decimal salary)
            {
                return Budget >= salary;
            }
        }

        /// <summary>Тестовый класс сотрудника для изоляции тестов.</summary>
        private class TestEmployee
        {
            public TestCitizen Citizen { get; set; }
            public EmployeeGrade Grade { get; set; }
            public TestDepartment Department { get; set; }
            public decimal Salary { get; set; }

            public TestEmployee(TestCitizen citizen, EmployeeGrade grade, TestDepartment department, decimal salary)
            {
                Citizen = citizen;
                Grade = grade;
                Department = department;
                Salary = salary;
            }

            public override string ToString()
            {
                return $"{Citizen.Id} - {Citizen.Profession} ({Grade}) - {Salary:C}";
            }
        }

        /// <summary>Тестовый класс менеджера персонала для изоляции тестов.</summary>
        private class TestHRManager
        {
            public List<TestDepartment> Departments { get; } = new List<TestDepartment>();
            public List<TestEmployee> Employees { get; } = new List<TestEmployee>();

            /// <summary>Создание тестового отдела.</summary>
            public TestDepartment CreateDepartment(string name, DepartmentType type, TestBuilding location)
            {
                var department = new TestDepartment
                {
                    Name = name,
                    Type = type,
                    Location = location
                };
                Departments.Add(department);
                return department;
            }

            /// <summary>Наем тестового гражданина в отдел.</summary>
            public TestEmployee HireCitizen(TestCitizen citizen, TestDepartment department, decimal salary)
            {
                if (!department.CanHire(citizen.Profession) || !department.CanAffordSalary(salary))
                    return null;

                var grade = citizen.Profession switch
                {
                    JobType.Manager or JobType.Doctor or JobType.Engineer => EmployeeGrade.Senior,
                    JobType.Teacher or JobType.Pharmacist => EmployeeGrade.Specialist,
                    _ => EmployeeGrade.Junior
                };

                var employee = new TestEmployee(citizen, grade, department, salary);
                department.Employees.Add(employee);
                Employees.Add(employee);
                department.Budget -= salary;

                return employee;
            }

            /// <summary>Увольнение тестового сотрудника.</summary>
            public bool FireEmployee(TestEmployee employee)
            {
                if (employee.Department == null || !Employees.Contains(employee))
                    return false;

                employee.Department.Employees.Remove(employee);
                employee.Department.Budget += employee.Salary;
                Employees.Remove(employee);
                return true;
            }

            /// <summary>Получение сотрудников по профессии.</summary>
            public List<TestEmployee> GetEmployeesByProfession(JobType profession)
            {
                return Employees.Where(e => e.Citizen.Profession == profession).ToList();
            }

            /// <summary>Получение общего количества сотрудников.</summary>
            public int GetTotalEmployees() => Employees.Count;

            /// <summary>Получение общей суммы зарплат.</summary>
            public decimal GetTotalSalaries() => Employees.Sum(e => e.Salary);
        }

        /// <summary>Создание тестового гражданина с указанной профессией.</summary>
        private TestCitizen CreateTestCitizen(JobType profession, int id = DEFAULT_CITIZEN_ID)
        {
            return new TestCitizen
            {
                Id = id,
                Profession = profession,
                IsEmployed = false
            };
        }

        /// <summary>Проверка создания отдела и его добавления в список.</summary>
        [TestMethod]
        public void CreateDepartment_ShouldAddDepartmentToList()
        {
            // Arrange
            var hrManager = new TestHRManager();
            var building = new TestBuilding();

            // Act
            var department = hrManager.CreateDepartment("IT отдел", DepartmentType.Administration, building);

            // Assert
            Assert.AreEqual(1, hrManager.Departments.Count);
            Assert.AreEqual("IT отдел", department.Name);
            Assert.AreEqual(DepartmentType.Administration, department.Type);
            Assert.AreEqual(building, department.Location);
        }

        /// <summary>Проверка успешного найма сотрудника при возможности отдела.</summary>
        [TestMethod]
        public void HireCitizen_ShouldAddEmployee_WhenDepartmentCanHire()
        {
            // Arrange
            var hrManager = new TestHRManager();
            var department = hrManager.CreateDepartment("Отдел продаж", DepartmentType.Commercial, new TestBuilding());
            var citizen = CreateTestCitizen(JobType.Salesman);

            // Act
            var employee = hrManager.HireCitizen(citizen, department, SALARY_35000);

            // Assert
            Assert.IsNotNull(employee);
            Assert.AreEqual(1, hrManager.Employees.Count);
            Assert.AreEqual(citizen, employee.Citizen);
            Assert.AreEqual(department, employee.Department);
            Assert.AreEqual(SALARY_35000, employee.Salary);
            Assert.AreEqual(EmployeeGrade.Junior, employee.Grade);
        }

        /// <summary>Проверка невозможности найма при недостаточном бюджете.</summary>
        [TestMethod]
        public void HireCitizen_ShouldReturnNull_WhenDepartmentCannotAffordSalary()
        {
            // Arrange
            var hrManager = new TestHRManager();
            var department = hrManager.CreateDepartment("Маленький отдел", DepartmentType.Administration, new TestBuilding());
            department.Budget = SMALL_BUDGET;
            var citizen = CreateTestCitizen(JobType.Manager);

            // Act
            var employee = hrManager.HireCitizen(citizen, department, SALARY_50000);

            // Assert
            Assert.IsNull(employee);
            Assert.AreEqual(0, hrManager.Employees.Count);
        }

        /// <summary>Проверка корректного определения грейда для различных профессий.</summary>
        [TestMethod]
        public void HireCitizen_ShouldSetCorrectGrade_ForDifferentProfessions()
        {
            // Arrange
            var hrManager = new TestHRManager();

            // СОЗДАЕМ ОТДЕЛЬНЫЕ ОТДЕЛЫ ДЛЯ КАЖДОГО СОТРУДНИКА чтобы хватило бюджета
            var managerDept = hrManager.CreateDepartment("Для менеджера", DepartmentType.Administration, new TestBuilding());
            var doctorDept = hrManager.CreateDepartment("Для врача", DepartmentType.Administration, new TestBuilding());
            var teacherDept = hrManager.CreateDepartment("Для учителя", DepartmentType.Administration, new TestBuilding());
            var workerDept = hrManager.CreateDepartment("Для рабочего", DepartmentType.Administration, new TestBuilding());

            var manager = CreateTestCitizen(JobType.Manager);
            var doctor = CreateTestCitizen(JobType.Doctor);
            var teacher = CreateTestCitizen(JobType.Teacher);
            var worker = CreateTestCitizen(JobType.Worker);

            // Act - нанимаем каждого в свой отдел
            var managerEmployee = hrManager.HireCitizen(manager, managerDept, SALARY_80000);
            var doctorEmployee = hrManager.HireCitizen(doctor, doctorDept, SALARY_70000);
            var teacherEmployee = hrManager.HireCitizen(teacher, teacherDept, SALARY_50000);
            var workerEmployee = hrManager.HireCitizen(worker, workerDept, SALARY_30000);

            // Assert
            Assert.IsNotNull(managerEmployee, MANAGER_SHOULD_BE_HIRED_MESSAGE);
            Assert.IsNotNull(doctorEmployee, DOCTOR_SHOULD_BE_HIRED_MESSAGE);
            Assert.IsNotNull(teacherEmployee, TEACHER_SHOULD_BE_HIRED_MESSAGE);
            Assert.IsNotNull(workerEmployee, WORKER_SHOULD_BE_HIRED_MESSAGE);

            Assert.AreEqual(EmployeeGrade.Senior, managerEmployee.Grade);
            Assert.AreEqual(EmployeeGrade.Senior, doctorEmployee.Grade);
            Assert.AreEqual(EmployeeGrade.Specialist, teacherEmployee.Grade);
            Assert.AreEqual(EmployeeGrade.Junior, workerEmployee.Grade);
        }

        /// <summary>Проверка уменьшения бюджета отдела после найма сотрудника.</summary>
        [TestMethod]
        public void HireCitizen_ShouldReduceDepartmentBudget()
        {
            // Arrange
            var hrManager = new TestHRManager();
            var department = hrManager.CreateDepartment("Финансовый отдел", DepartmentType.Administration, new TestBuilding());
            decimal initialBudget = department.Budget;
            var citizen = CreateTestCitizen(JobType.Engineer);
            decimal salary = SALARY_60000;

            // Act
            var employee = hrManager.HireCitizen(citizen, department, salary);

            // Assert
            Assert.IsNotNull(employee);
            Assert.AreEqual(initialBudget - salary, department.Budget);
        }

        /// <summary>Проверка увольнения сотрудника и возврата бюджета.</summary>
        [TestMethod]
        public void FireEmployee_ShouldRemoveEmployee_AndReturnBudget()
        {
            // Arrange
            var hrManager = new TestHRManager();
            var department = hrManager.CreateDepartment("Тестовый отдел", DepartmentType.Production, new TestBuilding());
            var citizen = CreateTestCitizen(JobType.Worker);
            decimal salary = SALARY_32000;

            var employee = hrManager.HireCitizen(citizen, department, salary);
            decimal budgetAfterHire = department.Budget;

            // Act
            var result = hrManager.FireEmployee(employee);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, hrManager.Employees.Count);
            Assert.AreEqual(0, department.Employees.Count);
            Assert.AreEqual(budgetAfterHire + salary, department.Budget);
        }

        /// <summary>Проверка обработки попытки уволить несуществующего сотрудника.</summary>
        [TestMethod]
        public void FireEmployee_ShouldReturnFalse_WhenEmployeeNotFound()
        {
            // Arrange
            var hrManager = new TestHRManager();
            var department = hrManager.CreateDepartment("Отдел", DepartmentType.Administration, new TestBuilding());
            var citizen = CreateTestCitizen(JobType.Manager);
            var employee = new TestEmployee(citizen, EmployeeGrade.Senior, department, SALARY_80000);

            // Act
            var result = hrManager.FireEmployee(employee);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>Проверка фильтрации сотрудников по профессии.</summary>
        [TestMethod]
        public void GetEmployeesByProfession_ShouldReturnCorrectList()
        {
            // Arrange
            var hrManager = new TestHRManager();

            // СОЗДАЕМ НЕСКОЛЬКО ОТДЕЛОВ чтобы хватило бюджета
            var dept1 = hrManager.CreateDepartment("Отдел 1", DepartmentType.Administration, new TestBuilding());
            var dept2 = hrManager.CreateDepartment("Отдел 2", DepartmentType.Administration, new TestBuilding());
            var dept3 = hrManager.CreateDepartment("Отдел 3", DepartmentType.Administration, new TestBuilding());

            var salesman1 = CreateTestCitizen(JobType.Salesman);
            var salesman2 = CreateTestCitizen(JobType.Salesman);
            var engineer = CreateTestCitizen(JobType.Engineer);

            // Act - нанимаем в разные отделы
            var employee1 = hrManager.HireCitizen(salesman1, dept1, SALARY_35000);
            var employee2 = hrManager.HireCitizen(salesman2, dept2, SALARY_38000);
            var employee3 = hrManager.HireCitizen(engineer, dept3, SALARY_60000);

            // Проверяем что все наняты успешно
            Assert.IsNotNull(employee1, SALESMAN1_SHOULD_BE_HIRED_MESSAGE);
            Assert.IsNotNull(employee2, SALESMAN2_SHOULD_BE_HIRED_MESSAGE);
            Assert.IsNotNull(employee3, ENGINEER_SHOULD_BE_HIRED_MESSAGE);

            var salesmen = hrManager.GetEmployeesByProfession(JobType.Salesman);
            var engineers = hrManager.GetEmployeesByProfession(JobType.Engineer);
            var doctors = hrManager.GetEmployeesByProfession(JobType.Doctor);

            // Assert
            Assert.AreEqual(2, salesmen.Count, string.Format(EXPECTED_SALESMEN_FORMAT, 2, salesmen.Count, hrManager.Employees.Count));
            Assert.AreEqual(1, engineers.Count, string.Format(EXPECTED_ENGINEERS_FORMAT, 1, engineers.Count));
            Assert.AreEqual(0, doctors.Count);
        }

        /// <summary>Проверка подсчета общего количества сотрудников.</summary>
        [TestMethod]
        public void GetTotalEmployees_ShouldReturnCorrectCount()
        {
            // Arrange
            var hrManager = new TestHRManager();

            // СОЗДАЕМ НЕСКОЛЬКО ОТДЕЛОВ для 5 сотрудников
            var dept1 = hrManager.CreateDepartment("Отдел 1", DepartmentType.Production, new TestBuilding());
            var dept2 = hrManager.CreateDepartment("Отдел 2", DepartmentType.Production, new TestBuilding());
            var dept3 = hrManager.CreateDepartment("Отдел 3", DepartmentType.Production, new TestBuilding());

            // Act - нанимаем 5 сотрудников в разные отделы
            var employees = new List<TestEmployee>();
            for (int i = 0; i < TOTAL_EMPLOYEES_FOR_COUNT_TEST; i++)
            {
                var citizen = CreateTestCitizen(JobType.Worker, i + 1);
                // Распределяем по разным отделам
                var department = i % 3 == 0 ? dept1 : (i % 3 == 1 ? dept2 : dept3);
                var employee = hrManager.HireCitizen(citizen, department, SALARY_30000);
                Assert.IsNotNull(employee, string.Format(EMPLOYEE_HIRED_FORMAT, i + 1));
                employees.Add(employee);
            }

            var total = hrManager.GetTotalEmployees();

            // Assert
            Assert.AreEqual(TOTAL_EMPLOYEES_FOR_COUNT_TEST, total, string.Format(EXPECTED_EMPLOYEES_FORMAT, TOTAL_EMPLOYEES_FOR_COUNT_TEST, total));
        }

        /// <summary>Проверка подсчета общей суммы зарплат.</summary>
        [TestMethod]
        public void GetTotalSalaries_ShouldReturnCorrectSum()
        {
            // Arrange
            var hrManager = new TestHRManager();

            // СОЗДАЕМ ОТДЕЛЬНЫЕ ОТДЕЛЫ для каждого сотрудника
            var dept1 = hrManager.CreateDepartment("Для менеджера", DepartmentType.Administration, new TestBuilding());
            var dept2 = hrManager.CreateDepartment("Для инженера", DepartmentType.Administration, new TestBuilding());
            var dept3 = hrManager.CreateDepartment("Для рабочего", DepartmentType.Administration, new TestBuilding());

            // Act - нанимаем сотрудников в разные отделы
            var employee1 = hrManager.HireCitizen(CreateTestCitizen(JobType.Manager), dept1, SALARY_80000);
            var employee2 = hrManager.HireCitizen(CreateTestCitizen(JobType.Engineer), dept2, SALARY_60000);
            var employee3 = hrManager.HireCitizen(CreateTestCitizen(JobType.Worker), dept3, SALARY_30000);

            Assert.IsNotNull(employee1, "Manager should be hired");
            Assert.IsNotNull(employee2, "Engineer should be hired");
            Assert.IsNotNull(employee3, "Worker should be hired");

            var totalSalaries = hrManager.GetTotalSalaries();

            // Assert
            decimal expectedTotal = SALARY_80000 + SALARY_60000 + SALARY_30000;
            Assert.AreEqual(expectedTotal, totalSalaries, string.Format(TOTAL_SALARIES_FORMAT, expectedTotal, totalSalaries, hrManager.Employees.Count));
        }

        /// <summary>Проверка ограничения на количество сотрудников в отделе.</summary>
        [TestMethod]
        public void Department_CanHire_ShouldReturnFalse_WhenOverLimit()
        {
            // Arrange
            var department = new TestDepartment();

            // Добавляем 10 сотрудников напрямую (имитируем переполнение)
            for (int i = 0; i < EMPLOYEE_COUNT_FOR_LIMIT_TEST; i++)
            {
                var citizen = CreateTestCitizen(JobType.Salesman, i + 1);
                department.Employees.Add(new TestEmployee(citizen, EmployeeGrade.Junior, department, SALARY_30000));
            }

            // Act
            var canHire = department.CanHire(JobType.Salesman);

            // Assert
            Assert.IsFalse(canHire, CANNOT_HIRE_AT_LIMIT_MESSAGE);
        }

        /// <summary>Проверка возможности найма при наличии свободных мест в отделе.</summary>
        [TestMethod]
        public void Department_CanHire_ShouldReturnTrue_WhenUnderLimit()
        {
            // Arrange
            var department = new TestDepartment();

            // Добавляем 5 сотрудников
            for (int i = 0; i < EMPLOYEE_COUNT_FOR_UNDER_LIMIT_TEST; i++)
            {
                var citizen = CreateTestCitizen(JobType.Worker, i + 1);
                department.Employees.Add(new TestEmployee(citizen, EmployeeGrade.Junior, department, SALARY_30000));
            }

            // Act
            var canHire = department.CanHire(JobType.Worker);

            // Assert
            Assert.IsTrue(canHire, CAN_HIRE_UNDER_LIMIT_MESSAGE);
        }

        /// <summary>Проверка достаточности бюджета для выплаты зарплаты.</summary>
        [TestMethod]
        public void Department_CanAffordSalary_ShouldReturnTrue_WhenBudgetIsSufficient()
        {
            // Arrange
            var department = new TestDepartment
            {
                Budget = SALARY_50000
            };

            // Act
            var canAfford = department.CanAffordSalary(SALARY_30000);

            // Assert
            Assert.IsTrue(canAfford);
        }

        /// <summary>Проверка недостаточности бюджета для выплаты зарплаты.</summary>
        [TestMethod]
        public void Department_CanAffordSalary_ShouldReturnFalse_WhenBudgetIsInsufficient()
        {
            // Arrange
            var department = new TestDepartment
            {
                Budget = LOW_SALARY_THRESHOLD
            };

            // Act
            var canAfford = department.CanAffordSalary(SALARY_30000);

            // Assert
            Assert.IsFalse(canAfford);
        }

        /// <summary>Проверка корректной инициализации свойств сотрудника.</summary>
        [TestMethod]
        public void Employee_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var citizen = CreateTestCitizen(JobType.Engineer);
            var department = new TestDepartment();
            var grade = EmployeeGrade.Senior;
            var salary = SALARY_60000;

            // Act
            var employee = new TestEmployee(citizen, grade, department, salary);

            // Assert
            Assert.AreEqual(citizen, employee.Citizen);
            Assert.AreEqual(grade, employee.Grade);
            Assert.AreEqual(department, employee.Department);
            Assert.AreEqual(salary, employee.Salary);
        }

        /// <summary>Проверка форматирования строкового представления сотрудника.</summary>
        [TestMethod]
        public void Employee_ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var citizen = CreateTestCitizen(JobType.Manager);
            citizen.Id = TEST_CITIZEN_ID;
            var department = new TestDepartment();
            var employee = new TestEmployee(citizen, EmployeeGrade.Senior, department, SALARY_80000);

            // Act
            var result = employee.ToString();

            // Assert
            StringAssert.Contains(result, TEST_CITIZEN_ID.ToString());
            StringAssert.Contains(result, "Manager");
            StringAssert.Contains(result, "Senior");
        }

        /// <summary>Проверка установки бюджета по умолчанию при создании отдела.</summary>
        [TestMethod]
        public void Department_Constructor_ShouldSetDefaultBudget()
        {
            // Act
            var department = new TestDepartment();

            // Assert
            Assert.AreEqual(DEFAULT_DEPARTMENT_BUDGET, department.Budget);
        }

        /// <summary>Проверка пустого списка сотрудников в новом отделе.</summary>
        [TestMethod]
        public void Department_EmployeesList_ShouldBeEmpty_OnNewDepartment()
        {
            // Act
            var department = new TestDepartment();

            // Assert
            Assert.AreEqual(0, department.Employees.Count);
        }

        /// <summary>Проверка начального состояния менеджера персонала.</summary>
        [TestMethod]
        public void SimpleHRManager_InitialState_ShouldBeEmpty()
        {
            // Arrange & Act
            var hrManager = new TestHRManager();

            // Assert
            Assert.AreEqual(0, hrManager.Departments.Count);
            Assert.AreEqual(0, hrManager.Employees.Count);
        }

        /// <summary>Проверка невозможности найма при достижении лимита сотрудников в отделе.</summary>
        [TestMethod]
        public void HireCitizen_ShouldNotHire_WhenDepartmentAtLimit()
        {
            // Arrange
            var hrManager = new TestHRManager();
            var department = hrManager.CreateDepartment("Полный отдел", DepartmentType.Commercial, new TestBuilding());

            // Заполняем отдел до предела напрямую (имитируем переполнение)
            for (int i = 0; i < EMPLOYEE_COUNT_FOR_LIMIT_TEST; i++)
            {
                var citizen = CreateTestCitizen(JobType.Salesman, i + 1);
                department.Employees.Add(new TestEmployee(citizen, EmployeeGrade.Junior, department, SALARY_30000));
                hrManager.Employees.Add(department.Employees[i]); // Добавляем также в общий список
            }

            var extraCitizen = CreateTestCitizen(JobType.Salesman, 11);

            // Act
            var result = hrManager.HireCitizen(extraCitizen, department, SALARY_30000);

            // Assert
            Assert.IsNull(result);
            Assert.AreEqual(EMPLOYEE_COUNT_FOR_LIMIT_TEST, hrManager.Employees.Count);
        }
    }
}