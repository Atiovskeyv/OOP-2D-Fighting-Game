using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public Transform player1Spawn;
    public Transform player2Spawn;

    void Start()
    {

        Debug.Log("P1 ID: " + GameManager.instance.player1Character);
        Debug.Log("P2 ID: " + GameManager.instance.player2Character);
        Debug.Log("Array Size: " + GameManager.instance.characterPrefabs.Length);

        // PLAYER 1
        Instantiate(
            GameManager.instance.characterPrefabs[
                GameManager.instance.player1Character
            ],
            player1Spawn.position,
            Quaternion.identity
        );

        // PLAYER 2
        Instantiate(
            GameManager.instance.characterPrefabs[
                GameManager.instance.player2Character
            ],
            player2Spawn.position,
            Quaternion.identity
        );
    }
}
