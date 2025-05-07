using UnityEngine;
using UnityEngine.UI; 

[System.Serializable]
public class AttackStats
{
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;

    public int projectileCount;
    public int projectilePierce;

    public float criticalChance;
    public float criticalDamage;

    public float lifesteal;

    public float bossDamageBonus;
    public float armorPenetration;
}


[System.Serializable]
public class DefenseStats
{
    public float defense;
    public float reflectDamage;
}

[System.Serializable]
public class VitalStats
{
    public float maxHealth;
    public float maxMana;

    public float healthRegen;
    public float manaRegen;
}

[System.Serializable]
public class UtilityStats
{
    public float moveSpeed;
    public float magnetRange;

    public float rewardQuantityBonus;
    public float rewardQualityBonus;

    public float skillCooldownReduction;

    public float spawnQuantityMultiplier;
    public float spawnQualityMultiplier;
}

public class StatManager : MonoBehaviour
{
    // Base Stat Variables
    private AttackStats baseAttackStats;
    private DefenseStats baseDefenseStats;
    private UtilityStats baseUtilityStats;

    // diff Stat Variables
    public AttackStats attackStats;
    public DefenseStats defenseStats;
    public UtilityStats utilityStats;

    [SerializeField] private StatUIManager uiManager;
    [SerializeField] private CardManager cardManager;

    public static StatManager Instance;

    private void Awake()
    {
        Instance = this;

        // BaseStat Deep Copy
        baseAttackStats = CloneAttackStats(attackStats);
        baseDefenseStats = CloneDefenseStats(defenseStats);
        baseUtilityStats = CloneUtilityStats(utilityStats);
    }

    private void Start()
    {
        // add on click listener to calculate button
        Button calculateButton = GetComponentInChildren<Button>();
        if (calculateButton != null)
        {
            calculateButton.onClick.RemoveAllListeners(); // 중복 방지
            calculateButton.onClick.AddListener(CalculateStats);
        }
        else
        {
            Debug.LogError("Calculate Button not found!");
        }
    }

    public void ApplyCardEffects(CardData card)
    {
        // apply each card effect to the stats
        attackStats.attackDamage += card.attackDamageChange;
        defenseStats.defense += card.defenseChange;
        utilityStats.moveSpeed += card.moveSpeedChange;

        uiManager.UpdateStatTexts(attackStats, defenseStats, utilityStats);
    }

    public void CalculateStats()
    {
        // deep copy base stats
        attackStats = CloneAttackStats(baseAttackStats);
        defenseStats = CloneDefenseStats(baseDefenseStats);
        utilityStats = CloneUtilityStats(baseUtilityStats);

        //  apply card effects
        cardManager.CalculateStats();

        // UI update
        uiManager.UpdateStatTexts(attackStats, defenseStats, utilityStats);

        Debug.Log("Calculate button clicked!");
    }
    
    private AttackStats CloneAttackStats(AttackStats original)
    {
        return new AttackStats
        {
            attackDamage = original.attackDamage,
            attackSpeed = original.attackSpeed,
            attackRange = original.attackRange,
            projectileCount = original.projectileCount,
            projectilePierce = original.projectilePierce,
            criticalChance = original.criticalChance,
            criticalDamage = original.criticalDamage,
            lifesteal = original.lifesteal,
            bossDamageBonus = original.bossDamageBonus,
            armorPenetration = original.armorPenetration
        };
    }

    private DefenseStats CloneDefenseStats(DefenseStats original)
    {
        return new DefenseStats
        {
            defense = original.defense,
            reflectDamage = original.reflectDamage
        };
    }

    private UtilityStats CloneUtilityStats(UtilityStats original)
    {
        return new UtilityStats
        {
            moveSpeed = original.moveSpeed,
            magnetRange = original.magnetRange,
            rewardQuantityBonus = original.rewardQuantityBonus,
            rewardQualityBonus = original.rewardQualityBonus,
            skillCooldownReduction = original.skillCooldownReduction,
            spawnQuantityMultiplier = original.spawnQuantityMultiplier,
            spawnQualityMultiplier = original.spawnQualityMultiplier
        };
    }
}