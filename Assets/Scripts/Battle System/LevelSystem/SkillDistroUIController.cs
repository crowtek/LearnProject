using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponSkillRowInstance
{
    public WeaponCategorySO config;
    public Label pointsLabel;
}

public class SkillDistroUIController : MonoBehaviour
{
    [Header("UI Templates & Containers")]
    [SerializeField] private VisualTreeAsset weaponRowTemplate;
    [Header("Data Configurations")]
    [SerializeField] private List<WeaponCategorySO> activeWeaponCategories;

    private VisualElement skillDistroWindow;
    private VisualElement skillRowsContainer;
    private Label unspentPointsLabel;

    private Label detailTitleLabel;
    private Label detailDescLabel;
    private Label weaponSkillAddition;

    private List<WeaponSkillRowInstance> spawnedRows = new List<WeaponSkillRowInstance>();
    private Dictionary<string, int> tempAllocations = new Dictionary<string, int>();
    private int tempUnspent;
    private System.Action onDistributionComplete;

    public void Initialize(VisualElement root)
    {
        skillDistroWindow = root.Q<VisualElement>("SkillDistroWindow");
        unspentPointsLabel = root.Q<Label>("UnspentPointsValue");
        skillRowsContainer = root.Q<VisualElement>("SkillRowsContainer");

        detailTitleLabel = root.Q<Label>("DetailTitleLabel");
        detailDescLabel = root.Q<Label>("DetailDescriptionLabel");
        weaponSkillAddition = root.Q<Label>("WapponSkillAddition");

        if (skillDistroWindow != null)
        {
            skillDistroWindow.style.display = DisplayStyle.None;
            skillDistroWindow.pickingMode = PickingMode.Ignore;
        }
    }

    public void SetupDynamicWeaponRows(PlayerRuntimeState player)
    {
        if (weaponRowTemplate == null || skillRowsContainer == null) return;

        skillRowsContainer.Clear();
        spawnedRows.Clear();
        tempAllocations.Clear();
        skillRowsContainer.pickingMode = PickingMode.Ignore;

        foreach (var weaponConfig in activeWeaponCategories)
        {
            tempAllocations[weaponConfig.weaponName] = 0;

            VisualElement rowInstance = weaponRowTemplate.CloneTree();
            rowInstance.pickingMode = PickingMode.Ignore;

            Label nameLabel = rowInstance.Q<Label>("WeaponNameLabel");
            VisualElement iconContainer = rowInstance.Q<VisualElement>("WeaponIcon");

            Label pointsLabel = rowInstance.Q<Label>("WeaponPointsLabel");
            Button addBtn = rowInstance.Q<Button>("AddPointsBTN");
            Button removeBtn = rowInstance.Q<Button>("RemovePointsBTN");
            Label additionLabel = rowInstance.Q<Label>("WapponSkillAddition");

            if (nameLabel != null) nameLabel.text = weaponConfig.weaponName;
            if (iconContainer != null && weaponConfig.weaponIcon != null) iconContainer.style.backgroundImage = new StyleBackground(weaponConfig.weaponIcon);

            if (addBtn != null) addBtn.pickingMode = PickingMode.Position;
            if (removeBtn != null) removeBtn.pickingMode = PickingMode.Position;

            var rowData = new WeaponSkillRowInstance
            {
                config = weaponConfig,
                pointsLabel = pointsLabel
            };

            // Display details on hover
            rowInstance.RegisterCallback<PointerEnterEvent>(evt =>
            {
                int allocated = tempAllocations.TryGetValue(weaponConfig.weaponName, out int val) ? val : 0;
                DisplayWeaponDetails(weaponConfig, player.GetPointsForWeapon(weaponConfig.weaponName) + allocated);
            });

            if (addBtn != null)
            {
                addBtn.clicked += () =>
                {
                    string weaponKey = rowData.config.weaponName;
                    int activeTotal = player.GetPointsForWeapon(weaponKey) + tempAllocations[weaponKey];
                    if (tempUnspent > 0 && activeTotal < 100)
                    {
                        tempAllocations[weaponKey]++;
                        tempUnspent--;
                        UpdateSkillLabels(player);
                        DisplayWeaponDetails(rowData.config, activeTotal + 1);
                    }
                };
            }

            if (removeBtn != null)
            {
                removeBtn.clicked += () =>
                {
                    string weaponKey = rowData.config.weaponName;
                    if (tempAllocations[weaponKey] > 0)
                    {
                        tempAllocations[weaponKey]--;
                        tempUnspent++;
                        UpdateSkillLabels(player);
                        DisplayWeaponDetails(rowData.config, player.GetPointsForWeapon(weaponKey) + tempAllocations[weaponKey]);
                    }
                };
            }

            spawnedRows.Add(rowData);
            skillRowsContainer.Add(rowInstance);
        }
    }

