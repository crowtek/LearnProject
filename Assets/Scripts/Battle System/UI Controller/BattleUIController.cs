using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleUIController : MonoBehaviour
{
    [SerializeField] private UIDocument battleHUD;
    [SerializeField] private TypewriterHandler typewriter;
    [SerializeField] private SkillDistroUIController skillDistroUI;

    // ── Root & Structural Elements ───────────────────────────────────────────
    private VisualElement root;
    private VisualElement container;

    // ── Player Stats ─────────────────────────────────────────────────────────
    private VisualElement playerPortre;
    private VisualElement hpBarFill;
    private VisualElement mpBarFill;
    private Label playerHPLabel;
    private Label playerMPLabel;
    private Label levelLabel;

    // ── Enemy Stats ──────────────────────────────────────────────────────────
    private VisualElement enemyHpBarFill;
    private Label enemyNameLabel;
    private Label enemyHPLabel;

    // ── Textbox & Damage Labels ──────────────────────────────────────────────
    private VisualElement textboxContainer;
    private Label resultText;
    private Label enemyDamage;
    private Label playerDamage;
    private Label battleLogLabel;

    // ── Action Buttons ───────────────────────────────────────────────────────
    private Button attackBtn;
    private Button defButton;
    private Button closeBtn;
    private Button skillsMenuBtn;

    // ── Skill Panel ──────────────────────────────────────────────────────────
    private VisualElement skillMenuPanel;
    private VisualElement skillButtonsContainer;

    // ── Internal ─────────────────────────────────────────────────────────────
    private Coroutine enemyDamageCoroutine;
    private Coroutine playerDamageCoroutine;
    private Coroutine battleLogCoroutine;
    private System.Action<BattleSkillData> onSkillSelected;

    public SkillDistroUIController SkillDistro => skillDistroUI;

    // ── Init ─────────────────────────────────────────────────────────────────

    public void Initialize()
    {
        root = battleHUD.rootVisualElement;
        container = root.Q<VisualElement>("container");

        // Player
        playerPortre = root.Q<VisualElement>("PlayerImage");
        playerHPLabel = root.Q<Label>("PlayerHP");
        playerMPLabel = root.Q<Label>("PlayerMP");
        hpBarFill = root.Q<VisualElement>("HPBarFill");
        mpBarFill = root.Q<VisualElement>("MPBarFill");
        levelLabel = root.Q<Label>("level");

        // Enemy
        enemyHpBarFill = root.Q<VisualElement>("EnemyHPBarFill");
        enemyNameLabel = root.Q<Label>("EnemyName");
        enemyHPLabel = root.Q<Label>("EnemyHP");

        // Textbox
        textboxContainer = root.Q<VisualElement>("TextBoxContainer");
        resultText = root.Q<Label>("ResultText");
        enemyDamage = root.Q<Label>("EnemyDamage");
        playerDamage = root.Q<Label>("PlayerDamage");
        battleLogLabel = root.Q<Label>("BattleLog");

        // Buttons
        attackBtn = root.Q<Button>("AttackButton");
        defButton = root.Q<Button>("DefButton");
        closeBtn = root.Q<Button>("CloseButton");
        skillsMenuBtn = root.Q<Button>("SkillsButton");

        // Skill panel
        skillMenuPanel = root.Q<VisualElement>("SkillMenuPanel");
        skillButtonsContainer = root.Q<VisualElement>("SkillButtonsContainer");

        if (skillMenuPanel != null)
            skillMenuPanel.style.display = DisplayStyle.None;

        if (skillsMenuBtn != null)
            skillsMenuBtn.clicked += ToggleSkillPanel;

        if (skillDistroUI != null)
            skillDistroUI.Initialize(root);
    }

    // ── Visibility ───────────────────────────────────────────────────────────

    public void ShowBattleUI(bool active)
    {
        container.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ── Stats ────────────────────────────────────────────────────────────────

    public void UpdateStats(int currentHP, int maxHP, int currentMP, int maxMP, int level)
    {
        if (playerHPLabel != null)
            playerHPLabel.text = $"HP: {currentHP} / {maxHP}";
        if (playerMPLabel != null)
            playerMPLabel.text = $"MP: {currentMP} / {maxMP}";
        if (levelLabel != null)
            levelLabel.text = $"Lv. {level}";

        SetBarPercent(hpBarFill, currentHP, maxHP);
        SetBarPercent(mpBarFill, currentMP, maxMP);
    }

    public void UpdateEnemyHP(string enemyName, int currentHP, int maxHP)
    {
        if (enemyNameLabel != null) enemyNameLabel.text = enemyName;
        if (enemyHPLabel != null) enemyHPLabel.text = $"{currentHP} / {maxHP}";
        SetBarPercent(enemyHpBarFill, currentHP, maxHP);
    }

    private void SetBarPercent(VisualElement bar, int current, int max)
    {
        if (bar == null || max <= 0) return;
        float pct = Mathf.Clamp((float)current / max * 100f, 0f, 100f);
        bar.style.width = new Length(pct, LengthUnit.Percent);
    }

    public void SetupBattleImages(Sprite playerPort, string playerName)
    {
        root.Q<Label>("PlayerName").text = playerName;
        if (playerPortre != null && playerPort != null)
            playerPortre.style.backgroundImage = new StyleBackground(playerPort);
    }

    // ── Damage Text ──────────────────────────────────────────────────────────

    public void ShowMonsterDamage(int damage)
    {
        if (enemyDamage == null) return;
        enemyDamage.text = $"-{damage}";
        if (enemyDamageCoroutine != null) StopCoroutine(enemyDamageCoroutine);
        enemyDamageCoroutine = StartCoroutine(HideLabelAfterDelay(enemyDamage, 1.2f));
    }

    public void ShowPlayerDamage(int damage)
    {
        if (playerDamage == null) return;
        playerDamage.text = $"-{damage}";
        if (playerDamageCoroutine != null) StopCoroutine(playerDamageCoroutine);
        playerDamageCoroutine = StartCoroutine(HideLabelAfterDelay(playerDamage, 1.2f));
    }

    private IEnumerator HideLabelAfterDelay(Label label, float delay)
    {
        yield return new WaitForSeconds(delay);
        label.text = "";
    }

    // ── Battle Log ───────────────────────────────────────────────────────────

    public void ShowBattleLog(string message)
    {
        if (battleLogLabel == null) return;
        battleLogLabel.text = message;
        if (battleLogCoroutine != null) StopCoroutine(battleLogCoroutine);
        battleLogCoroutine = StartCoroutine(HideLabelAfterDelay(battleLogLabel, 2.5f));
    }

    // ── Victory Screen ───────────────────────────────────────────────────────

    public void ShowVictoryScreen(string message, System.Action onContinue)
    {
        textboxContainer.style.display = DisplayStyle.Flex;
        typewriter.RunText(resultText, message);

        textboxContainer.UnregisterCallback<PointerDownEvent, System.Action>(OnContainerClicked);
        textboxContainer.RegisterCallback<PointerDownEvent, System.Action>(OnContainerClicked, onContinue);
    }

    private void OnContainerClicked(PointerDownEvent evt, System.Action callback)
    {
        callback?.Invoke();
        textboxContainer.style.display = DisplayStyle.None;
    }

    // ── Action Buttons ───────────────────────────────────────────────────────

    public void BindButtons(System.Action onAttack, System.Action onDefend, System.Action onEnd)
    {
        if (attackBtn != null) attackBtn.clicked += onAttack;
        if (defButton != null) defButton.clicked += onDefend;
        if (closeBtn != null) closeBtn.clicked += onEnd;
    }

    public void UnbindButtons(System.Action onAttack, System.Action onDefend, System.Action onEnd)
    {
        if (attackBtn != null) attackBtn.clicked -= onAttack;
        if (defButton != null) defButton.clicked -= onDefend;
        if (closeBtn != null) closeBtn.clicked -= onEnd;
    }

    // ── Skill Panel ──────────────────────────────────────────────────────────

    public void BindSkillButtons(List<BattleSkillData> skills, System.Action<BattleSkillData> onSkillChosen)
    {
        onSkillSelected = onSkillChosen;
        RebuildSkillPanel(skills);
    }

    public void UnbindSkillButtons()
    {
        onSkillSelected = null;
        skillButtonsContainer?.Clear();
        if (skillMenuPanel != null)
            skillMenuPanel.style.display = DisplayStyle.None;
    }

    private void ToggleSkillPanel()
    {
        if (skillMenuPanel == null) return;
        bool isVisible = skillMenuPanel.style.display == DisplayStyle.Flex;
        skillMenuPanel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void RebuildSkillPanel(List<BattleSkillData> skills)
    {
        if (skillButtonsContainer == null) return;
        skillButtonsContainer.Clear();

        if (skills == null || skills.Count == 0)
        {
            if (skillsMenuBtn != null) skillsMenuBtn.SetEnabled(false);
            return;
        }

        if (skillsMenuBtn != null) skillsMenuBtn.SetEnabled(true);

        foreach (var skill in skills)
        {
            var btn = new Button();
            btn.text = $"{skill.skillName}  ({skill.mpCost} MP)";
            btn.AddToClassList("skill-button");
            btn.tooltip = skill.description;

            // Capture local reference for the lambda
            var capturedSkill = skill;
            btn.clicked += () =>
            {
                if (skillMenuPanel != null)
                    skillMenuPanel.style.display = DisplayStyle.None;
                onSkillSelected?.Invoke(capturedSkill);
            };

            skillButtonsContainer.Add(btn);
        }
    }
}