using UnityEngine;

public class Player : Actor
{
    public Vector2 inputVec;

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mousePos - transform.position;
        inputVec = dir.normalized;
    }

    void FixedUpdate()
    {
        Vector2 nextVec = inputVec * statManager.utilityStats.moveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    private void LateUpdate()
    {
        animator.SetFloat("Speed", inputVec.magnitude); 
        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }
}
