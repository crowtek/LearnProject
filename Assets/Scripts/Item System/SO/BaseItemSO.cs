using UnityEngine;

public abstract class BaseItemSO : ScriptableObject
{
    [Header("Display Info")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public int goldValue;

    public abstract string ItemTypeDisplayName { get; }
}

public interface IUsableItem
{
    bool Use(PlayerRuntimeState player);
}

public interface IEquipableItem
{
    void Equip(PlayerRuntimeState player);
    void Unequip(PlayerRuntimeState player);
}