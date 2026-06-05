using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public string selectedMap = "Map1";
    public int player1Character = -1;
    public int player2Character = -1;

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
