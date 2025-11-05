using Core.Models.Map;
using Core.Models.Roads;

namespace Core.Services
{
    /// <summary>
    /// Сервис для поиска пути на карте.
    /// Использует алгоритм A* для построения оптимального маршрута.
    /// </summary>
    public class PathfindingService
    {
        private GameMap _map;

        /// <summary>
        /// Конструктор сервиса поиска пути
        /// </summary>
        public PathfindingService(GameMap map)
        {
            _map = map;
        }

        /// <summary>
        /// Находит путь от начальной точки до конечной
        /// </summary>
        /// <param name="startX">X координата начальной точки</param>
        /// <param name="startY">Y координата начальной точки</param>
        /// <param name="endX">X координата конечной точки</param>
        /// <param name="endY">Y координата конечной точки</param>
        /// <returns>Список точек пути или null если путь не найден</returns>
        public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
        {
            // Проверка границ карты
            if (!IsValidPosition(startX, startY) || !IsValidPosition(endX, endY))
                return null;

            // Список открытых узлов (которые нужно проверить)
            var openList = new List<PathNode>();
            // Список закрытых узлов (которые уже проверены)
            var closedList = new HashSet<string>();

            // Создаём начальный узел
            var startNode = new PathNode
            {
                X = startX,
                Y = startY,
                G = 0,
                H = CalculateDistance(startX, startY, endX, endY),
                Parent = null
            };

            openList.Add(startNode);

            // Основной цикл алгоритма A*
            while (openList.Count > 0)
            {
                // Находим узел с наименьшей стоимостью F
                var currentNode = GetNodeWithLowestF(openList);

                // Если достигли цели
                if (currentNode.X == endX && currentNode.Y == endY)
                {
                    return ReconstructPath(currentNode);
                }

                // Перемещаем текущий узел из открытого списка в закрытый
                openList.Remove(currentNode);
                closedList.Add(GetNodeKey(currentNode.X, currentNode.Y));

                // Проверяем всех соседей
                foreach (var neighbor in GetNeighbors(currentNode))
                {
                    string neighborKey = GetNodeKey(neighbor.X, neighbor.Y);

                    // Пропускаем если узел уже в закрытом списке
                    if (closedList.Contains(neighborKey))
                        continue;

                    // Вычисляем стоимость пути до соседа
                    float tentativeG = currentNode.G + CalculateMoveCost(currentNode.X, currentNode.Y, neighbor.X, neighbor.Y);

                    // Ищем соседа в открытом списке
                    var existingNode = openList.FirstOrDefault(n => n.X == neighbor.X && n.Y == neighbor.Y);

                    if (existingNode == null)
                    {
                        // Добавляем нового соседа в открытый список
                        neighbor.G = tentativeG;
                        neighbor.H = CalculateDistance(neighbor.X, neighbor.Y, endX, endY);
                        neighbor.Parent = currentNode;
                        openList.Add(neighbor);
                    }
                    else if (tentativeG < existingNode.G)
                    {
                        // Нашли более короткий путь до существующего узла
                        existingNode.G = tentativeG;
                        existingNode.Parent = currentNode;
                    }
                }
            }

            // Путь не найден
            return null;
        }

        /// <summary>
        /// Проверяет, находится ли позиция в пределах карты
        /// </summary>
        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _map.Width && y >= 0 && y < _map.Height;
        }

        /// <summary>
        /// Вычисляет эвристическое расстояние между двумя точками (манхэттенское расстояние)
        /// </summary>
        private float CalculateDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        /// <summary>
        /// Вычисляет стоимость перемещения между соседними клетками
        /// </summary>
        private float CalculateMoveCost(int fromX, int fromY, int toX, int toY)
        {
            var tile = _map.Tiles[toX, toY];

            // Базовая стоимость
            float cost = 1.0f;

            // Дороги имеют меньшую стоимость передвижения
            if (tile.HasRoad)
            {
                cost = 0.5f;
            }

            // Здания и непроходимые тайлы имеют очень высокую стоимость
            if (tile.Building != null)
            {
                cost = 100f; // Очень высокая стоимость, но не бесконечная
            }

            return cost;
        }

        /// <summary>
        /// Получает соседние клетки (4 направления: верх, низ, лево, право)
        /// </summary>
        private List<PathNode> GetNeighbors(PathNode node)
        {
            var neighbors = new List<PathNode>();

            // Направления: вверх, вниз, влево, вправо
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int newX = node.X + dx[i];
                int newY = node.Y + dy[i];

                if (IsValidPosition(newX, newY))
                {
                    neighbors.Add(new PathNode { X = newX, Y = newY });
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Находит узел с наименьшей стоимостью F в списке
        /// </summary>
        private PathNode GetNodeWithLowestF(List<PathNode> nodes)
        {
            PathNode lowest = nodes[0];
            foreach (var node in nodes)
            {
                if (node.F < lowest.F)
                {
                    lowest = node;
                }
            }
            return lowest;
        }

        /// <summary>
        /// Восстанавливает путь от конечного узла к начальному
        /// </summary>
        private List<PathNode> ReconstructPath(PathNode endNode)
        {
            var path = new List<PathNode>();
            var current = endNode;

            while (current != null)
            {
                path.Add(current);
                current = current.Parent;
            }

            path.Reverse(); // Переворачиваем путь от начала к концу
            return path;
        }

        /// <summary>
        /// Создаёт уникальный ключ для узла
        /// </summary>
        private string GetNodeKey(int x, int y)
        {
            return $"{x},{y}";
        }
    }

    /// <summary>
    /// Представляет узел в алгоритме поиска пути
    /// </summary>
    public class PathNode
    {
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Стоимость пути от начала до этого узла
        /// </summary>
        public float G { get; set; }

        /// <summary>
        /// Эвристическая оценка расстояния от этого узла до цели
        /// </summary>
        public float H { get; set; }

        /// <summary>
        /// Общая стоимость F = G + H
        /// </summary>
        public float F => G + H;

        /// <summary>
        /// Родительский узел для восстановления пути
        /// </summary>
        public PathNode Parent { get; set; }
    }
}
