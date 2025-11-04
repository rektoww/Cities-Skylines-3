using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Enums
{
    //// <summary>
    /// Перечисление типов бедствий, которые могут происходить в игре.
    /// </summary>
    public enum DisasterType
    {
        
        Earthquake, // Землетрясение - наносит урон зданиям и инфраструктуре.
        Fire, // Пожар - уничтожает деревья, парки и наносит урон зданиям.
        PowerGridFailure, // Авария в энергосети - отключает электричество.
        GasLeak, // Утечка газа - отключает газ и может вызвать взрыв.
        IndustrialAccident // Промышленная авария - наносит значительный урон заводам.
    }
}
