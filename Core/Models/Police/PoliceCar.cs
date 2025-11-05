using Core.Models.Base;
using Core.Models.Map;
using Core.Services;

namespace Core.Models.Police
{
    /// <summary>
    /// Представляет полицейскую машину для патрулирования и реагирования на преступления.
    /// Перевозит полицейских к месту происшествия.
    /// </summary>
    public class PoliceCar : Transport
    {
        private const int PoliceCarCapacity = 4; // Обычно 2-4 офицера
        public List<PoliceOfficer> Officers { get; set; }
        public bool IsOnPatrol { get; set; }
        public bool IsRespondingToCall { get; set; }
        public List<PathNode> CurrentRoute { get; set; }
        public Crime TargetCrime { get; set; }
        public PoliceStation HomeStation { get; set; }

        /// <summary>
        /// Конструктор полицейской машины
        /// </summary>
        public PoliceCar(int x, int y, GameMap map, PoliceStation station) 
            : base(x, y, map, PoliceCarCapacity)
        {
            Speed = 11f; // Полицейские машины быстрее обычных
            Officers = new List<PoliceOfficer>();
            IsOnPatrol = false;
            IsRespondingToCall = false;
            CurrentRoute = new List<PathNode>();
            HomeStation = station;
        }

        /// <summary>
        /// Движение полицейской машины
        /// </summary>
        public override void Move()
        {
            // Если есть маршрут, следуем по нему
            if (CurrentRoute != null && CurrentRoute.Count > 0)
            {
                var nextNode = CurrentRoute[0];

                // Перемещаемся к следующей точке маршрута
                X = nextNode.X;
                Y = nextNode.Y;

                // Убираем пройденную точку из маршрута
                CurrentRoute.RemoveAt(0);

                // Синхронизируем положение офицеров
                foreach (var officer in Officers)
                {
                    officer.X = X;
                    officer.Y = Y;
                }

                // Если маршрут закончился
                if (CurrentRoute.Count == 0)
                {
                    HandleArrival();
                }
            }
            else if (IsOnPatrol)
            {
                // Патрулирование: случайное движение по дорогам
                Patrol();
            }
        }

        /// <summary>
        /// Обработка прибытия в точку назначения
        /// </summary>
        private void HandleArrival()
        {
            if (IsRespondingToCall && TargetCrime != null)
            {
                // Прибыли на место преступления
                RespondToCrime();
            }

            IsRespondingToCall = false;
            IsOnPatrol = true;
        }

        /// <summary>
        /// Реагирование на преступление
        /// </summary>
        private void RespondToCrime()
        {
            if (TargetCrime == null) return;

            // Назначаем офицерам расследование преступления
            foreach (var officer in Officers)
            {
                if (officer.IsAvailable())
                {
                    officer.AssignCrime(TargetCrime);
                    break; // Один офицер на одно преступление
                }
            }

            TargetCrime = null;
        }

        /// <summary>
        /// Патрулирование улиц
        /// </summary>
        private void Patrol()
        {
            // Случайное перемещение для патрулирования
            Random random = new Random();
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            int direction = random.Next(4);
            int newX = X + dx[direction];
            int newY = Y + dy[direction];

            // Проверяем границы карты
            if (newX >= 0 && newX < GameMap.Width && 
                newY >= 0 && newY < GameMap.Height)
            {
                X = newX;
                Y = newY;

                // Синхронизируем положение офицеров
                foreach (var officer in Officers)
                {
                    officer.X = X;
                    officer.Y = Y;
                }
            }
        }

        /// <summary>
        /// Отправляет патруль на место преступления
        /// </summary>
        public bool DispatchToCrime(Crime crime)
        {
            if (IsRespondingToCall || crime == null)
                return false;

            TargetCrime = crime;
            IsRespondingToCall = true;
            IsOnPatrol = false;

            // Создаём маршрут к месту преступления
            var pathfinder = new PathfindingService(GameMap);
            CurrentRoute = pathfinder.FindPath(X, Y, crime.LocationX, crime.LocationY);

            return CurrentRoute != null;
        }

        /// <summary>
        /// Добавляет офицера в патрульную машину
        /// </summary>
        public bool AddOfficer(PoliceOfficer officer)
        {
            if (Officers.Count < PoliceCarCapacity)
            {
                Officers.Add(officer);
                officer.X = X;
                officer.Y = Y;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Начинает патрулирование
        /// </summary>
        public void StartPatrol()
        {
            IsOnPatrol = true;
            IsRespondingToCall = false;
        }

        /// <summary>
        /// Проверяет, доступен ли патруль для вызова
        /// </summary>
        public bool IsAvailable()
        {
            return !IsRespondingToCall && Officers.Count > 0;
        }
    }
}
