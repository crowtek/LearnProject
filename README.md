# Sovereign Chronicles

**A modular Unity JRPG architecture prototype focused on gameplay systems, reusable tooling, and decoupled system design.**

Sovereign Chronicles is a systems-focused Unity learning project built to explore how RPG gameplay features can be structured in a scalable and maintainable way. The project is not intended to be a fully polished commercial game. Instead, it demonstrates my ability to design, connect, and document gameplay systems using professional Unity architecture patterns.

The core focus is on separating data, runtime state, gameplay logic, and presentation so that each system can be developed and extended independently.

---

## Focus Areas

* Gameplay Programming
* Unity Architecture
* ScriptableObject-Driven Systems
* Event-Driven Communication
* Editor Tooling
* Runtime State Management
* Modular RPG Feature Design

---

## Project Preview

[Watch gameplay demo](./gameplay-demo.mp4)

Recommended media:

* Short gameplay clip showing overworld interaction
* Inventory/equipment interaction screenshot or GIF
* Dialogue and story flag progression clip
* Turn-based battle system preview
* Architecture diagram of the inventory or event-channel system

---

## Technical Summary

The project is built around a **ScriptableObject-driven, event-based architecture**. Core systems communicate through event channels instead of direct references or global Singleton dependencies.

This approach keeps systems such as battle, dialogue, inventory, quests, and UI loosely coupled. It also makes the project easier to refactor, test, and expand with new gameplay features.

Key architectural choices:

* ScriptableObjects for static game data and configuration
* Runtime state objects for player/session data
* Event channels for communication between isolated systems
* Assembly Definitions for stronger module boundaries
* Custom editor tooling to reduce setup errors
* UI controllers that react to gameplay events instead of directly controlling game logic

---

## Main Systems

### Battle System

A modular turn-based battle system inspired by classic JRPG combat loops.

Implemented features include:

* Turn state management
* Player and enemy action flow
* Enemy AI turns
* ScriptableObject-based skills
* Encounter zones
* Battle rewards and progression integration
* Scene transition handoff between overworld and battle scenes

The battle system is designed around clear separation between gameplay logic, combat UI, enemy data, skills, encounter setup, and audio feedback.

---

### Inventory & Item System

A data-driven item system using ScriptableObjects for item definitions and runtime inventory state for active player data.

Implemented features include:

* Reusable item assets
* Consumable items
* Equipment items
* Important/story items
* Inventory slots
* Item pickups
* Equipment stat changes
* Inventory UI updates through event channels

The system separates item definitions from runtime inventory state. This allows item data to be edited directly in the Unity Editor while keeping gameplay state consistent during runtime.

---

### Player Runtime State

A central runtime state asset acts as the single source of truth for player progression.

Tracked data includes:

* Player stats
* Current HP and experience
* Level progression
* Unlocked skill points
* Equipment state
* Animation trigger names
* Runtime combat references

Combat, UI, inventory, and equipment systems read from the same shared runtime state to avoid duplicated or inconsistent player data.

---

### Dialogue System

A ScriptableObject-based dialogue system for managing character dialogue and story progression.

Implemented features include:

* Dialogue databases
* Dialogue entries
* Dialogue UI controller
* NPC dialogue triggers
* Story flag integration
* Dropdown-based editor tooling for safer dialogue setup

The dialogue system is designed to be reusable across different scenes and projects without requiring direct scene-specific references.

---

### Story Flag System

A global progression system for tracking important story states and world changes.

Implemented features include:

* Story flag databases
* Runtime story progression state
* Event-driven story updates
* Integration with dialogue, quests, and world interactions

Story flags help keep narrative progression centralized instead of spreading progression checks across unrelated scripts.

---

### Quest System

A state-based quest system connected to dialogue, story flags, and UI updates.

Implemented features include:

* Quest data assets
* Quest runtime tracking
* Quest state changes
* Quest UI updates
* Story flag requirements
* Event-driven quest progression

The goal of the quest system is to keep quest data, quest state, and quest presentation separated from each other.

---

### Editor Tooling

Custom editor tooling was added to improve usability and reduce fragile manual setup.

Implemented tooling includes:

* Custom PropertyDrawers
* ScriptableObject-driven dropdowns
* Safer story flag selection
* Safer dialogue option selection
* Reduced string-reference errors

This makes the systems easier to configure inside the Unity Editor and helps prevent mistakes caused by manually typed string identifiers.

---

## Architecture Overview

The project follows a layered architecture:

### 1. Data Layer

ScriptableObjects define static game content such as:

* Items
* Skills
* Dialogue entries
* Story flags
* Quests
* Encounter zones

### 2. Runtime State Layer

Runtime state objects track active gameplay data such as:

* Player stats
* Inventory contents
* Active quests
* Completed story flags
* Current progression state

### 3. Communication Layer

ScriptableObject event channels broadcast changes between systems.

Examples:

* Inventory changed
* Equipment changed
* Story flag updated
* Player state changed
* Battle transition requested

### 4. Logic Layer

Managers and controllers process gameplay rules, including:

* Battle flow
* Quest progression
* Inventory operations
* Dialogue progression
* Scene transitions

### 5. Presentation Layer

UI controllers listen for changes and update visual elements without owning core gameplay logic.

This keeps UI code focused on presentation instead of becoming tightly coupled to systems such as inventory, battle, or quests.

---

## Why This Architecture

The project was built to practice maintainable Unity development patterns that become important once multiple gameplay systems need to interact.

The main problems I wanted to solve were:

* Avoiding tightly coupled Singleton dependencies
* Preventing UI from directly controlling gameplay systems
* Separating static data from runtime state
* Making systems reusable across scenes
* Reducing fragile string references in editor setup
* Keeping gameplay systems easier to debug and extend

---

## What I Learned

### Decoupled communication

Using ScriptableObject event channels helped me understand how systems can communicate without directly depending on each other.

### Data-driven design

Moving configuration into ScriptableObjects made the project easier to iterate on and reduced the need for hardcoded gameplay values.

### Runtime state separation

Separating static data from runtime state helped prevent stale data, hidden dependencies, and confusing scene-specific behavior.

### UI as a listener

Building UI around event reactions made the interface easier to maintain and reduced direct dependencies on gameplay systems.

### System integration

The most valuable learning came from connecting multiple systems together: dialogue, quests, story flags, inventory, player progression, scene management, and battle flow.

---

## Current Development Focus

The project is still being expanded and refactored. Current focus areas include:

* Addressables for asset management
* More advanced custom editor tooling
* Automated Unity testing
* Combat-system scalability
* Cleaner assembly/module boundaries
* Further runtime-state improvements

---

## Project Status

This is an active architecture and gameplay systems prototype.

The main goal is not content volume or visual polish. The main goal is to demonstrate clean gameplay system design, modular architecture, and practical Unity engineering habits.

---

## Repository Structure

```text
Assets/
├── Scripts/
│   ├── Audio/
│   ├── Battle System/
│   ├── Dialog/
│   ├── Editor/
│   ├── Event/
│   ├── Item System/
│   ├── Player/
│   ├── QuestSystem/
│   ├── Scene Manager/
│   ├── Settings System/
│   ├── StoryManager/
│   └── Utils/
```

---

## Recruiter Note

This project is intended to showcase how I approach Unity gameplay programming from a systems and architecture perspective.

It demonstrates practical experience with:

* Modular gameplay systems
* ScriptableObject architecture
* Event-driven communication
* Runtime state management
* Editor tooling
* UI/gameplay separation
* Turn-based combat structure
* RPG system integration

The strongest focus of this project is not the amount of content, but the way the systems are structured, connected, and prepared for future expansion.
