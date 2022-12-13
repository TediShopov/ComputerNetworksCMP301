using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverShow : MonoBehaviour
{
    [SerializeField]
    public TMPro.TMP_Text gameOverText;
    private void Awake()
    {
        gameOverText.text = "";
    }
    // Update is called once per frame
    void Update()
    {
        if (ClientData.Finished)
        {
            gameOverText.text = ClientData.GameOverMessage;
            StartCoroutine(LoadLevelAfterDelay(3));
        }   
        
    }

    IEnumerator LoadLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClientData.Pause = false;
        ClientData.Finished = false;
        ClientData.GameOverMessage = "";
        SceneManager.LoadScene(0);
    }


}
