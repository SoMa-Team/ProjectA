using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float healthPoint;
    public float defense;
    public Rigidbody2D target;
    public RuntimeAnimatorController[] controllers;

    bool isLive = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator animator;
    WaitForFixedUpdate wait;
    
    public float knockbackSize = 3;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        wait = new WaitForFixedUpdate();
    }

    private void FixedUpdate()
    {
        if (!isLive || animator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
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
        StartCoroutine(KnockBack());

        if(healthPoint > 0)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            Dead();
        }
    }

    IEnumerator KnockBack()
    {
        yield return wait;
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * knockbackSize * 3, ForceMode2D.Impulse);
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}
