using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public string selectedMap = "Map1";
    public int player1Character = -1;
    public int player2Character = -1;

    [Header("Character Prefabs")]
    [Tooltip("Sırasıyla: [0]=Wesker, [1]=Omniman, [2]=LaraCroft, [3]=SoldierBoy")]
    public GameObject[] characterPrefabs;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
