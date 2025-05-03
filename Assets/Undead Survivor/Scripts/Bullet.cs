using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody2D rigid;

    int leftPenetration;
    float speed;
    float range;
    Vector2 startPos;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir)
    {
        leftPenetration = StatManager.instance.attakStats.projectileCount;
        speed = StatManager.instance.attakStats.projectileSpeed;
        range = StatManager.instance.attakStats.attackRange;

        startPos = rigid.position;

        rigid.linearVelocity = dir.normalized * speed;
    }

    private void FixedUpdate()
    {
        if (Vector2.Distance(startPos, rigid.position) > range)
        {
            rigid.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Enemy")) return;

        leftPenetration--;
        if(leftPenetration == 0)
        {
            rigid.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }
}
