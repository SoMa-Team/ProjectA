using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public int prefabId;

    float timer;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

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
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3 fireDir = mouseWorldPos - transform.position;

        GameObject bulletObj = GameManager.instance.poolManager.Get(prefabId);
        bulletObj.transform.position = transform.position;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Init(fireDir);
    }
}
