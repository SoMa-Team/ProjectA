using UnityEngine;

// ScriptableObject 또는 일반 C# 클래스로 정의 가능
// ScriptableObject가 에셋 관리 측면에서 편리할 수 있음
[CreateAssetMenu(fileName = "New CardData", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject // 또는 그냥 public class CardData
{
    public int id; // 고유 식별자
    public string cardName;
    [TextArea] public string description;
    public Sprite cardSprite;
    // public CardStats stats; // 카드 능력치 클래스 (필요시 정의)
    // public CardEffect effect; // 카드 효과 로직 (필요시 정의)
    
    public int attackPower;
    public int defensePower;
    
    // Stat diff variables
    public float attackDamageChange;
    public float defenseChange;
    public float moveSpeedChange;

    // 필요에 따라 능력치, 설명 등을 위한 별도 클래스 정의 가능
    // [System.Serializable]
    // public class CardStats { ... }
}