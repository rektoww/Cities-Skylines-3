using System;

namespace Core.Enums
{
    /// <summary>
    /// Типы дорог
    /// </summary>
    public enum RoadType
    {
        /// <summary>
        /// Обычная дорога (по умолчанию)
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Проселочная/грунтовая дорога
        /// </summary>
        Dirt = 1,

        /// <summary>
        /// Проспект/широкая дорога
        /// </summary>
        Avenue = 2,

        /// <summary>
        /// Шоссе/автострада
        /// </summary>
        Highway = 3,

        /// <summary>
        /// Односторонняя дорога
        /// </summary>
        OneWay = 4,

        /// <summary>
        /// Пешеходная дорожка
        /// </summary>
        Pedestrian = 5,
        Street = 6
    }
}