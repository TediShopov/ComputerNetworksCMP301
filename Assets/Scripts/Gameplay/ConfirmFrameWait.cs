using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ConfirmFrameWait : MonoBehaviour
{
  

    static int GetConfirmFrame(InputBuffer b, InputBuffer b2)
    {
        //Have to be algigned perfectly 
        if (b2.BufferedInput==null || b2.BufferedInput.Count==0)
        {
            return -1;
        }
        if (b.BufferedInput.Peek().TimeStamp != b2.BufferedInput.Peek().TimeStamp)
        {
            
            return -1;
        }
        else
        {
           return b2.LastFrame.TimeStamp;
        }
       

    }

    static int GetConfirmFrame()
    {

        return GetConfirmFrame(StaticBuffers.Instance.PlayerBuffer, StaticBuffers.Instance.EnemyBuffer);
    }
    private TimeSpan _indefiniteWaitTime=new TimeSpan(0,0,0,0,-1);
    void Update()
    {

        //if (GetConfirmFrame() >= 0)
        //{
        //If next frame is bigger than the confirm frame
        //not enough input to proceed simulation

        if (!ClientData.Pause)
        {
            if (FrameLimiter.Instance.FramesInPlay > GetConfirmFrame())
            {
                Debug.LogError
                    ($" Simulation should wait Confirm Frame is still: {GetConfirmFrame()}");
                //Wait till frames in play
                SpinWait.SpinUntil((() => { return (FrameLimiter.Instance.FramesInPlay <= GetConfirmFrame()); }), _indefiniteWaitTime);
            }
        }


        // }
    }

}
