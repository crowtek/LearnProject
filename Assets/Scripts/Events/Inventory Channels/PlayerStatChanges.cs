public struct PlayerStatChange
{
    // For Consumables
    public int healthChange;
    public bool curePoison;

    // For Equipment
    public int attackChange;
    public int defenseChange;
    public int agilityChange;
    // Use an int or string ID to tell the PlayerState WHICH item this is
    public string equipmentID;
}