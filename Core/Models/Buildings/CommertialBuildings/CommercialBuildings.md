# Документация по коммерческим зданиям (Commercial Buildings)

Этот документ описывает конкретные реализации коммерческих зданий. Все они наследуются от абстрактного класса `CommercialBuilding` и реализуют интерфейс `IConstructable<T>`.

Каждый класс определяет свои уникальные характеристики (стоимость, вместимость, категории товаров) и **статические** свойства `BuildCost` и `RequiredMaterials`, необходимые для строительства.

## Глава 1: Cafe.cs

`Cafe` — представляет собой здание кафе.
- **Тип**: `CommercialBuildingType.Cafe`
- **Стоимость**: 80,000
- **Материалы**: 5 Steel, 5 Concrete

## Глава 2: GasStation.cs

`GasStation` — представляет собой автозаправочную станцию.
- **Тип**: `CommercialBuildingType.GasStation`
- **Стоимость**: 100,000
- **Материалы**: 5 Steel, 5 Concrete

## Глава 3: Pharmacy.cs

`Pharmacy` — представляет собой аптеку.
- **Тип**: `CommercialBuildingType.Pharmacy`
- **Стоимость**: 70,000
- **Материалы**: 5 Steel, 5 Concrete

## Глава 4: Restaurant.cs

`Restaurant` — представляет собой ресторан.
- **Тип**: `CommercialBuildingType.Restaurant`
- **Стоимость**: 150,000
- **Материалы**: 5 Steel, 5 Concrete

## Глава 5: Shop.cs

`Shop` — представляет собой небольшой магазин.
- **Тип**: `CommercialBuildingType.Shop`
- **Стоимость**: 50,000
- **Материалы**: 5 Steel, 5 Concrete

## Глава 6: Supermarket.cs

`Supermarket` — представляет собой супермаркет.
- **Тип**: `CommercialBuildingType.Supermarket`
- **Стоимость**: 200,000
- **Материалы**: 5 Steel, 5 Concrete

## Глава 7: CosmeticsFactory.cs
`CosmeticsFactory` — представляет собой фабрику косметики.
- **Тип**: `CommercialBuildingType.Factory`
- **Стоимость**: 300,000
- **Материалы**: 5 Steel, 8 Concrete, 4 Glass, 3 Plastic