using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody2D rigid;

    int leftPenetration;
    float speed;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir)
    {
        leftPenetration = StatManager.Instance.attakStats.projectileCount;
        speed = StatManager.Instance.attakStats.projectileSpeed;

        rigid.linearVelocity = dir.normalized * speed;
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
