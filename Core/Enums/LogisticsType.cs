using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Enums
{
    /// <summary>
    /// Типы логистических компаний и услуг
    /// </summary>
    public enum LogisticsType
    {
        // По типу транспорта
        TruckingCompany,
        RailwayLogistics,
        AirCargo,
        MaritimeShipping,
        CourierServices,

        // По специализации
        RefrigeratedLogistics,
        BulkCargo,
        ContainerShipping,
        ExpressDelivery,
        WarehouseLogistics,

        // По охвату
        LocalLogistics,
        RegionalLogistics,
        InternationalLogistics
    }

    /// <summary>
    /// Типы перевозимых грузов в логистике
    /// </summary>
    public enum CargoType
    {
        GeneralCargo,
        PerishableGoods,
        DangerousGoods,
        OversizedCargo,
        FragileGoods,
        LiquidCargo,
        BulkMaterials,
        RefrigeratedGoods,
        LiveAnimals,
        Documents,
        HighValueGoods
    }

    /// <summary>
    /// Типы транспортных средств в логистике
    /// </summary>
    public enum LogisticsVehicle
    {
        Truck,
        RefrigeratedTruck,
        TankerTruck,
        FlatbedTruck,
        ContainerTruck,

        DumpTruck,
        CarCarrier,
        LivestockTruck,

        CargoPlane,
        Helicopter,

        CargoShip,
        ContainerShip,
        Tanker
    }
}