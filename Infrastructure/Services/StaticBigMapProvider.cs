using Core.Enums;
using Core.Models.Map;

namespace Infrastructure.Services
{
    /// <summary>
    /// Статичный конструктор большой карты 50×50 без случайности.
    /// На карте:
    /// - Вода: «океан» по периметру + залив на севере и река внутрь (связная акватория).
    /// - Рельеф: горные гребни, пятна леса, чередование луга (Meadow) и равнины (Plain).
    /// - Ресурсы: Iron — в горах, Oil/Gas — на равнинах/лугах кластерами.
    /// Это по сути «маски», только описанные формулами, чтобы не хранить 2500 символов.
    /// </summary>
    public static class StaticBigMapProvider
    {
        /// <summary>
        /// Построить предопределённую карту 50×50.
        /// </summary>
        public static GameMap Build50()
        {
            const int W = 50; // ширина карты
            const int H = 50; // высота карты
            var map = new GameMap(W, H);

            // ============ ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ============

            // Квадрат числа (удобно для формул кругов/эллипсов)
            static int Sq(int v) => v * v;

            // Попадание точки (x,y) внутрь эллипса с центром (cx,cy) и радиусами rx, ry.
            // Классическая формула: (x-cx)^2 / rx^2 + (y-cy)^2 / ry^2 <= 1
            static bool InsideEllipse(int x, int y, int cx, int cy, int rx, int ry)
                => Sq(x - cx) * ry * ry + Sq(y - cy) * rx * rx <= Sq(rx) * Sq(ry);

            // ============ 1) БАЗА: всё — Meadow (луг) ============
            // Ставим луг в каждую клетку. Дальше поверх него будем рисовать воду/горы/лес.
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    map.Tiles[x, y].Terrain = TerrainType.Meadow;

            // ============ 2) ВОДА: ОКЕАН + ЗАЛИВ + РЕКА ============
            // 2.1) Океан по периметру: рамка шириной 2 клетки.
            // Это гарантирует связанность воды и даёт «береговую линию» по краям.
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    // Условие: попали в рамку 2 клетки с любой стороны
                    if (x < 2 || y < 2 || x > W - 3 || y > H - 3)
                        map.Tiles[x, y].Terrain = TerrainType.Water;
                }
            }

            // 2.2) Залив на севере: эллипс (центр и радиусы можно легко менять).
            int bayCX = 26, bayCY = 8;   // центр залива
            int bayRX = 10, bayRY = 6;   // радиусы по X/Y
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (InsideEllipse(x, y, bayCX, bayCY, bayRX, bayRY))
                        map.Tiles[x, y].Terrain = TerrainType.Water;

            // 2.3) Река: «стекает» от залива вниз до y=19, ширина 2–3 клетки.
            // Небольшая извилистость: центральный столбец реки слегка смещается вправо/влево
            // в зависимости от y (простая периодическая формула).
            for (int y = 2; y < 20; y++)
            {
                // Центральная ось реки: колеблется вокруг bayCX.
                int cx = bayCX + (y % 4 == 0 ? 1 : 0) - (y % 7 == 0 ? 1 : 0);

                // Река шириной 3 клетки: cx-1, cx, cx+1
                for (int dx = -1; dx <= 1; dx++)
                {
                    int rx = cx + dx;
                    // Не выходим за рамку «океана» (2..W-3)
                    if (rx >= 2 && rx < W - 2)
                        map.Tiles[rx, y].Terrain = TerrainType.Water;
                }
            }

            // ============ 3) РЕЛЬЕФ: ГОРЫ, ЛЕСА, РАВНИНЫ ============

