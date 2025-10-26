# Refactoring & Tech Debt Checklist

**Purpose**: To track necessary code improvements and bug fixes identified during analysis.
**Created**: 2025-10-25

## Critical Bugs

- [ ] **`GameMap` NullReferenceException**: The `_buildingsGrid` array is not initialized in the constructor, which will cause a crash when `GetBuildingAt` or `SetBuildingAt` is called.

## Code Quality & Refactoring

- [ ] **Incorrect Namespace**: The class `MapGeneratorService` in `Core/Services/MapGeneratorService.cs` has the namespace `Infrastructure.Services` instead of `Core.Services`.
- [ ] **SOLID Violation in `ServiceBuilding`**: The `ProvideService` method directly modifies the state of `Citizen` objects. This logic should be extracted into a dedicated service (e.g., `CitizenActionService`) to adhere to the Single Responsibility Principle.
- [ ] **Model-Logic Coupling in `Citizen`**: The `Citizen` class contains business logic (`TryReproduce`, `Work`, `Study`). This logic should be moved to a service layer (e.g., `PopulationService`) to keep models as simple data containers.
- [ ] **Remove "Magic Numbers"**: `ResidentialBuilding` and `ServiceBuilding` use hardcoded values for default properties (capacity, size, etc.). These should be refactored into a configuration-based system or constants for better maintainability.
- [ ] **Inappropriate Comments**: The `Citizen.cs` file contains unprofessional comments that must be removed.
- [ ] **Incomplete Logic**: Methods like `Move` in `Car.cs` and parts of `Study` in `Citizen.cs` throw `NotImplementedException` or contain placeholder comments.

## Documentation

- [ ] **Add XML Comments**: None of the existing classes or public methods are documented. XML comments should be added to all public members as required by the project constitution.