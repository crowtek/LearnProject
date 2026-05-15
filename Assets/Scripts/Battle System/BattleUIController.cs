using log4net.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleUIController : MonoBehaviour
{
    [SerializeField] private UIDocument battleHUD;
    [SerializeField] private TypewriterHandler typewriter;

    // UI Elements
    private VisualElement root;
    private VisualElement container, playerPortre, playerSprite, enemySprite;
    private VisualElement hpBarFill, textboxContainer;
    private Label playerHPLabel, levelLabel, resultText, enemyDamage, playerDamage;
    private Button attackBtn, defButton, closeBtn;

    private Coroutine damageTextCoroutine;

    public void Initialize()
    {
        root = battleHUD.rootVisualElement;
        container = root.Q<VisualElement>("container");

        // Querying elements - Using your exact UXML strings
        playerPortre = root.Q<VisualElement>("PlayerImage");
        playerSprite = root.Q<VisualElement>("PlayerSprite");
        enemySprite = root.Q<VisualElement>("EnemySprite");
        playerHPLabel = root.Q<Label>("PlayerHP");
        hpBarFill = root.Q<VisualElement>("HPBarFill");
        levelLabel = root.Q<Label>("level");
        textboxContainer = root.Q<VisualElement>("TextBoxContainer");
        resultText = root.Q<Label>("ResultText");
        enemyDamage = root.Q<Label>("EnemyDamage");
        playerDamage = root.Q<Label>("PlayerDamage");

        attackBtn = root.Q<Button>("AttackButton");
        defButton = root.Q<Button>("DefButton");
        closeBtn = root.Q<Button>("CloseButton");

        container.style.display = DisplayStyle.None;
    }

    public void SetActive(bool active)
    {
        container.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetupBattleImages(Sprite playerPort, Sprite playerBat, Sprite enemyBat, string playerName)
    {
        root.Q<Label>("PlayerName").text = playerName;
        playerPortre.style.backgroundImage = new StyleBackground(playerPort);
        playerSprite.style.backgroundImage = new StyleBackground(playerBat);
        enemySprite.style.backgroundImage = new StyleBackground(enemyBat);
    }

    public void UpdateStats(int currentHP, int maxHP, int level)
    {
        playerHPLabel.text = $"HP: {currentHP} / {maxHP}";
        levelLabel.text = $"Lv. {level}";
        enemyDamage.text = "";
        playerDamage.text = "";

        float hpRatio = (float)currentHP / maxHP;
        float hpPercent = Mathf.Clamp(hpRatio * 100f, 0, 100f);

        hpBarFill.style.width = new Length(hpPercent, LengthUnit.Percent);
    }

    public void ShowMonsterDamage(int damage)
    {
        enemyDamage.text = $"-{damage}";
        typewriter.RunText(enemyDamage, enemyDamage.text);
        damageTextCoroutine = StartCoroutine(HideDamageTextAfterDelay(enemyDamage, 1.0f));
    }

    public void ShowPlayerDamage(int damage)
    {
        playerDamage.text = $"-{damage}";
        typewriter.RunText(playerDamage, playerDamage.text);
        damageTextCoroutine = StartCoroutine(HideDamageTextAfterDelay(playerDamage, 1.0f));
    }

    private IEnumerator HideDamageTextAfterDelay(Label label,float delay)
    {
        yield return new WaitForSeconds(delay);
        label.text = "";
    }

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

    // Connect buttons to logic
    public void BindButtons(System.Action onAttack, System.Action onDefend, System.Action onEnd)
    {
        attackBtn.clicked += onAttack;
        defButton.clicked += onDefend;
        closeBtn.clicked += onEnd;
    }

    public void UnbindButtons(System.Action onAttack, System.Action onDefend, System.Action onEnd)
    {
        attackBtn.clicked -= onAttack;
        defButton.clicked -= onDefend;
        closeBtn.clicked -= onEnd;
    }
}