            // 3.1) Гребень гор: «вертикальный столб» с утолщениями.
            // Принцип: идём по y от 10 до 39, ставим горы в x≈24..25,
            // а каждые 5–6 клеток добавляем ещё одну колонку (утолщаем гребень).
            for (int y = 10; y < 40; y++)
            {
                int x = 24 + ((y % 6 == 0) ? 1 : 0); // лёгкий «зиг-заг» по X
                if (map.Tiles[x, y].Terrain != TerrainType.Water)
                    map.Tiles[x, y].Terrain = TerrainType.Mountain;

                if (map.Tiles[x + 1, y].Terrain != TerrainType.Water)
                    map.Tiles[x + 1, y].Terrain = TerrainType.Mountain;

                if (y % 5 == 0 && map.Tiles[x + 2, y].Terrain != TerrainType.Water)
                    map.Tiles[x + 2, y].Terrain = TerrainType.Mountain;
            }

            // 3.2) «Перекат» гор: диагональ из 20 клеток в южной части карты.
            // Добавляет разнообразия в ландшафт; вода снова не трогаем.
            for (int i = 0; i < 20; i++)
            {
                int x = 10 + i;         // диагонально вправо
                int y = 30 + (i / 3);   // лёгкий подъём по Y
                if (x >= 2 && x < W - 2 && y >= 2 && y < H - 2 &&
                    map.Tiles[x, y].Terrain != TerrainType.Water)
                {
                    map.Tiles[x, y].Terrain = TerrainType.Mountain;
                }
            }

            // 3.3) Леса — три эллипса.
            // Вода и горы при этом не затрагиваются (лес только на равнинах/лугах).
            /// <summary>
            /// Рисует пятно леса в виде эллипса.
            /// Лес ставится ТОЛЬКО там, где нет воды и гор: на Meadow/Plain.
            /// </summary>
            /// <param name="cx">X-координата центра эллипса.</param>
            /// <param name="cy">Y-координата центра эллипса.</param>
            /// <param name="rx">«Радиус» эллипса по оси X (полуось).</param>
            /// <param name="ry">«Радиус» эллипса по оси Y (полуось).</param>
            void ForestEllipse(int cx, int cy, int rx, int ry)
            {
                // Идём по минимальному охватывающему прямоугольнику эллипса
                for (int y = cy - ry; y <= cy + ry; y++)
                {
                    // Защита: не залезаем в «океанскую рамку» (толщина 2 клетки) и за границы
                    if (y < 2 || y >= H - 2) continue;

                    for (int x = cx - rx; x <= cx + rx; x++)
                    {
                        if (x < 2 || x >= W - 2) continue;

                        // Точка принадлежит эллипсу?
                        if (InsideEllipse(x, y, cx, cy, rx, ry) &&
                            map.Tiles[x, y].Terrain != TerrainType.Water &&
                            map.Tiles[x, y].Terrain != TerrainType.Mountain)
                        {
                            // Лес размещаем только на суше без гор
                            map.Tiles[x, y].Terrain = TerrainType.Forest;
                        }
                    }
                }
            }

            // Три пятна леса в разных регионах
            ForestEllipse(35, 14, 6, 4); // маленький северо-восточный лес
            ForestEllipse(38, 32, 5, 6); // восточный лес
            ForestEllipse(18, 36, 7, 4); // юго-западный лес

            // ===================== 3.4) ФАКТУРА РАВНИН =====================
            // Часть Meadow превращаем в Plain по шахматному паттерну, чтобы «оживить» картинку.
            for (int y = 2; y < H - 2; y++)
            {
                for (int x = 2; x < W - 2; x++)
                {
                    var t = map.Tiles[x, y].Terrain;
                    if (t == TerrainType.Meadow)
                    {
                        map.Tiles[x, y].Terrain = ((x + y) % 4 == 0)
                            ? TerrainType.Plain   // «светло-зелёная» равнина
                            : TerrainType.Meadow; // луг
                    }
                }
            }

            // ============ 4) РЕСУРСЫ ============

