using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public int prefabId;

    float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > 1 / StatManager.Instance.attakStats.attackSpeed)
        {
            timer = 0f;
            Fire();
        }
    }
    void Fire()
    {
        Transform bullet  = GameManager.instance.poolManager.Get(prefabId).transform;
        bullet.position = transform.position; 
    }
}
