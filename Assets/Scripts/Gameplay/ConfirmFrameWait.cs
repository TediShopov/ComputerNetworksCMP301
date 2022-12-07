using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ConfirmFrameWait : MonoBehaviour
{

    private static bool WaitForConfirmFrame=false;


    static void OnBufferUpdate(InputFrame frame) 
    {
        //Buffer Updating
        WaitForConfirmFrame= FrameLimiter.Instance.FramesInPlay >
               GetConfirmFrame(StaticBuffers.Instance.PlayerBuffer,
                               StaticBuffers.Instance.EnemyBuffer);
        Debug.LogError($"Wait Condition Updated {WaitForConfirmFrame} Confirm Frame " +
            $"{GetConfirmFrame(StaticBuffers.Instance.PlayerBuffer,StaticBuffers.Instance.EnemyBuffer)}");
       
    }

    static int GetConfirmFrame(InputBuffer b, InputBuffer b2)
    {
        if (b==null || b2==null)
        {
            return -1;
        }
        if (b2.BufferedInput == null || b2.BufferedInput.Count == 0
            || b.BufferedInput == null || b.BufferedInput.Count == 0)
        {
            return -1;
        }

        //Have to be algigned perfectly 
        if (b.BufferedInput.Peek().TimeStamp != b2.BufferedInput.Peek().TimeStamp)
        {
            return -1;
        }
        else
        {
           return b2.LastFrame.TimeStamp;
        }
       

    }

   

    private TimeSpan _indefiniteWaitTime=new TimeSpan(0,0,0,0,-1);

    private void Start()
    {
        StaticBuffers.Instance.EnemyBuffer.OnInputFrameAdded+=OnBufferUpdate;
        StaticBuffers.Instance.PlayerBuffer.OnInputFrameAdded += OnBufferUpdate;

    }
    void Update()
    {

      
        //If next frame is bigger than the confirm frame
        //not enough input to proceed simulation
        if (!ClientData.Pause)
        {
            if (WaitForConfirmFrame && !ClientData.SoloPlay)
            {
            
                //Wait till frames in play
                SpinWait.SpinUntil((() => { return (!WaitForConfirmFrame || ClientData.SoloPlay); }), _indefiniteWaitTime);

            }
        }
    }

}
