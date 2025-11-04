using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Core.Finance;
using Core.Models.Finance;

namespace Tests.UnitTests
{
    [TestClass]
    public sealed class FinanceModelsTests
    {
        [TestMethod]
        public void Transaction_IsRevenue_Works()
        {
            // Подготовка: создаем две транзакции с разными суммами
            var t1 = new Transaction { Amount = 100m };
            var t2 = new Transaction { Amount = -50m };

            // Проверка признака дохода/расхода
            Assert.IsTrue(t1.IsRevenue);
            Assert.IsFalse(t2.IsRevenue);
        }

        [TestMethod]
        public void Loan_Has_GeneratedId_And_Properties_Settable()
        {
            // Подготовка: заполняем основные поля кредита
            var loan = new Loan
            {
                BorrowerId = "CITY",
                Principal = 1000000m,
                AnnualInterestRate = 5.0,
                StartDate = new DateTime(2025,1,1),
                MaturityDate = new DateTime(2030,1,1),
                OutstandingPrincipal = 900000m
            };

            // Проверка: идентификатор создан и поля заданы
            Assert.IsFalse(string.IsNullOrWhiteSpace(loan.LoanId));
            Assert.AreEqual("CITY", loan.BorrowerId);
            Assert.AreEqual(1000000m, loan.Principal);
            Assert.AreEqual(5.0, loan.AnnualInterestRate, 1e-9);
        }

        [TestMethod]
        public void Budget_Items_List_NotNull_And_Totals_Assignable()
        {
            // Подготовка: создаем бюджет с одной доходной и одной расходной статьей
            var budget = new Budget { Year = 2025 };
            budget.Items.Add(new BudgetItem { Id = "R1", Category = FinanceCategory.Revenue, Amount = 100m, IsRevenue = true });
            budget.Items.Add(new BudgetItem { Id = "E1", Category = FinanceCategory.Transport, Amount = 60m, IsRevenue = false });

            // Вычисление итогов (агрегирование значений)
            budget.TotalRevenue = budget.Items.Where(i => i.IsRevenue).Sum(i => i.Amount);
            budget.TotalExpenses = budget.Items.Where(i => !i.IsRevenue).Sum(i => i.Amount);
            budget.SurplusOrDeficit = budget.TotalRevenue - budget.TotalExpenses;

            // Проверка итогов и количества статей
            Assert.AreEqual(2, budget.Items.Count);
            Assert.AreEqual(100m, budget.TotalRevenue);
            Assert.AreEqual(60m, budget.TotalExpenses);
            Assert.AreEqual(40m, budget.SurplusOrDeficit);
        }

        [TestMethod]
        public void DebtLedger_Can_Aggregate_Total()
        {
            // Подготовка: добавляем два инструмента долга
            var ledger = new DebtLedger();
            ledger.Instruments.Add(new DebtInstrument { OutstandingPrincipal = 100m });
            ledger.Instruments.Add(new DebtInstrument { OutstandingPrincipal = 50m });

            // Вычисление совокупного остатка долга
            ledger.TotalOutstandingDebt = ledger.Instruments.Sum(i => i.OutstandingPrincipal);

            // Проверка количества и суммы
            Assert.AreEqual(2, ledger.Instruments.Count);
            Assert.AreEqual(150m, ledger.TotalOutstandingDebt);
        }

        [TestMethod]
        public void TaxPolicy_Rates_Are_Settable()
        {
            // Подготовка: задаем ставки налогов и взносов
            var policy = new TaxPolicy
            {
                CorporateTaxRate = 0.2m,
                PersonalIncomeTaxRate = 0.13m,
                ValueAddedTaxRate = 0.2m,
                PropertyTaxRate = 0.01m,
                SocialContributionsRate = 0.3m
            };

            // Проверка: значения сохранены
            Assert.AreEqual(0.2m, policy.CorporateTaxRate);
            Assert.AreEqual(0.13m, policy.PersonalIncomeTaxRate);
        }
    }
}
