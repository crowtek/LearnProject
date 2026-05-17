using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponSkillRowInstance
{
    public string weaponName;
    public Label pointsLabel;
    public System.Action onIncrement;

    public System.Func<int> GetCurrentPoints;
    public System.Action<int> SetCurrentPoints;
}

public class SkillDistroUIController : MonoBehaviour
{
    [Header("UI Templates & Containers")]
    [SerializeField] private VisualTreeAsset weaponRowTemplate;

    private VisualElement skillDistroWindow;
    private VisualElement skillRowsContainer;
    private Label unspentPointsLabel;

    private List<WeaponSkillRowInstance> spawnedRows = new List<WeaponSkillRowInstance>();
    private Dictionary<string, int> tempAllocations = new Dictionary<string, int>();
    private int tempUnspent;
    private System.Action onDistributionComplete;
    private VisualElement rootVisualElement;

    public void Initialize(VisualElement root)
    {
        rootVisualElement = root;
        skillDistroWindow = root.Q<VisualElement>("SkillDistroWindow");
        unspentPointsLabel = root.Q<Label>("UnspentPointsValue");
        skillRowsContainer = root.Q<VisualElement>("SkillRowsContainer");

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
        skillRowsContainer.pickingMode = PickingMode.Ignore;

        var categories = new[]
        {
            new { Name = "Sword", Get = (System.Func<int>)(() => player.swordPoints), Set = (System.Action<int>)(v => player.swordPoints = v) },
            new { Name = "Spear", Get = (System.Func<int>)(() => player.spearPoints), Set = (System.Action<int>)(v => player.spearPoints = v) },
            new { Name = "Boomerang", Get = (System.Func<int>)(() => player.boomerangPoints), Set = (System.Action<int>)(v => player.boomerangPoints = v) },
            new { Name = "Fisticuffs", Get = (System.Func<int>)(() => player.fisticuffsPoints), Set = (System.Action<int>)(v => player.fisticuffsPoints = v) }
        };

        foreach (var cat in categories)
        {
            VisualElement rowInstance = weaponRowTemplate.CloneTree();
            rowInstance.pickingMode = PickingMode.Ignore;

            Label nameLabel = rowInstance.Q<Label>("WeaponNameLabel");
            Label pointsLabel = rowInstance.Q<Label>("WeaponPointsLabel");
            Button addBtn = rowInstance.Q<Button>("AddPointsBTN");
            Button removeBtn = rowInstance.Q<Button>("RemovePointsBTN");

            if (nameLabel != null) nameLabel.text = cat.Name;
            if (addBtn != null) addBtn.pickingMode = PickingMode.Position;
            if (removeBtn != null) removeBtn.pickingMode = PickingMode.Position;

            var rowData = new WeaponSkillRowInstance
            {
                weaponName = cat.Name,
                pointsLabel = pointsLabel,
                GetCurrentPoints = cat.Get,
                SetCurrentPoints = cat.Set
            };

            if (addBtn != null)
            {
                addBtn.clicked += () =>
                {
                    if (tempUnspent > 0 && (rowData.GetCurrentPoints() + tempAllocations[rowData.weaponName]) < 100)
                    {
                        tempAllocations[rowData.weaponName]++;
                        tempUnspent--;
                        UpdateSkillLabels();
                    }
                };
            }

            if (removeBtn != null)
            {
                removeBtn.clicked += () =>
                {
                    if (tempAllocations[rowData.weaponName] > 0)
                    {
                        tempAllocations[rowData.weaponName]--;
                        tempUnspent++;
                        UpdateSkillLabels();
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
            tempAllocations[row.weaponName] = 0;
        }

        UpdateSkillLabels();
        skillDistroWindow.style.display = DisplayStyle.Flex;

        rootVisualElement.RegisterCallback<KeyDownEvent>(HandleSkillNavigation);
        rootVisualElement.Focus();
    }

    private void UpdateSkillLabels()
    {
        if (unspentPointsLabel == null) return;
        unspentPointsLabel.text = $"Skill Points Left: {tempUnspent}";

        for (int i = 0; i < spawnedRows.Count; i++)
        {
            var rowData = spawnedRows[i];
            VisualElement visualRow = skillRowsContainer[i];

            Label additionLabel = visualRow.Q<Label>("WapponSkillAddition");
            Label pointsLabel = visualRow.Q<Label>("WeaponPointsLabel");

            int basePoints = rowData.GetCurrentPoints();
            int addedPoints = tempAllocations[rowData.weaponName];

            if (additionLabel != null)
                additionLabel.text = addedPoints > 0 ? $"+{addedPoints}" : "0";

            if (pointsLabel != null)
                pointsLabel.text = $"{basePoints + addedPoints} / 100";
        }
    }

    private void HandleSkillNavigation(KeyDownEvent evt)
    {
        if (tempUnspent > 0)
        {
            int selectionIndex = -1;
            if (evt.keyCode == KeyCode.Alpha1) selectionIndex = 0;
            if (evt.keyCode == KeyCode.Alpha2) selectionIndex = 1;
            if (evt.keyCode == KeyCode.Alpha3) selectionIndex = 2;
            if (evt.keyCode == KeyCode.Alpha4) selectionIndex = 3;

            if (selectionIndex >= 0 && selectionIndex < spawnedRows.Count)
            {
                var targetRow = spawnedRows[selectionIndex];
                if ((targetRow.GetCurrentPoints() + tempAllocations[targetRow.weaponName]) < 100)
                {
                    tempAllocations[targetRow.weaponName]++;
                    tempUnspent--;
                    UpdateSkillLabels();
                }
            }
        }

        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            rootVisualElement.UnregisterCallback<KeyDownEvent>(HandleSkillNavigation);
            skillDistroWindow.style.display = DisplayStyle.None;
            onDistributionComplete?.Invoke();
        }
    }

    public void ApplyAllocatedPoints(PlayerRuntimeState player)
    {
        player.unspentSkillPoints = tempUnspent;
        foreach (var row in spawnedRows)
        {
            int finalValue = row.GetCurrentPoints() + tempAllocations[row.weaponName];
            row.SetCurrentPoints(finalValue);
        }
    }
}