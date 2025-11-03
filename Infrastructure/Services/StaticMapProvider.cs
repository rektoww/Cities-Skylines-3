using System;
using System.Linq;
using Core.Enums;
using Core.Models.Map;

namespace Infrastructure.Services
{
    /// <summary>
    /// Провайдер статичной (фиксированной) карты.
    /// Строит экземпляр <see cref="GameMap"/> на основе двух строковых масок:
    /// маски рельефа (terrain) и маски ресурсов (resources).
    /// Никакой случайной генерации здесь нет — карта всегда одна и та же.
    /// </summary>
    public static class StaticMapProvider
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Создаёт и заполняет <see cref="GameMap"/> на основании двух масок.
        /// </summary>
        /// <remarks>
        /// <para><b>Легенда рельефа:</b> 
        /// <c>~</c> — вода, 
        /// <c>^</c> — гора, 
        /// <c>.</c> — равнина, 
        /// <c>#</c> — скальное плато (тоже гора),
        /// <c>T</c> — лес,
        /// <c>M</c> — луг</para>
        /// <para><b>Легенда ресурсов:</b> <c>I</c> — железо, <c>O</c> — нефть, <c>G</c> — газ.</para>
        /// Если строка короче вычисленной ширины карты, недостающие символы трактуются как равнина без ресурсов.
        /// </remarks>
        /// <returns>Готовая карта с тайлами, у которых заполнены <see cref="Tile.Terrain"/> и <see cref="Tile.Resources"/>.</returns>
        public static GameMap Build()
        {
            // ========= 1) Сырые маски (их удобно редактировать прямо здесь) =========

            // Маска рельефа.
            string[] terrainRows =
            {
                "~~~~~~~~~~~~~~~~~~~~~~~",
                "~~~~.....^^^^TTTTT~~~~~",
                "~~~......^^^^TTTTT.~~~~",
                "~~.......^^^TTT.T..~~~~",
                "~...M....^^.....M..~~~~",
                "~..MM..........MMM...~~",
                "~....######....MM.....~",
                "~....######.....TT....~",
                "~..............TTT...~~",
                "~~..MM....^^.....T..~~~",
                "~~~..MM..^^^^......~~~~",
                "~~~~.....^^^^.....~~~~~",
                "~~~~~~~~~~~~~~~~~~~~~~~"
            };

            // Маска ресурсов (накладывается поверх рельефа).
            string[] resourceRows =
            {
                "......................",
                "....I.............G...",
                "......................",
                ".........O............",
                "......................",
                ".....I....G...........",
                ".....I................",
                ".............O........",
                "....................G.",
                "......................",
                "...........I..........",
                "......................",
                "......................"
            };

            // ========= 2) Проверки размеров и расчёт итоговой геометрии =========

            // Высота карт должна совпадать: по строке ресурсов на каждую строку рельефа.
            int heightT = terrainRows.Length;
            int heightR = resourceRows.Length;
            if (heightT != heightR)
                throw new InvalidOperationException(
                    $"Высота карт не совпадает: terrain={heightT}, resources={heightR}. Проверь количество строк.");

            int height = heightT;

            // Ширина берётся как максимум по всем строкам обеих масок.
            // Это защищает от случайных «коротких» строк: недостающие символы позже будем трактовать как '.'.
            int width = Math.Max(
                terrainRows.Max(r => r.Length),
                resourceRows.Max(r => r.Length));

            // ========= 3) Создание и заполнение GameMap =========

            // Инициализируем карту нужного размера; внутри будут созданы пустые Tile[,].
            var map = new GameMap(width, height);

            // Локальная функция: безопасно получить символ из строки.
            // Если x выходит за пределы длины строки — возвращаем fallback (по умолчанию '.').
            static char GetChar(string row, int x, char fallback = '.') =>
                (x >= 0 && x < row.Length) ? row[x] : fallback;

            // Проходим по каждой строке (y) и колонке (x) и задаём рельеф/ресурсы клетки.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // ВАЖНО: у карты двумерный массив тайлов -> обращаемся как [x, y], а не [x][y].
                    var tile = map.Tiles[x, y];

                    // ----- Рельеф -----
                    // Берём символ рельефа; если строка короче — считаем равниной ('.').
                    char t = GetChar(terrainRows[y], x, '.');

                    tile.Terrain = t switch
                    {
                        '~' => TerrainType.Water,
                        '^' => TerrainType.Mountain,
                        '#' => TerrainType.Mountain, // трактуем плато как гору
                        'T' => TerrainType.Forest,   // лес
                        'M' => TerrainType.Meadow,   // луг
                        '.' => TerrainType.Plain,
                        _ => TerrainType.Plain     // всё неизвестное считаем равниной
                    };

                    // ----- Деревья -----
                    // Автоматически добавляем деревья в зависимости от типа местности
                    if (tile.Terrain == TerrainType.Forest)
                    {
                        tile.TreeType = GetRandomTreeType();
                        tile.TreeCount = _random.Next(5, 11); // 5-10 деревьев в лесу
                    }
                    else if (tile.Terrain == TerrainType.Meadow)
                    {
                        // На лугах могут быть редкие деревья (30% chance)
                        if (_random.Next(0, 100) < 30)
                        {
                            tile.TreeType = GetRandomTreeType();
                            tile.TreeCount = _random.Next(1, 4); // 1-3 дерева на лугу
                        }
                    }

                    // ----- Ресурсы -----
                    // Берём символ ресурсов; если строки нет/короче — ничего не добавляем.
                    char r = GetChar(resourceRows[y], x, '.');

                    if (r is 'I' or 'O' or 'G')
                    {
                        tile.Resources.Add(new NaturalResource
                        {
                            Type = r switch
                            {
                                'I' => ResourceType.Iron,
                                'O' => ResourceType.Oil,
                                'G' => ResourceType.Gas,
                                _ => ResourceType.Iron // дефолт не достижим, но оставим на всякий случай
                            },
                            Amount = 200 // фиксированное значение; при необходимости можно вынести в константу/настройку
                        });
                    }
                }
            }

            // Возвращаем полностью готовую карту.
            return map;
        }

        /// <summary>
        /// Возвращает случайный тип дерева
        /// </summary>
        private static TreeType GetRandomTreeType()
        {
            var treeTypes = (TreeType[])Enum.GetValues(typeof(TreeType));
            return treeTypes[_random.Next(treeTypes.Length)];
        }
    }
}