using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Consumable")]
public class ConsumableItemSO : BaseItemSO, IUsableItem
{
    public override string ItemTypeDisplayName => "Item";

    [Header("Effect Settings")]
    public int healAmount;
    public bool curesPoison;

    public bool Use(PlayerRuntimeState player)
    {
        bool used = false;

        if (healAmount > 0 && player.currentHP < player.maxHP)
        {
            player.currentHP = Mathf.Min(player.currentHP + healAmount, player.maxHP);
            Debug.Log($"{itemName} used. Healed {healAmount} HP.");
            used = true;
        }

        if (curesPoison && player.isPoisoned)
        {
            player.isPoisoned = false;
            Debug.Log($"{itemName} cured poison!");
            used = true;
        }
        

        return used;
    }
}