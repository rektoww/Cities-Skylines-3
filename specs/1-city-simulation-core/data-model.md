# Data Model: City Simulation Core

**Version**: 1.0
**Status**: Draft
**Author**: Kilo Code
**Last Updated**: 2025-10-25
**Related Plan**: [plan.md](plan.md)

## 1. Core Entities

This document details the structure of the primary data models for the city simulation.

### 1.1. `GameObject` (Base Class)
The absolute base class for any object that exists in the game world.

-   **`Id`**: `Guid` - Unique identifier for the object.
-   **`Name`**: `string` - Display name of the object.
-   **`Position`**: `Point` - X, Y coordinates on the game map.

### 1.2. `Tile`
Represents a single cell on the map grid.

-   **`TerrainType`**: `enum` (`Grass`, `Water`, `Mountain`) - The type of terrain.
-   **`NaturalResource`**: `NaturalResource` (nullable) - The resource deposit on this tile, if any.
-   **`Building`**: `Building` (nullable) - The building on this tile, if any.

### 1.3. `GameMap`
Contains the grid of tiles and manages map-level data.

-   **`Width`**: `int` - The width of the map in tiles.
-   **`Height`**: `int` - The height of the map in tiles.
-   **`Tiles`**: `Tile[,]` - 2D array representing the map grid.

### 1.4. `NaturalResource`
Represents a deposit of a raw resource.

-   **Inherits from**: `GameObject`
-   **`ResourceType`**: `enum` (`Iron`, `Copper`, `Oil`, `Gas`, `Wood`) - The type of resource.
-   **`Amount`**: `int` - The quantity of resource remaining in the deposit.

## 2. Building Entities

### 2.1. `Building` (Base Class)
Base class for all structures that can be placed on the map.

-   **Inherits from**: `GameObject`
-   **`Size`**: `Size` - The footprint of the building in tiles (e.g., 2x2).
-   **`Condition`**: `double` - Structural integrity, from 1.0 (perfect) to 0.0 (destroyed).
-   **`MaintenanceCost`**: `decimal` - Cost per simulation tick to maintain the building.

### 2.2. `ResidentialBuilding`
A building where citizens live.

-   **Inherits from**: `Building`
-   **`Capacity`**: `int` - The maximum number of citizens that can live here.
-   **`Residents`**: `List<Citizen>` - The list of citizens currently living in the building.
-   **`Floors`**: `int` - Number of floors.

## 3. Mob (Mobile Object) Entities

### 3.1. `Mob` (Base Class)
Base class for any mobile object in the game.

-   **Inherits from**: `GameObject`
-   **`CurrentPath`**: `List<Point>` - The sequence of points to the destination.
-   **`Speed`**: `double` - Movement speed in tiles per simulation tick.

### 3.2. `Citizen`
Represents a person in the simulation.

-   **Inherits from**: `Mob`
-   **`Age`**: `int` - The citizen's age in years.
-   **`EducationLevel`**: `enum` (`None`, `School`, `College`, `University`)
-   **`Workplace`**: `Building` (nullable) - The building where the citizen works.
-   **`Home`**: `ResidentialBuilding` (nullable) - The building where the citizen lives.

### 3.3. `Car`
A basic vehicle for transport.

-   **Inherits from**: `Mob`
-   **`Owner`**: `Citizen` (nullable) - The owner of the car.
