using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rollback : MonoBehaviour
{
    public FighterController playerRB;
    public FighterController enemyRB;

    public bool RollackActive = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    void ResimulateFramesForFighter(FighterController fighter,int frames) 
    {
        Debug.LogError($"Resimulating {frames} Frames");
        fighter.ResimulateInput(
            fighter.InputBuffer,
            frames);


        Debug.LogError($"Cleared Buffer");
        for (int i = 0; i < frames; i++)
        {
            fighter.InputBuffer.BufferedInput.Dequeue();
        }

    }


    // Update is called once per frame
    void Update()
    {
            if (!ClientData.Pause)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                ResimulateFramesForFighter(playerRB,7);
                ResimulateFramesForFighter(enemyRB,7);

                ClientData.Pause = true;



            }
        }

            if (Input.GetKeyDown(KeyCode.P))
            {
                ClientData.Pause = false;

            }
            // }
        
    }
}
