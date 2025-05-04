using System;
using TMPro;
using UnityEngine;

public class StatUIManager : MonoBehaviour
{
    [Header("Stat Text References")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    
    [Header("Stat Name References")]
    [SerializeField] private TextMeshProUGUI attackName;
    [SerializeField] private TextMeshProUGUI defenseName;
    [SerializeField] private TextMeshProUGUI speedName;

    private void Awake()
    {
        // check if the stat texts are assigned
        if (attackName == null || defenseName == null || speedName == null)
        {
            Debug.LogError("Stat Texts are not assigned in the inspector.");
            return;
        }
        
        attackName.text = $"Attack Damage";
        defenseName.text = $"Defense";
        speedName.text = $"Move Speed";
    }

    // update UI texts with the current stats
    public void UpdateStatTexts(AttackStats attack, DefenseStats defense, UtilityStats utility)
    {
        if (attackText != null)
            attackText.text = $"{attack.attackDamage:F1}";

        if (defenseText != null)
            defenseText.text = $"{defense.defense:F1}";

        if (speedText != null)
            speedText.text = $"{utility.moveSpeed:F1}";
    }
}