using Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Map;

namespace Core.Models.Mobs
{
    public class Citizen : Mob
    {
        public Citizen(int x, int y, GameMap map) : base(x, y, map) { }

        public int Age { get; set; }
        public bool IsMale { get; set; }

        public EducationLevel Education { get; set; }
        public bool IsStudying { get; set; }
        public Building School { get; set; }

        public JobType Profession { get; set; }
        public bool IsEmployed { get; set; }
        public Building Workplace { get; set; }
        public decimal Salary { get; set; }

        public bool IsMarried { get; set; }
        public Citizen Partner { get; set; }
        public List<Citizen> Children { get; set; } = new();
        public bool CanReproduce => Age >= 18 && Age <= 45 && IsMarried;
        public ResidentialBuilding Home { get; set; }

        public float Health { get; set; } = 100f;

        public float Happiness { get; set; } = 50f;

        public float AcademicProgress { get; set; } = 0f; // прогресс обучения
        public float AcademicPerformance { get; set; } // успеваемость обучения

        public Building DestinationBuilding { get; set; } // Конечная цель перемещения (Workplace, School, Home)
        public TransitStation TargetTransitStation { get; set; }// Остановка, к которой гражданин направляется или на которой ждет.
        public Transport CurrentTransport { get; set; }// Текущий транспорт, в котором находится гражданин.
        public bool IsWaitingForTransport => TargetTransitStation != null && CurrentTransport == null && IsAtStationTile(); // Флаг: Гражданин ждет транспорт на остановке?
        public bool IsOnTransport => CurrentTransport != null;// Флаг: Гражданин едет в транспорте?
        public bool IsMoving => DestinationBuilding != null || TargetTransitStation != null || IsOnTransport; // Флаг: Гражданин находится в процессе перемещения?

        /// <summary>
        /// Обновляет счастье жителя в зависимости от окружающей инфраструктуры
        /// </summary>
        public void UpdateHappinessBasedOnInfrastructure()
        {
            // Проверяем координаты и получаем тайл напрямую из Tiles[,]
            if (GameMap?.Tiles == null) return;
            if (X < 0 || X >= GameMap.Width || Y < 0 || Y >= GameMap.Height) return;

            var currentTile = GameMap.Tiles[X, Y];

            if (currentTile == null) return;

            if (currentTile.HasPark)
                Happiness = Math.Min(100, Happiness + 0.1f);

            if (currentTile.HasBikeLane)
                Happiness = Math.Min(100, Happiness + 0.05f);

            if (currentTile.HasPedestrianPath)
                Happiness = Math.Min(100, Happiness + 0.03f);

            if (Home?.IsOperational == true)
                Happiness = Math.Min(100, Happiness + 0.02f);

            if (Workplace?.IsOperational == true)
                Happiness = Math.Min(100, Happiness + 0.01f);

            Happiness = Math.Max(0, Happiness - 0.01f);
        }

        /// <summary>
        /// Влияние счастья на здоровье и продуктивность
        /// </summary>
        public void ApplyHappinessEffects()
        {
            // Счастье влияет на здоровье
            if (Happiness > 70f)
                Health = Math.Min(100, Health + 0.05f);
            else if (Happiness < 30f)
                Health = Math.Max(0, Health - 0.1f);

            // Счастье влияет на шанс размножения
            if (Happiness > 80f && CanReproduce)
            {
                var random = new Random();
                if (random.NextDouble() < 0.15) // Увеличенный шанс при высоком счастье
                    TryReproduce();
            }
        }

