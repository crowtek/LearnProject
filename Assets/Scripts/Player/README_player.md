# Player Architecture Overview

A modular, lightweight setup handling player locomotion, input routing, and world interactions. Designed around a separation of concerns to remain scalable and easy to debug.

---

## 🛠️ Script Overview & Architecture

                   [ InputSystem_Actions ]
                              │
                              ▼
                   [ PlayerInputHandler ]
                              │
     ┌────────────────────────┴────────────────────────┐
     ▼ (Passes Vector2)                                 ▼ (Disables via Events)
[ PlayerMovement ]                                [ PlayerInteractor ]
│                                                 │
▼ (Moves Character)                               ▼ (Detects)
[ CharacterController ]                            [ IInteractable Objects ]
│
▼ (Positions & Faces Camera)
[ InteractionUIController ]


### 1. `PlayerInputHandler.cs`
* **What it does:** Reads raw value data from Unity's New Input System.
* **How it works:** Polls movement data inside `Update()` and pushes it directly into `PlayerMovement`.
* **Dev Note:** Listens to a `BoolEventChannelSO` (`toggleInputChannel`) to cleanly enable or disable all player input during events like dialogues or menus.

### 2. `PlayerMovement.cs`
* **What it does:** Handles physical movement, snappy gravity calculations, and model rotation.
* **How it works:** * Processes movement inside `FixedUpdate()` utilizing `Time.fixedDeltaTime`.
  * Projects movement relative to the Main Camera's forward/right vectors (flattens out the Y-axis so looking downward doesn't modify move speeds).
  * Implements custom snappy gravity via `gravityMultiplier` and a flat `groundedGravity` to keep the player glued smoothly to ramps and slopes.

### 3. `PlayerInteractor.cs`
* **What it does:** Detects interactable objects within proximity.
* **How it works:** * Uses `Physics.OverlapSphereNonAlloc` inside its logic to check for custom `IInteractable` components on a designated layer (`interactLayer`).
  * Passes the closest target over to the UI or triggers `.Interact()` when the interaction key is pressed.
  * Automatically ignores tracking entirely if a dialogue system raises an active event on `dialogueEventChannel`.

### 4. `InteractionUIController.cs`
* **What it does:** Controls the visual prompt/interaction bubble appearing above world items.
* **How it works:** Snaps its screen position to whatever target the interactor feeds it, executing a quick face-camera projection (`transform.LookAt()`) inside `LateUpdate()` so text or icons remain completely flat relative to your view.

---

## 🚀 Quick Setup Guide for Devs

1. Ensure the Player prefab contains a **Character Controller**, **Animator**, `PlayerMovement`, `PlayerInputHandler`, and `PlayerInteractor`.
2. Link the references across the components in the inspector.
3. **Camera Setup:** If setting up a classic locked top-down layout, configure your *CinemachineCamera* **Position Control** to `Follow` and its **Binding Mode** to `World Space` so that movement calculations correlate perfectly with screen orientation.

## ⚠️ Core Coding Rules for this Module
* **No Physics in Update:** Always keep horizontal movement and gravity updates inside `FixedUpdate()` on `PlayerMovement` to avoid jerky motion. Use `Time.fixedDeltaTime`.
* **Input Routing:** Do not read `InputSystem_Actions` directly inside your gameplay l