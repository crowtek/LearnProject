Modular Turn-Based Battle System (Dragon Quest-Inspired)

A highly decoupled, modular 3D turn-based combat system and encounter loop built in Unity.
This project showcases modern architectural patterns, separating gameplay orchestration, UI layouts, and data persistence to create a scalable JRPG foundation.

🚀 Highlights for Recruiters
Event-Driven & Decoupled

Avoids tightly bound Singletons by relying on:

ScriptableObjects for data configuration
Global messaging channels for communication
Zero hard dependencies between gameplay states and UI
Modern UI Toolkit

Built entirely using Unity’s modern UI Toolkit (UIToolkit) instead of the legacy Canvas workflow, creating:

Clean UI hierarchy
Modular layouts
Better scalability and maintainability
Predictive Animation Timing

Implements a custom asynchronous animation workflow ensuring:

Combat animations
Battle logs
Damage popups
State transitions

all synchronize correctly without race conditions or fragile timer dependencies.

Robust Game Loop

Features:

Seamless additive scene transitions
Real-time random encounter calculations
Post-battle progression systems
Weapon-based skill progression trees
🛠️ Architecture & Script Breakdown
Core Systems
BattleManager.cs

Primary Finite State Machine (FSM) controlling:

PlayerTurn
EnemyTurn
Busy
Won/Lost

Responsibilities:

Dynamically initializes combatants from 3D prefabs
Executes combat actions
Handles clean combat teardown and scene unloading
EncounterManager.cs

Handles overworld encounter logic by:

Monitoring player movement
Calculating traveled distance on the horizontal XZ plane
Triggering random encounters after threshold checks
Loading battle scenes additively
ProgressionManager.cs

Listens to global battle event channels to:

Allocate EXP
Process level-ups
Upgrade player stats
Trigger progression systems
🎨 UI & Animation Systems
BattleUIController.cs

Presentation-layer controller responsible for:

Player/enemy stat displays
Animated combat logs
Damage popups
Dynamic UI action bindings

Designed as a fully decoupled UI module.

SkillDistroUIController.cs

Dedicated weapon-based skill tree interface allowing players to:

Spend progression points
Upgrade weapon specializations
Customize combat builds
CombatAnimator.cs

Coroutine-backed animation extension for Unity’s Animator.

Features:

Reads runtime animation clip states
Detects clip progression dynamically
Fires execution callbacks at ~90% completion
Keeps gameplay events perfectly synchronized with visuals
📦 Data Containers (ScriptableObjects)
BattleEntityData.cs

Defines:

Base stats
Scaling formulas
Combat values
Prefab references
BattleSkillData.cs

Contains:

Skill probabilities
Damage formulas
Animation references
AI usage weights
Combat metadata
BattleTransferDataSO.cs

Reliable cross-scene data bridge used for:

Passing enemy data
Preserving encounter state
Managing additive scene transitions safely
🧠 Technical Implementation & Engineering Pillars
1. Data-Driven Configuration via ScriptableObjects

All gameplay metrics and balancing values are externalized from executable code, including:

AI skill probabilities
Stat scaling
Visual configurations
Animation assignments

This enables rapid iteration directly inside the Unity Inspector without risking gameplay logic regressions.

2. Zero-Dependency UI Binding

UI controllers never directly reference:

Player state
Enemy logic
Combat systems

Instead:

BattleManager injects bindings dynamically during initialization
Delegates/lambdas connect actions at runtime
UI hooks are cleaned up through UnbindButtons() during teardown

This makes the entire presentation layer fully hot-swappable and modular.

3. Asynchronous Animation Sequencing

Instead of relying on fragile hardcoded timers, the system:

Reads live Animator runtime values
Tracks actual animation progress
Synchronizes gameplay execution with visual timing

Result:

Damage numbers
HP updates
Battle logs
Animation impacts

all trigger with frame-accurate consistency.

4. Flawless State Transitions & Lifecycle Management

The encounter lifecycle is carefully managed to avoid memory leaks and inconsistent states.

Encounter Start
Overworld triggers are paused
Primary camera is safely disabled
Battle scene loads additively
During Combat
Runtime battle data is isolated
UI and combat systems communicate through event channels
Encounter End
EXP and progression are written back to persistent runtime containers
Combat objects are explicitly destroyed
Additive battle scenes unload cleanly
Overworld state is restored seamlessly