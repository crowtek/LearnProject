using UnityEngine;

public class PlayerEqHandler : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private InventorySO inventoryData;

    [Header("Listening Channels")]
    [SerializeField] private EquipmentChangeChannelSO equipmentChannel;

    private GameObject currentSpawnedWeaponInstance;

    // wappon Spawn Point changes
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    private void Start()
    {

        if (inventoryData != null && inventoryData.currentlyEquipped != null)
        {
            EquipmentItemSO equippedWeapon = inventoryData.currentlyEquipped.Find(x => x.slot == EquipmentSlot.Weapon);
            if (equippedWeapon != null)
            {
                SpawnWeapon(equippedWeapon);
            }
        }
    }

    private void OnEnable()
    {
        if (equipmentChannel != null)
        {
            equipmentChannel.OnEventRaised += OnEquipmentChanged;
        }
    }

    private void OnDisable()
    {
        if (equipmentChannel != null)
        {
            equipmentChannel.OnEventRaised -= OnEquipmentChanged;
        }
    }

    private void OnEquipmentChanged(EquipmentChange change)
    {
        if (change.slot == EquipmentSlot.Weapon)
        {
            if (change.isEquipping)
            {
                EquipmentItemSO newWeapon = inventoryData.currentlyEquipped.Find(x => x.slot == change.slot);
                SpawnWeapon(newWeapon);
            }
            else
            {
                DespawnCurrentWeapon();
            }
        }
    }

    private void SpawnWeapon(EquipmentItemSO weaponItem)
    {

        DespawnCurrentWeapon();

        if (weaponItem == null || weaponItem.weaponPrefab == null) return;

        if (weaponHoldPoint == null)
        {
            Debug.LogError($"[PlayerEqHandler] Kein weaponHoldPoint (Hand-Transform) zugewiesen!", this);
            return;
        }

        // Neue Waffe als Child des Hand-Ankerpunkts spawnen
        currentSpawnedWeaponInstance = Instantiate(weaponItem.weaponPrefab, weaponHoldPoint);

        currentSpawnedWeaponInstance.transform.localPosition = positionOffset;
        currentSpawnedWeaponInstance.transform.localEulerAngles = rotationOffset;
    }

    private void DespawnCurrentWeapon()
    {
        if (currentSpawnedWeaponInstance != null)
        {
            Destroy(currentSpawnedWeaponInstance);
            currentSpawnedWeaponInstance = null;
        }
    }
}