    public void OpenSkillDistribution(PlayerRuntimeState player, System.Action onComplete)
    {
        onDistributionComplete = onComplete;
        tempUnspent = player.unspentSkillPoints;

        tempAllocations.Clear();
        foreach (var row in spawnedRows)
        {
            tempAllocations[row.config.weaponName] = 0;
        }

        UpdateSkillLabels(player);
        skillDistroWindow.style.display = DisplayStyle.Flex;

        // Find your 'Confirm' or 'Close' button inside the window and bind it directly to mouse click instead
        Button confirmBtn = skillDistroWindow.Q<Button>("ConfirmSkillsBTN"); // Adjust name to your actual close button element
        if (confirmBtn != null)
        {
            confirmBtn.clicked += CloseDistributionWindow;
        }
    }

    private void CloseDistributionWindow()
    {
        // Unbind click to prevent double assignments on subsequent battles
        Button confirmBtn = skillDistroWindow.Q<Button>("ConfirmSkillsBTN");
        if (confirmBtn != null) confirmBtn.clicked -= CloseDistributionWindow;

        skillDistroWindow.style.display = DisplayStyle.None;
        onDistributionComplete?.Invoke();
    }

    private void UpdateSkillLabels(PlayerRuntimeState player)
    {
        if (unspentPointsLabel == null) return;
        unspentPointsLabel.text = $"Skill Points Left: {tempUnspent}";

        for (int i = 0; i < spawnedRows.Count; i++)
        {
            var rowData = spawnedRows[i];
            VisualElement visualRow = skillRowsContainer[i];

            Label additionLabel = visualRow.Q<Label>("WapponSkillAddition");
            Label pointsLabel = visualRow.Q<Label>("WeaponPointsLabel");

            int basePoints = player.GetPointsForWeapon(rowData.config.weaponName);
            int addedPoints = tempAllocations[rowData.config.weaponName];

            if (additionLabel != null)
                additionLabel.text = addedPoints > 0 ? $"+{addedPoints}" : "0";

            if (pointsLabel != null)
                pointsLabel.text = $"{basePoints + addedPoints} / 100";
        }
    }

    private void DisplayWeaponDetails(WeaponCategorySO config, int currentTotalPoints)
    {
        if (detailTitleLabel == null || detailDescLabel == null) return;

        detailTitleLabel.text = config.weaponName;

        string infoText = $"{config.categoryDescription}\n\nProgression unlocks:\n";
        foreach (var node in config.skillNodes)
        {
            string status = currentTotalPoints >= node.pointsRequired ? "[UNLOCKED]" : $"[{node.pointsRequired} Pts]";
            infoText += $"{status} {node.skillName}: {node.description}\n";
        }

        detailDescLabel.text = infoText;
    }

    public void ApplyAllocatedPoints(PlayerRuntimeState player)
    {
        player.unspentSkillPoints = tempUnspent;

        foreach (var row in spawnedRows)
        {
            int finalValue = player.GetPointsForWeapon(row.config.weaponName) + tempAllocations[row.config.weaponName];
            player.SetPointsForWeapon(row.config.weaponName, finalValue);
        }

        player.RecalculateWeaponSkillBonuses(activeWeaponCategories);
    }
}