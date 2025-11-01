# Implementation Plan: City Simulation Core

**Version**: 1.0
**Status**: Draft
**Author**: Kilo Code
**Last Updated**: 2025-10-25
**Related Spec**: [spec.md](spec.md)

## 1. Technical Context

-   **Project Type**: Desktop Application
-   **Language/Framework**: C# 9, .NET 9, WPF
-   **Architecture**: MVVM (Model-View-ViewModel)
-   **Core Libraries**: None specified yet.
-   **Key Dependencies**: None specified yet.
-   **Data Storage**: Save/Load to local files (format to be determined, likely JSON or binary).
-   **Integration Points**: Modules will interact via interfaces defined in the `Core` project.

## 2. Constitution Check

-   **[Manual File Creation]**: All file creation will be explicitly requested or performed by the user.
-   **[Restricted File Edits]**: No edits will be made to `*.csproj` or `*.json` files. Only `*.cs` and `*.xaml` files will be modified.
-   **[Code Documentation]**: All new code will be documented with XML comments for classes, methods, and complex logic.

## 3. Implementation Phases

This plan breaks down the development into logical phases, starting with foundational models and gradually adding complexity.

### Phase 0: Refactoring & Stabilization

Before adding new features, this phase addresses the technical debt found in the existing codebase. All items are detailed in the [`refactoring-checklist.md`](checklists/refactoring-checklist.md).

-   **Tasks**:
    -   Fix critical bug in `GameMap` constructor.
    -   Correct namespace in `MapGeneratorService`.
    -   Refactor `ServiceBuilding` and `Citizen` to separate logic from models.
    -   Remove hardcoded "magic numbers" from building classes.
    -   Add XML documentation to all existing public classes and methods.
    -   Remove or clean up unprofessional/placeholder comments.

### Phase 1: Core Models & Map (Items 1, 3, 4)

This phase focuses on creating the fundamental data structures for the simulation, building upon the stabilized codebase.

-   **data-model.md**: Review and extend the core entities.
-   **Entities to implement/extend**:
    -   `GameMap`, `Tile`, `NaturalResource`
    -   `Building` (Base), `ResidentialBuilding`, `CommercialBuilding`
    -   `Citizen` (Base)
    -   `GameObject` (Base)
    -   `Mob` (Base)

### Phase 2: Basic Interaction & UI (Items 2, 5, 6)

This phase introduces basic user interaction and visual representation.

-   **Entities to implement/extend**:
    -   `Road` (new entity)
    -   `Car` (Base)
-   **Services to implement**:
    -   `SaveLoadService` (in `Infrastructure`)
    -   `PathfindingService` (in `Core`)
-   **ViewModels/Views**:
    -   `MainViewModel`, `MainWindow`
    -   `MapView` (new view for rendering the map)

### Phase 3: Social & Emergency Services (Items 7-11)

This phase adds complexity to the social simulation.

-   **Entities to implement/extend**:
    -   `ServiceBuilding` (e.g., `Hospital`, `PoliceStation`, `FireStation`, `School`)
    -   `EmergencyVehicle` (e.g., `FireTruck`, `Ambulance`)
    -   `Disaster` (new entity)

### Phase 4: Economy & Industry (Items 12, 15, 17-26, 28-32)

This phase builds out the economic engine of the simulation.

-   **Entities to implement/extend**:
    -   `CommercialBuilding` (e.g., `Shop`, `Factory`)
    -   `IndustrialBuilding` (e.g., `Mine`, `PowerPlant`)
    -   `Resource` (new entity for produced goods)

### Phase 5: Advanced Infrastructure (Items 13, 14, 16)

This phase enhances the city's infrastructure.

-   **Entities to implement/extend**:
    -   `PublicTransportVehicle`
    -   `PedestrianPath`
    -   `Airport`, `Seaport`

## 4. Unresolved Decisions / Research Needed

-   **[NEEDS CLARIFICATION: Visual Style]**: Awaiting final decision on the visual style and color palette.
-   **[NEEDS CLARIFICATION: Economic Model]**: Awaiting decision on the core economic principles (currency, prices, wages).
-   **[RESEARCH NEEDED: Pathfinding Algorithm]**: Research and select an efficient pathfinding algorithm (e.g., A*, Dijkstra) for vehicle and citizen movement.
-   **[RESEARCH NEEDED: Save/Load Format]**: Determine the best format for saving and loading game state (e.g., JSON, BinaryFormatter, XML) considering performance and versioning.
