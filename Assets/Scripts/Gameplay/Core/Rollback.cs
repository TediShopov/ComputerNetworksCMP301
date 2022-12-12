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
        Debug.LogError($"Resimulating from{FrameLimiter.Instance.FramesInPlay} " +
            $" {fighter.InputBuffer.BufferedInput.Peek().TimeStamp} Frames");
        fighter.ResimulateInput(
            fighter.InputBuffer,
            frames);


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
                int count = playerRB.InputBuffer.BufferedInput.Count;
                for (int i = 0; i < count; i++)
                {
                    ResimulateFramesForFighter(playerRB,1);
                    ResimulateFramesForFighter(enemyRB,1);
                }
               

                ClientData.Pause = true;
            }
        }

          
            // }
        
    }

     
}
