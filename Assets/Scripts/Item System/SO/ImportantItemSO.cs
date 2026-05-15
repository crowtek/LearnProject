using UnityEngine;

[CreateAssetMenu(fileName = "New Key Item", menuName = "Items/Important")]
public class ImportantItemSO : BaseItemSO
{
    public override string ItemTypeDisplayName => "Important Item";
    // Usually no 'Use' logic, just existence checks in the code
}