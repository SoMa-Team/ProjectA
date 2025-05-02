using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;
    public PoolManager poolManager;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }
}