        public override void Move()
        {
            // TODO: остальные методы перемещения

            if (IsOnTransport)
            {
                // СОСТОЯНИЕ 1: Едет в транспорте.
                // Координаты гражданина синхронизируются самим транспортом.
                if (IsArrivedAtDestination())
                {
                    TryDisembarkFromTransport();
                }
                return;
            }

            if (IsWaitingForTransport)
            {
                // СОСТОЯНИЕ 2: Ждет транспорт на остановке.
                // Тут мы только ждем. Логика посадки будет инициирована транспортом.
                return;
            }

            if (DestinationBuilding == null)
            {
                // СОСТОЯНИЕ 3: Нет цели. Определяем, куда нужно идти.
                DecideDestination();
            }

            if (DestinationBuilding != null)
            {
                // Есть цель, начинаем движение.

                if (IsArrivedAtDestination())
                {
                    // Прибыли в конечный пункт.
                    DestinationBuilding = null;
                    TargetTransitStation = null;
                    return;
                }

                // Логика выбора: Транспорт или пешком?
                bool isFar = CalculateDistanceToDestination() > 10; // Если далеко (условное расстояние)
                bool needsTransit = TargetTransitStation != null || (isFar && FindNearestTransitStation() != null);

                if (needsTransit)
                {
                    // Шаг А: Определить остановку
                    if (TargetTransitStation == null)
                    {
                        TargetTransitStation = FindNearestTransitStation();
                    }

                    // Шаг Б: Движемся к остановке
                    if (TargetTransitStation != null && !IsAtStationTile())
                    {
                        MoveTowards(TargetTransitStation.X, TargetTransitStation.Y);

                        // Если только что пришли на остановку, регистрируемся в ней.
                        if (IsAtStationTile())
                        {
                            TargetTransitStation.AddWaitingCitizen(this);
                        }
                    }
                }
                else
                {
                    // Идем пешком
                    TargetTransitStation = null;
                    MoveTowards(DestinationBuilding.X, DestinationBuilding.Y);
                }
            }
        }

        /// <summary>
        /// ПОРНО!!!! НЕ СМОТРЕТЬ 18+ 
        /// </summary>
        /// <returns></returns>
        public bool TryReproduce()
        {
            if (!CanReproduce || Partner == null) return false;

            var random = new Random();
            if (random.NextDouble() < 0.1)
                return true;

            return false;
        }

        public void Work()
        {
            if (!IsEmployed || Workplace == null) return;

            // ворк ворк ворк ворк ворк ворк
        }

        /// <summary>
        /// Выполняет процесс обучения. Вызывается каждый игровой такт.
        /// </summary>
        public void Study()
        {
            if (!IsStudying || School == null) return;

            if (School is not ServiceBuilding serviceSchool)
            {
                // Если здание не является учебным сервисным зданием — прекращаем учиться.
                IsStudying = false;
                School = null;
                return;
            }

            // Если здание не работает, обучение замедляется/останавливается и прогресс может немного падать
            if (!serviceSchool.IsOperational)
            {
                Happiness = Math.Max(0, Happiness - 0.05f);
                // Небольшая деградация прогресса при отсутствии сервисов
                AcademicProgress = Math.Max(0, AcademicProgress - 0.01f);
                return;
            }

            // Накопление прогресса: базовая скорость масштабируется успеваемостью ученика (0..100 -> 0..1)
            float baseIncrement = 1.0f; // прогресс за такт при Performance = 100
            float perfMultiplier = Math.Clamp(AcademicPerformance / 100f, 0f, 2f); // допускаем бонусы до x2
            AcademicProgress = Math.Min(100f, AcademicProgress + baseIncrement * perfMultiplier);

            // Положительные побочные эффекты от регулярного обучения
            Health = Math.Min(100, Health + 0.01f);
            Happiness = Math.Min(100, Happiness + 0.01f);

            // Попытка выпуска по накопленному прогрессу
            if (AcademicProgress >= 100f)
                TryGraduate();
        }

        // Доп метод для обучения
        /// <summary>
        /// Попытка завершить текущий уровень образования и перейти на следующий.
        /// </summary>
        public void TryGraduate()
        {
            if (!IsStudying || School == null) return;

            if (School is not ServiceBuilding serviceSchool)
            {
                return;
            }

            if (!serviceSchool.IsOperational) return;

            // Требуем явный порог прогресса
            if (AcademicProgress < 100f) return;

            bool promoted = false;

            switch (Education)
            {
                case EducationLevel.School:
                    Education = EducationLevel.College;
                    promoted = true;
                    break;

                case EducationLevel.College:
                    Education = EducationLevel.University;
                    promoted = true;
                    break;

                case EducationLevel.University:
                    // Уже на максимуме — можно просто завершить обучение
                    promoted = true;
                    break;
            }

            if (promoted)
            {
                IsStudying = false;
                // Удаляем гражданина из очереди/клиентов школы, если он там
                if (serviceSchool.Clients.Contains(this))
                    serviceSchool.Clients.Remove(this);
                School = null;
                AcademicProgress = 0f;

                // Счастье повышается после окончания учебы
                Happiness = Math.Min(100, Happiness + 5f);
            }
        }

