using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPosManager : MonoBehaviour
{
    public GameObject playerFighter;
    public GameObject playerRBFighter;

    public GameObject enemyFighter;
    public GameObject enemyRBFighter;

    public Transform LeftSpawnPos;
    public Transform RightSpawnPos;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Character Index is {GameOptions.Instance.CharacterIndex}");
        //If 0 default left side start
        if (GameOptions.Instance.CharacterIndex == false)
        {
            playerFighter.transform.position = LeftSpawnPos.position;
            playerRBFighter.transform.position = LeftSpawnPos.position;

            enemyFighter.transform.position = RightSpawnPos.position;
            enemyRBFighter.transform.position = RightSpawnPos.position;

        }
        else
        {
            enemyFighter.transform.position = LeftSpawnPos.position;
            enemyRBFighter.transform.position = LeftSpawnPos.position;


            playerFighter.transform.position = RightSpawnPos.position;
            playerRBFighter.transform.position = RightSpawnPos.position;

        }


    }

   
}
