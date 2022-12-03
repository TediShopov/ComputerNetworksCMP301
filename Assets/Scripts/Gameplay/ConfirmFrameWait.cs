using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ConfirmFrameWait : MonoBehaviour
{
    //static int GetConfirmFrame(InputBuffer b, InputBuffer b2) 
    //{
    //    // 4 5 6 
    //    //   5  6  7
    //    int maxStartFrameStamp = Mathf.Max(b.LowestFrameStamp, b2.LowestFrameStamp);
    //    int minEndFrameStamp = Mathf.Min(b.HighestFrameStamp, b2.HighestFrameStamp);

    //    // 3 4 5 
    //    //       6 7 8
    //    //Todo check if this is ever reached 
    //    //Todo check how exactly the confirm frame should be calculated !
    //    //if (maxStartFrameStamp < minEndFrameStamp)
    //    //{
    //    //    return -1;
    //    //}
    //    return minEndFrameStamp;

    //}

    static int GetConfirmFrame(InputBuffer b, InputBuffer b2)
    {
        //Have to be algigned perfectly 
        if (b2.BufferedInput==null || b2.BufferedInput.Count==0)
        {
            return -1;
        }
        if (b.BufferedInput.Peek()._inputInFrame[0].timeStamp != b2.BufferedInput.Peek()._inputInFrame[0].timeStamp)
        {
            return -1;
        }
        else
        {
           return b2.LastFrame._inputInFrame[0].timeStamp;
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

        if (!ClientData.IsPaused)
        {
            if (FrameLimiter.Instance.FramesInPlay  > GetConfirmFrame())
            {
                Debug.LogError
                    ($" Simulation should wait Confirm Frame is still: {GetConfirmFrame()}");
                //Wait till frames in play
                SpinWait.SpinUntil((() => { return (FrameLimiter.Instance.FramesInPlay  <= GetConfirmFrame()); }), _indefiniteWaitTime);
                //why passes ffs dkdc
                int a = 3;
            }
        }

           
       // }
    }

}
