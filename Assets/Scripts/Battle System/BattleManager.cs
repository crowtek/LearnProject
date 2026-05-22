using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public enum BattleState { Idle, Start, PlayerTurn, EnemyTurn, Busy, Won, Lost, End }

    [Header("UI Ref")]
    [SerializeField] private BattleUIController uiController;
    private Camera overworldCamera;

    [Header("Bridge Data")]
    [SerializeField] private BattleTransferDataSO battleTransferData;

    [Header("3D Spawn Points in the Arena")]
    [SerializeField] private Transform player3DSpawnPoint;
    [SerializeField] private Transform enemy3DSpawnPoint;

    [Header("Player Ref")]
    [SerializeField] private PlayerRuntimeState playerRuntime;

    [Header("Broadcasting Channels")]
    [SerializeField] private BattleResultEventChannelSO battleResultEventChannel;
    [SerializeField] private BoolEventChannelSO battleStateEventChannel;

    [Header("Audio Channels")]
    [SerializeField] private AudioEventChannelSO musicChannel;
    [SerializeField] private AudioEventChannelSO sfxChannel;

    [Header("Audio files to play")]
    [SerializeField] private AudioConfigurationSO battleStartSFX;
    [SerializeField] private AudioConfigurationSO battleMusicBGM;
    [SerializeField] private AudioConfigurationSO attackSFX;
    [SerializeField] private AudioConfigurationSO skillSFX;
    [SerializeField] private AudioConfigurationSO mapBGM;
    [SerializeField] private AudioConfigurationSO battleWonBGM;

    // Runtime state
    private BattleEntityData currentEnemy;
    private int currentPlayerHP;
    private int currentPlayerMP;
    private int currentEnemyHP;
    private int accumulatedEXP;
    private BattleState currentState = BattleState.Idle;

    private GameObject spawnedPlayerGO;
    private GameObject spawnedEnemyGO;

    // Cached animation helpers (added at runtime via GetComponent after Spawn)
    private CombatAnimator playerCombatAnimator;
    private CombatAnimator enemyCombatAnimator;

    void Awake()
    {
        uiController.Initialize();
        if (uiController.SkillDistro != null)
            uiController.SkillDistro.SetupDynamicWeaponRows(playerRuntime);
    }

    void Start()
    {
        overworldCamera = Camera.main;
        if (overworldCamera != null)
            overworldCamera.gameObject.SetActive(false);

        if (battleTransferData == null || battleTransferData.currentEnemyData == null)
        {
            Debug.LogError("Keine Kampfdaten für die 3D-Szene gefunden!");
            return;
        }

        currentEnemy = battleTransferData.currentEnemyData;
        currentPlayerHP = playerRuntime.currentHP;
        currentPlayerMP = playerRuntime.currentMP;
        currentEnemyHP = currentEnemy.maxHP;
        accumulatedEXP = currentEnemy.expReward;

        if (sfxChannel != null) sfxChannel.RaiseEvent(battleStartSFX);
        if (musicChannel != null) musicChannel.RaiseEvent(battleMusicBGM);

        Spawn3DCombatants();

        uiController.ShowBattleUI(true);
        uiController.BindButtons(PlayerAttack, PlayerDefend, () => EndBattle(false));
        uiController.BindSkillButtons(playerRuntime.knownSkills, OnPlayerSelectSkill);

        UpdateHUD();
        currentState = BattleState.PlayerTurn;
    }


    private void Spawn3DCombatants()
    {
        if (playerRuntime.playerCombatPrefab != null && player3DSpawnPoint != null)
        {
            spawnedPlayerGO = Instantiate(playerRuntime.playerCombatPrefab,
                player3DSpawnPoint.position, player3DSpawnPoint.rotation);
            playerCombatAnimator = spawnedPlayerGO.GetComponent<CombatAnimator>();
            if (playerCombatAnimator == null)
                playerCombatAnimator = spawnedPlayerGO.AddComponent<CombatAnimator>();
        }
        else Debug.LogWarning("Player Combat Prefab oder Spawn Point nicht zugewiesen!");

        if (currentEnemy.combatPrefab != null && enemy3DSpawnPoint != null)
        {
            spawnedEnemyGO = Instantiate(currentEnemy.combatPrefab,
                enemy3DSpawnPoint.position, enemy3DSpawnPoint.rotation);
            enemyCombatAnimator = spawnedEnemyGO.GetComponent<CombatAnimator>();
            if (enemyCombatAnimator == null)
                enemyCombatAnimator = spawnedEnemyGO.AddComponent<CombatAnimator>();
        }
        else Debug.LogWarning("Enemy Combat Prefab oder Spawn Point nicht zugewiesen!");
    }

    private void UpdateHUD()
    {
        uiController.UpdateStats(
            currentPlayerHP, playerRuntime.maxHP,
            currentPlayerMP, playerRuntime.maxMP,
            playerRuntime.currentLevel);

        float enemyHPRatio = (float)currentEnemyHP / currentEnemy.maxHP;
        uiController.UpdateEnemyHP(currentEnemy.entityName, currentEnemyHP, currentEnemy.maxHP);
    }

    // ── Player Actions ───────────────────────────────────────────────────────

    public void PlayerAttack()
    {
        if (currentState != BattleState.PlayerTurn) return;
        currentState = BattleState.Busy;

        if (sfxChannel != null) sfxChannel.RaiseEvent(attackSFX);

        // Player swings — wait for animation, then apply damage
        playerCombatAnimator?.PlayAndThen(playerRuntime.normalAttackTrigger, 0.8f, () =>
        {
            int damage = Mathf.Max(1, playerRuntime.attack - (currentEnemy.defense / 2));
            currentEnemyHP -= damage;

            enemyCombatAnimator?.Play(currentEnemy.hurtTrigger);
            uiController.ShowMonsterDamage(damage);
            UpdateHUD();

            if (currentEnemyHP <= 0)
                ResolveEnemyDeath();
            else
                ScheduleEnemyTurn();
        });
    }

    public void PlayerDefend()
    {
        if (currentState != BattleState.PlayerTurn) return;
        currentState = BattleState.Busy;

        uiController.ShowBattleLog("Verteidigung!");
        // TODO: apply temporary defense buff this round
        ScheduleEnemyTurn();
    }

    private void OnPlayerSelectSkill(BattleSkillData skill)
    {
        if (currentState != BattleState.PlayerTurn) return;
        if (currentPlayerMP < skill.mpCost)
        {
            uiController.ShowBattleLog("Nicht genug MP!");
            return;
        }

        currentState = BattleState.Busy;
        currentPlayerMP -= skill.mpCost;
        UpdateHUD();

        if (sfxChannel != null && skillSFX != null) sfxChannel.RaiseEvent(skillSFX);

        string triggerToPlay = !string.IsNullOrEmpty(skill.attackerAnimationTrigger)
            ? skill.attackerAnimationTrigger
            : playerRuntime.normalAttackTrigger;

        playerCombatAnimator?.PlayAndThen(triggerToPlay, 1.0f, () =>
        {
            ApplySkillEffect(skill, isPlayerCasting: true);
        });
    }

    private void ApplySkillEffect(BattleSkillData skill, bool isPlayerCasting)
    {
        switch (skill.effectType)
        {
            case SkillEffectType.Damage:
                {
                    int power = isPlayerCasting
                        ? Mathf.Max(1, Mathf.RoundToInt(playerRuntime.attack * skill.attackMultiplier) + skill.basePower - currentEnemy.defense)
                        : Mathf.Max(1, Mathf.RoundToInt(currentEnemy.attack * skill.attackMultiplier) + skill.basePower - playerRuntime.defense);

                    if (isPlayerCasting)
                    {
                        currentEnemyHP = Mathf.Max(0, currentEnemyHP - power);
                        enemyCombatAnimator?.Play(currentEnemy.hurtTrigger);
                        uiController.ShowMonsterDamage(power);
                        uiController.ShowBattleLog($"{skill.skillName}! {power} Schaden!");
                    }
                    else
                    {
                        currentPlayerHP = Mathf.Max(0, currentPlayerHP - power);
                        playerCombatAnimator?.Play(playerRuntime.hurtTrigger);
                        uiController.ShowPlayerDamage(power);
                        uiController.ShowBattleLog($"{currentEnemy.entityName} benutzt {skill.skillName}! {power} Schaden!");
                    }
                    break;
                }

            case SkillEffectType.Heal:
                {
                    if (isPlayerCasting)
                    {
                        int healed = Mathf.Min(skill.basePower, playerRuntime.maxHP - currentPlayerHP);
                        currentPlayerHP += healed;
                        uiController.ShowBattleLog($"{skill.skillName}! +{healed} HP!");
                    }
                    break;
                }
        }

        UpdateHUD();

        if (isPlayerCasting)
        {
            if (currentEnemyHP <= 0) ResolveEnemyDeath();
            else ScheduleEnemyTurn();
        }
        else
        {
            if (currentPlayerHP <= 0) DetermineBattleOutcome();
            else currentState = BattleState.PlayerTurn;
        }
    }

    // ── Enemy Turn ───────────────────────────────────────────────────────────

    private void ScheduleEnemyTurn()
    {
        Invoke(nameof(EnemyTurn), 0.5f);
    }

    private void EnemyTurn()
    {
        currentState = BattleState.EnemyTurn;

        // Try to pick a skill; fall back to normal attack
        BattleSkillData chosenSkill = PickEnemySkill();

        if (chosenSkill != null)
        {
            string triggerName = !string.IsNullOrEmpty(chosenSkill.attackerAnimationTrigger)
                ? chosenSkill.attackerAnimationTrigger
                : currentEnemy.normalAttackTrigger;

            uiController.ShowBattleLog($"{currentEnemy.entityName} setzt {chosenSkill.skillName} ein!");
            enemyCombatAnimator?.PlayAndThen(triggerName, 1.0f, () =>
            {
                ApplySkillEffect(chosenSkill, isPlayerCasting: false);
            });
        }
        else
        {
            // Normal attack
            enemyCombatAnimator?.PlayAndThen(currentEnemy.normalAttackTrigger, 0.8f, () =>
            {
                int damage = Mathf.Max(1, currentEnemy.attack - (playerRuntime.defense / 2));
                currentPlayerHP = Mathf.Max(0, currentPlayerHP - damage);

                playerCombatAnimator?.Play(playerRuntime.hurtTrigger);
                uiController.ShowPlayerDamage(damage);
                uiController.ShowBattleLog($"{currentEnemy.entityName} greift an! {damage} Schaden!");

                UpdateHUD();

                if (currentPlayerHP <= 0) DetermineBattleOutcome();
                else currentState = BattleState.PlayerTurn;
            });
        }
    }

    private BattleSkillData PickEnemySkill()
    {
        if (currentEnemy.availableSkills == null || currentEnemy.availableSkills.Length == 0)
            return null;

        // Shuffle + probability check — first skill whose dice rolls passes wins
        var skills = currentEnemy.availableSkills;
        int start = Random.Range(0, skills.Length);
        for (int i = 0; i < skills.Length; i++)
        {
            var skill = skills[(start + i) % skills.Length];
            if (Random.value < skill.aiUseProbability)
                return skill;
        }
        return null;
    }

    // ── Outcome ──────────────────────────────────────────────────────────────

    private void ResolveEnemyDeath()
    {
        enemyCombatAnimator?.Play(currentEnemy.deathTrigger);
        DetermineBattleOutcome();
    }

    private void DetermineBattleOutcome()
    {
        if (currentEnemyHP <= 0)
        {
            if (musicChannel != null) musicChannel.RaiseEvent(battleWonBGM);
            currentState = BattleState.Won;

            string msg = $"{currentEnemy.entityName} wurde besiegt!\r\nDu erhältst {accumulatedEXP} EXP.";

            bool willLevelUp = (playerRuntime.currentEXP + accumulatedEXP) >= playerRuntime.expToNextLevel;
            if (willLevelUp)
            {
                msg += "\r\n\r\nLEVEL UP!";
                uiController.ShowVictoryScreen(msg, EvaluatePostBattleSkillFlow);
            }
            else
            {
                uiController.ShowVictoryScreen(msg, () => EndBattle(true));
            }
        }
        else if (currentPlayerHP <= 0)
        {
            playerCombatAnimator?.Play(playerRuntime.deathTrigger);
            currentState = BattleState.Lost;
            uiController.ShowVictoryScreen("Du wurdest kampfunfähig...", () => EndBattle(false));
        }
    }

    private void EvaluatePostBattleSkillFlow()
    {
        if (playerRuntime.unspentSkillPoints > 0)
        {
            uiController.SkillDistro.OpenSkillDistribution(playerRuntime, () =>
            {
                uiController.SkillDistro.ApplyAllocatedPoints(playerRuntime);
                EndBattle(true);
            });
        }
        else
        {
            EndBattle(true);
        }
    }

    public void EndBattle(bool isVictory)
    {
        uiController.UnbindButtons(PlayerAttack, PlayerDefend, null);
        uiController.UnbindSkillButtons();
        uiController.ShowBattleUI(false);

        playerRuntime.currentHP = currentPlayerHP;
        playerRuntime.currentMP = currentPlayerMP;

        battleResultEventChannel.RaiseEvent(new BattleResult
        {
            earnedEXP = isVictory ? accumulatedEXP : 0,
            isVictory = isVictory
        });

        battleTransferData.Clear();
        battleStateEventChannel.RaiseEvent(true);
        if (musicChannel != null) musicChannel.RaiseEvent(mapBGM);

        if (spawnedPlayerGO != null) Destroy(spawnedPlayerGO);
        if (spawnedEnemyGO != null) Destroy(spawnedEnemyGO);

        if (overworldCamera != null)
            overworldCamera.gameObject.SetActive(true);

        SceneManager.UnloadSceneAsync("BattleScene");
    }
}