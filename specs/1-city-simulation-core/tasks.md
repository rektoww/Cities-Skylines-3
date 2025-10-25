# Task List: City Simulation Core

**Version**: 1.0
**Status**: Draft
**Author**: Kilo Code
**Last Updated**: 2025-10-25
**Related Plan**: [plan.md](plan.md)

---

## Phase 0: Refactoring & Stabilization

*Before starting new feature development, it is crucial to address the existing technical debt to ensure a stable foundation.*

-   [ ] **TASK-REF-001**: Fix `NullReferenceException` in `GameMap` constructor by initializing `_buildingsGrid`.
-   [ ] **TASK-REF-002**: Correct the namespace for `MapGeneratorService` to `Core.Services`.
-   [ ] **TASK-REF-003**: Refactor `ServiceBuilding` and `Citizen` to decouple business logic from models.
-   [ ] **TASK-REF-004**: Add XML documentation to all existing public classes and methods in the `Core` and `Infrastructure` projects.
-   [ ] **TASK-REF-005**: Clean up placeholder and unprofessional comments.

---

## Phase 1: Core Industrial & Construction Models (Your Tasks)

*This phase focuses on the detailed implementation of the manufacturing and construction sectors.*

### Sub-Phase 1.1: Construction Industry Models

-   [ ] **TASK-CONST-001**: Create `ConstructionCompany` class inheriting from `Building`. This entity will manage construction projects.
-   [ ] **TASK-CONST-002**: Create `ConstructionMaterial` enum/class (e.g., `Steel`, `Concrete`, `Glass`).
-   [ ] **TASK-CONST-003**: Implement a `ConstructionYard` building class where `ConstructionMaterial` is produced or stored.
-   [ ] **TASK-CONST-004**: Define an `IConstructable` interface for buildings that can be built by `ConstructionCompany`.
-   [ ] **TASK-CONST-005**: Implement the logic for a `ConstructionCompany` to consume `ConstructionMaterial` and time to build a new building on the map.

### Sub-Phase 1.2: Factory Workshop Models

-   [ ] **TASK-FACT-001**: Create a base `FactoryBuilding` class inheriting from `Building`. It should have properties for input resources and output products.
-   [ ] **TASK-FACT-002**: Create a `Workshop` class. A `FactoryBuilding` can contain multiple `Workshop` instances. Each `Workshop` will have a specific function (e.g., `AssemblyLine`, `SmeltingFurnace`).
-   [ ] **TASK-FACT-003**: Implement a `ProductionChain` concept. A `Workshop` takes specific input resources/materials and produces an output.
-   [ ] **TASK-FACT-004**: Create a `Machine` class that can be placed inside a `Workshop`. Machines can require maintenance and have an operational efficiency.
-   [ ] **TASK-FACT-005**: Implement logic for `Workshop` to process resources. This includes checking for required materials, worker availability, and machine status.

---

## Phase 2: Foundational Simulation (Context)

*High-level tasks to provide context for the overall project.*

-   [ ] **TASK-CTX-001**: Implement Map Generation and Resource Placement.
-   [ ] **TASK-CTX-002**: Implement basic Population (Citizen) lifecycle and needs.
-   [ ] **TASK-CTX-003**: Implement road network and basic pathfinding for movement.
-   [ ] **TASK-CTX-004**: Develop basic UI for map rendering and interaction.
-   [ ] **TASK-CTX-005**: Implement Save/Load functionality.

---

## Phase 3 & Beyond: Other Sectors (Context)

*High-level tasks for other simulation areas.*

-   [ ] **TASK-CTX-006**: Model Social & Emergency Services (Healthcare, Police, Fire Dept).
-   [ ] **TASK-CTX-007**: Model advanced Economy & Commerce.
-   [ ] **TASK-CTX-008**: Model advanced Transportation & Infrastructure.