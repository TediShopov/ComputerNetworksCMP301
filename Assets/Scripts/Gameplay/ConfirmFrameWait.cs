using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ConfirmFrameWait : MonoBehaviour
{
    static int GetConfirmFrame(InputBuffer b, InputBuffer b2) 
    {
        int maxStartFrameStamp = Mathf.Max(b.LowestFrameStamp, b2.LowestFrameStamp);
        int minEndFrameStamp = Mathf.Min(b.HighestFrameStamp, b2.HighestFrameStamp);
        if (maxStartFrameStamp < minEndFrameStamp)
        {
            return -1;
        }
        return minEndFrameStamp;

    }

     void Update()
    {
        int confirmFrame = GetConfirmFrame(StaticBuffers.Instance.PlayerBuffer, StaticBuffers.Instance.EnemyBuffer);
        if (confirmFrame>-1 )
        {
            //If next frame has the reuqired input
            if (FrameLimiter.FramesInPlay+1 > confirmFrame)
            {
                Debug.LogError($"Confirm Frame is still: {confirmFrame}");
            }
        }
    }

}
