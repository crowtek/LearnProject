# Modular Quest System (ScriptableObject Architecture)

A decoupled, event-driven Quest System for Unity built entirely on **ScriptableObjects**. This architecture eliminates scene-dependencies, avoids the use of fragile singletons (`DontDestroyOnLoad`), and minimizes memory overhead by separating static game data from runtime session state.

## Architecture Overview

The system is split into three core pillars following the **SOLID principles** (Single Responsibility & Dependency Inversion):

1. **Static Data (Assets):** `QuestDataSO` and `QuestDatabaseSO` act as the data library. They define what a quest is and what quests exist in the game.
2. **Runtime Logic (State Manager):** `QuestManagerSO` handles the active runtime state and evaluates quest conditions.
3. **User Interface (UI View):** `QuestUIController` listens to dedicated event channels to render state changes without directly referencing the underlying logic manager.

## Script Breakdown

### 1. QuestDataSO.cs & QuestDatabaseSO.cs
* **`QuestDataSO`**: A configuration asset defining individual quests (Name, Description, Required Story Flag to unlock, and Completion Flag).
* **`QuestDatabaseSO`**: Acts as the central library/compiled list of all available quests in the entire game.

### 2. QuestManagerSO.cs
The brain of the system. It is a globally accessible ScriptableObject asset that:
* Listens to the global `storyFlagChangedBroadcastChannel`.
* Advances, activates, or completes quests based on incoming story events.
* Maintains runtime lists (`activeQuest` and `completedQuests`) that are safely encapsulated from external manipulation.
* Features a secure `Initialize()` method invoked at bootscreen/game start to reset session state and avoid persistent data leaks in the Unity Editor.

### 3. QuestUIController.cs
A lightweight UI Toolkit controller that:
* Binds to a neutral `BoolEventChannelSO` (`questChangedChannel`).
* Clears and redraws the UI container safely using robust null-checks (`ActiveQuest == null`) to completely prevent `NullReferenceExceptions` when no quest is active.

---

## How It Works (The Event Flow)

1. **Bootstrapping:** At game start, a central `GameInitializer` invokes `QuestManagerSO.Initialize()` to clear old runtime states and bind event listeners.
2. **Activation:** When the player triggers a progression milestone, a `StoryTrigger` fires a flag (e.g., `"TalkedToKing"`).
3. **Evaluation:** The `QuestManagerSO` catches this flag, checks the database, and moves the corresponding quest into the `activeQuest` slot. It then blinks a signal through the `questChangedChannel`.
4. **Rendering:** The `QuestUIController` receives the event signal and updates the UI Toolkit document safely.

---

## Key Features & Professional Highlights

* **Zero Scene Overhead:** No quest manager component needs to be duplicated or dragged into new gameplay scenes. The manager lives entirely in the project folder.
* **Strict Encapsulation:** Runtime variables are private and exposed to the UI via read-only properties (`public QuestDataSO ActiveQuest => activeQuest;`), ensuring data integrity.
* **Garbage-Collection & Memory Leak Prevention:** Explicit un-subscription routines in the initialization blocks ensure event handlers are kept clean across multiple play sessions in the editor.