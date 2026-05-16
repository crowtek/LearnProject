🏗️ Core Architecture Overview
This item system uses a Data-Driven Observer Pattern powered by ScriptableObjects. The data layer never directly looks at or talks to the UI layer. Instead, communication happens via Event Channels (ScriptableObject channels).

[World Pickup / Input] ──(Mutates Data)──> [InventorySO]
                                                │
                                       (Fires SO Radio Event)
                                                │
                                                ▼
                                    [Event Channel SO Assets]
                                                │
                                       (Listens & Reacts)
                                                │
                                                ▼
                                  [UI Controller / Player Stats]


📡 Event Channels & Connections
These are the radio stations that decouple your code. You need to create these assets and link them to the following scripts in the inspector:

1. onInventoryChanged (VoidEventChannelSO)
Who fires it: InventorySO (automatically whenever items are added, removed, or equipment changes).

Who listens to it: ItemMenuUIController (catches the event and automatically calls RefreshUI()).

Purpose: Ensures the inventory UI matches the actual data perfectly without manual micro-management.

2. equipmentChannel (EquipmentChangeChannelSO)
Who fires it: InventorySO (whenever an item is equipped or unequipped).

Who listens to it: PlayerRuntimeState (catches the data payload and adds/subtracts stat bonuses).

Purpose: Completely isolates your player stats logic from your inventory container logic.

🗂️ Inspector Dependency Checklist
When setting up your scenes or creating new items, use this quick checklist to ensure everything is wired correctly in the Inspector.

🎒 The Main Asset: InventorySO
equipmentChannel: Link your global EquipmentChangeChannelSO asset.

onInventoryChanged: Link your global VoidEventChannelSO asset.

playerState: Link your PlayerRuntimeState asset (so items can access and heal the player).

🖥️ The Scene Object: ItemMenuUIController
inventoryData: Link your main InventorySO asset.

onInventoryChanged: Link the exact same VoidEventChannelSO assigned in your inventory.

inventoryDocument: Link the UIDocument component inside the scene.

inventoryToggleAction: Link your Input System action map key to open/close the menu.

📦 The World Objects: ItemPickup
itemData: Link the specific BaseItemSO derivative (Potion, Sword, etc.) this object represents.

inventory: Link your main InventorySO asset.

amount: Set the quantity of items given on pickup.

🚀 Quick-Use Integration Guide
To add items from anywhere in code (Chests, Quests, Drops):
C#
// Just call this. The asset handles the data math AND tells the UI to update automatically.
inventory.AddItem(itemData, amount);
To create a new item type:
Inherit from BaseItemSO.

If it's a Consumable, implement IUsableItem and write your effect inside the bool Use(PlayerRuntimeState player) method. Return true only if the item was successfully consumed (e.g., don't consume a potion if the player is at full health).

If it's Equipment, inherit from EquipmentItemSO and set the target EquipmentSlot and stat parameters in the inspector.