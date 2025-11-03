# Документация по моделям зданий (Building Models)

Этот документ описывает различные типы зданий, которые существуют в симуляции. Все они наследуются от базового класса `Building`.

## Глава 1: CommercialBuilding.cs

`CommercialBuilding` представляет собой **абстрактный базовый класс** для всех коммерческих зданий. Он наследуется от `Building` и предоставляет общую функциональность для магазинов, кафе и других коммерческих заведений. Конкретные реализации (такие как `Shop`, `Cafe`) наследуются от этого класса.

### Свойства

- **`Type`**: `CommercialBuildingType` - Тип коммерческого здания (`Shop`, `Cafe` и т.д.).
- **`Capacity`**: `int` - Вместимость здания.
- **`EmployeeCount`**: `int` - Количество сотрудников.
- **`ProductCategories`**: `List<string>` - Категории товаров/услуг.

## Глава 2: ResidentialBuilding.cs

`ResidentialBuilding` — это жилое здание, в котором могут проживать горожане (`Citizen`).

### Свойства

- **`Type`**: `ResidentialType` - Тип жилого здания (`Apartment`, `Dormitory`, `Hotel`).
- **`Capacity`**: `int` - Максимальное количество жителей.
- **`CurrentResidents`**: `List<Citizen>` - Список текущих жителей.
- **`HasVacancy`**: `bool` - Показывает, есть ли свободные места.
- **`IsOperational`**: `bool` - Работоспособно ли здание (требуются все коммуникации).

### Методы

- **`TryAddResident(Citizen citizen)`**: `bool` - Пытается заселить жителя в здание.

## Глава 3: ServiceBuilding (и наследники)

### ServiceBuilding.cs
`ServiceBuilding` — это **базовый класс** для зданий, предоставляющих горожанам различные услуги (например, образование или здравоохранение).

#### Свойства
- **`Type`**: `ServiceBuildingType` - Тип сервисного здания (`School`, `Hospital`, `College`, `University`).
- **`Capacity`**: `int` - Максимальное количество клиентов.
- **`Clients`**: `List<Citizen>` - Список текущих клиентов.
- **`IsOperational`**: `bool` - Работоспособно ли здание.

#### Методы
- **`TryAddClient(Citizen citizen)`**: `bool` - Пытается добавить нового клиента.
- **`ProvideService()`**: `void` - Предоставляет услугу всем клиентам.

### Конкретные реализации
- **`SchoolBuilding.cs`**: Представляет школу. Наследуется от `ServiceBuilding` и в конструкторе устанавливает `ServiceBuildingType.School`.
- **`CollegeBuilding.cs`**: Представляет колледж. Наследуется от `ServiceBuilding` и в конструкторе устанавливает `ServiceBuildingType.College`.
- **`UniversityBuilding.cs`**: Представляет университет. Наследуется от `ServiceBuilding` и в конструкторе устанавливает `ServiceBuildingType.University`.

## Глава 4: BusStop.cs

`BusStop` — представляет собой автобусную остановку. Наследуется от абстрактного класса `TransitStation`.

### Особенности
- Реализует `IConstructable<BusStop>`, определяя собственную стоимость и материалы для строительства.
- **Логика размещения**: Переопределяет метод `CanPlace`, требуя, чтобы остановка размещалась только на тайле с дорогой (`tile.HasRoad`).

---

*Примечание: Классы `ExtractionFacility`, `ProcessingPlant` и `WoodProcessingFactory` были вынесены из иерархии `Building` и больше не являются зданиями в текущей архитектуре.*