        /// <summary>
        /// Простейшее движение к цели (пока без поиска пути, просто X, Y).
        /// </summary>
        private void MoveTowards(int targetX, int targetY)
        {
            // Здесь должна быть логика поиска пути (Pathfinding), 
            // но пока используем упрощенный алгоритм Mob.MoveTo.
            int deltaX = targetX - X;
            int deltaY = targetY - Y;

            if (Math.Abs(deltaX) > Math.Abs(deltaY))
            {
                // Сначала двигаемся по X
                MoveTo(X + Math.Sign(deltaX), Y);
            }
            else
            {
                // Сначала двигаемся по Y
                MoveTo(X, Y + Math.Sign(deltaY));
            }
        }

        // ======ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ======

        /// <summary>
        /// Проверяет, находится ли гражданин на плитке целевой остановки.
        /// </summary>
        private bool IsAtStationTile()
        {
            if (TargetTransitStation == null) return false;
            return X == TargetTransitStation.X && Y == TargetTransitStation.Y;
        }

        /// <summary>
        /// Проверяет, прибыл ли гражданин в конечный пункт (здание).
        /// </summary>
        public bool IsArrivedAtDestination()
        {
            if (DestinationBuilding == null) return false;

            int targetX = DestinationBuilding.X;
            int targetY = DestinationBuilding.Y;

            // Если в транспорте, проверяем его координаты
            if (IsOnTransport)
            {
                targetX = CurrentTransport.X;
                targetY = CurrentTransport.Y;
            }

            // Считаем, что прибыл, если находится на тайле здания (или рядом, для больших зданий)
            return X >= targetX && X < targetX + DestinationBuilding.Width &&
                   Y >= targetY && Y < targetY + DestinationBuilding.Height;
        }

        /// <summary>
        /// Упрощенная логика выбора цели (приоритет: Учеба -> Работа -> Дом).
        /// </summary>
        private void DecideDestination()
        {
            if (IsStudying && School != null)
            {
                DestinationBuilding = School;
            }
            else if (IsEmployed && Workplace != null)
            {
                DestinationBuilding = Workplace;
            }
            else if (Home != null)
            {
                // Если других дел нет, идем домой
                DestinationBuilding = Home;
            }
        }

        /// <summary>
        /// Попытка высадки из транспорта при достижении цели.
        /// </summary>
        private void TryDisembarkFromTransport()
        {
            if (CurrentTransport == null || DestinationBuilding == null) return;

            CurrentTransport.TryDisembark(this);

            // Позиция гражданина становится позицией транспорта
            X = CurrentTransport.X;
            Y = CurrentTransport.Y;

            CurrentTransport = null; // Мы вышли
            TargetTransitStation = null; // Сбрасываем остановку
            // В следующем такте IsArrivedAtDestination() должен завершить перемещение.
        }

        /// <summary>
        /// Упрощенный расчет расстояния до цели (для принятия решения о транспорте).
        /// </summary>
        private double CalculateDistanceToDestination()
        {
            if (DestinationBuilding == null) return 0;
            return Math.Sqrt(Math.Pow(X - DestinationBuilding.X, 2) + Math.Pow(Y - DestinationBuilding.Y, 2));
        }

        /// <summary>
        /// Ищет ближайшую рабочую TransitStation на карте.
        /// ТРЕБУЕТ, чтобы в GameMap был доступен список Buildings (GameMap.Buildings).
        /// </summary>
        private TransitStation FindNearestTransitStation()
        {
            if (GameMap == null || GameMap.Buildings == null) return null;

            TransitStation nearestStation = null;
            double minDistance = 50 * 50; // Ищем только в радиусе 50 тайлов

            foreach (var building in GameMap.Buildings)
            {
                // Используем is с приведением типа, чтобы найти только остановки, которые рабочие
                if (building is TransitStation station && station.IsOperational)
                {
                    double distanceSquared = Math.Pow(X - station.X, 2) + Math.Pow(Y - station.Y, 2);

                    if (distanceSquared < minDistance)
                    {
                        minDistance = distanceSquared;
                        nearestStation = station;
                    }
                }
            }

            return nearestStation;
        }
    }
}