            // 4.1) Железо (Iron): «ленты» внутри горного гребня.
            // Раскладываем строго по горным клеткам (Mountain).
            /// <summary>
            /// Кладёт железо (Iron) «лентой» по горизонтали в диапазоне X на фиксированном Y.
            /// Ресурс ставится ТОЛЬКО в горных клетках (Mountain).
            /// </summary>
            /// <param name="x0">Левая граница полосы (включительно).</param>
            /// <param name="x1">Правая граница полосы (включительно).</param>
            /// <param name="y">Y-координата полосы.</param>
            void IronStripe(int x0, int x1, int y)
            {
                for (int x = x0; x <= x1; x++)
                {
                    // Не заходим в океанскую обводку и за границы
                    if (x < 2 || x >= W - 2 || y < 2 || y >= H - 2) continue;

                    var tile = map.Tiles[x, y];
                    if (tile.Terrain == TerrainType.Mountain)
                    {
                        tile.Resources.Add(new NaturalResource
                        {
                            Type = ResourceType.Iron,
                            Amount = 250 // условный объём
                        });
                    }
                }
            }

            // Три параллельные ленты руды внутри гребня
            IronStripe(22, 28, 18);
            IronStripe(22, 28, 24);
            IronStripe(22, 28, 30);

            // 4.2) Газ (Gas): круглые «лужицы» на равнинах/лугах.
            /// <summary>
            /// Рисует круглый (дисковый) кластер газа с центром (cx, cy) и радиусом r.
            /// Ресурс размещается ТОЛЬКО на Plain/Meadow.
            /// </summary>
            /// <param name="cx">X-координата центра круга.</param>
            /// <param name="cy">Y-координата центра круга.</param>
            /// <param name="r">Радиус круга в клетках.</param>
            void GasDisk(int cx, int cy, int r)
            {
                // Проходим по квадрату r×r вокруг центра и отбираем точки, попадающие в окружность
                for (int y = cy - r; y <= cy + r; y++)
                {
                    for (int x = cx - r; x <= cx + r; x++)
                    {
                        // Не трогаем края (океан) и границы
                        if (x < 2 || x >= W - 2 || y < 2 || y >= H - 2) continue;

                        // Условие круга: (x-cx)^2 + (y-cy)^2 <= r^2
                        if (Sq(x - cx) + Sq(y - cy) <= r * r)
                        {
                            var t = map.Tiles[x, y];

                            // Газ — только на равнине/лугу
                            if (t.Terrain == TerrainType.Plain || t.Terrain == TerrainType.Meadow)
                            {
                                t.Resources.Add(new NaturalResource
                                {
                                    Type = ResourceType.Gas,
                                    Amount = 200
                                });
                            }
                        }
                    }
                }
            }

            // Две «лужицы» газа
            GasDisk(14, 20, 3);
            GasDisk(30, 40, 4);

            // 4.3) Нефть (Oil): продолговатые овалы (низины) на равнинах/лугах.
            /// <summary>
            /// Рисует овальный (эллиптический) кластер нефти с центром (cx, cy)
            /// и полуосями rx, ry. Ресурс ставится ТОЛЬКО на Plain/Meadow.
            /// </summary>
            /// <param name="cx">X-координата центра эллипса.</param>
            /// <param name="cy">Y-координата центра эллипса.</param>
            /// <param name="rx">Полуось эллипса по оси X.</param>
            /// <param name="ry">Полуось эллипса по оси Y.</param>
            void OilOval(int cx, int cy, int rx, int ry)
            {
                // Идём по охватывающему прямоугольнику и оставляем точки, попадающие в эллипс
                for (int y = cy - ry; y <= cy + ry; y++)
                {
                    for (int x = cx - rx; x <= cx + rx; x++)
                    {
                        // Защиты от выхода в океанскую рамку и за карту
                        if (x < 2 || x >= W - 2 || y < 2 || y >= H - 2) continue;

                        if (InsideEllipse(x, y, cx, cy, rx, ry))
                        {
                            var t = map.Tiles[x, y];

                            // Нефть — только на равнине/лугу
                            if (t.Terrain == TerrainType.Plain || t.Terrain == TerrainType.Meadow)
                            {
                                t.Resources.Add(new NaturalResource
                                {
                                    Type = ResourceType.Oil,
                                    Amount = 220
                                });
                            }
                        }
                    }
                }
            }

            // Два овала нефти в разных регионах
            OilOval(40, 26, 5, 3);
            OilOval(12, 34, 4, 2);

            // Карта готова
            return map;
        }
    }
}