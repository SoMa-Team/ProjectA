using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float healthPoint;
    public float defense;
    public Rigidbody2D target;

    bool isLive = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (!isLive)
        {
            return;
        }

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position+nextVec);
        rigid.linearVelocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (!isLive)
        {
            return;
        }

        spriter.flipX = target.position.x < rigid.position.x;
    }

    private void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet"))
        {
            return;
        }
        TakeDamage(StatManager.Instance.attakStats.attackDamage, StatManager.Instance.attakStats.armorPenetration);
    }

    private void TakeDamage(float attackDamage, float armorPenetration)
    {
        float effectiveDefense = Mathf.Max(0, defense * (100 - armorPenetration) / 100);
        float damage = attackDamage * 100 / (100 + effectiveDefense);
        healthPoint -= damage;

        if(healthPoint > 0)
        {

        }
        else
        {
            Dead();
        }
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}
