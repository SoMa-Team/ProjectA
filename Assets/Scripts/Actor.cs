using UnityEngine;

public class Actor : MonoBehaviour
{
    [Header("Stats")]
    public StatManager statManager;
    protected float currentHealth;

    protected Animator animator;
    protected Rigidbody2D rigid;
    protected Collider2D coll;
    protected SpriteRenderer spriter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        spriter = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        currentHealth = statManager.vitalStats.maxHealth;
    }
    public virtual void TakeDamage(float rawDamage, float armorPenetration)
    {
        float defense = statManager.defenseStats.defense;
        float effectiveDefense = Mathf.Max(0, defense * (100 - armorPenetration) / 100);
        float damage = rawDamage * 100 / (100 + effectiveDefense);
        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
        else
            OnHit();
    }

    protected virtual void OnHit()
    {
        animator?.SetTrigger("Hit");
    }

    protected virtual void Die()
    {
        currentHealth = 0;
        coll.enabled = false;
        rigid.simulated = false;
        animator?.SetBool("Dead", true);
        gameObject.SetActive(false);
    }
}
