using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Timers;
using System.Diagnostics;

public class FrameLimiter : MonoBehaviour
{

    //public float FPSLimit = 60.0f;
    //float currentFrameTime;
    //public bool WaitForReceivedInput;
    //private bool _receivedInput = false;
    //private bool _receivedInputOnce = false;
    public  bool ToggleSleep=false;
    public long SleepForMs = 160;
    private static Int32 _framesInPlay;


    public double FPSLimit;
    public long FPSLimitTicks;

    private long timeAfterFrameMs;

    private long lastTime;

    public static FrameLimiter Instance { get; set; }
    public static Int32 FramesInPlay
    {
        get { return _framesInPlay; }
        private set { _framesInPlay = value; }
    }


    private Stopwatch Stopwatch;
    private void Awake()
    {
        Instance = this;
        Stopwatch =new Stopwatch();
        Stopwatch.Start();
        lastTime= Stopwatch.ElapsedTicks;
        double sToWait = (1.0 / FPSLimit) ;
        long tickToWait = (long)((double)Stopwatch.Frequency / FPSLimit);

        FPSLimitTicks = (long)(tickToWait);
    }



    public double GetTimeSinceGameStartup() 
    {
        return Stopwatch.ElapsedTicks;
    }
    public long TickToMilliseconds(double ticks) 
    {
        double seconds = ticks / (double)Stopwatch.Frequency;
        return (long)((seconds) * 1000);
    }



    void Start()
    {
      
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;
        //currentFrameTime = Time.realtimeSinceStartup;

        
        StartCoroutine("WaitForNextFrame");
       // Listener.Instance.OnReceive += UpdateReceivedInput;
    }

    void UpdateReceivedInput() 
    {
        //_receivedInputOnce = true;
        //_receivedInput = true;
    }

    public long WaitForMsAtEndOfFrame = 0;

    public void WaitForMS(long ms) 
    {
        Stopwatch waitStopwatch = new Stopwatch();
        waitStopwatch.Start();
        var now = waitStopwatch.ElapsedMilliseconds;
        SleepForMs = ms;
        var timeToWaitUntil = now + ms;
        Stopwatch.Stop();
        UnityEngine.Debug.LogError($"Sleeping for {ms} milliseconds");
        SpinWait.SpinUntil(() => { return (waitStopwatch.ElapsedMilliseconds >= timeToWaitUntil); });
        Stopwatch.Start();
        waitStopwatch.Stop();

    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.K))
    //    {
    //        this.WaitForMS(160);
    //    }
    //}

    void Update()
    {
        if (FPSLimit == 0.0) return;

        if (!ClientData.IsPaused)
        {
            FramesInPlay++;
        }

        var now = Stopwatch.ElapsedTicks;

        if (WaitForMsAtEndOfFrame>0)
        {
            WaitForMS(WaitForMsAtEndOfFrame);
            WaitForMsAtEndOfFrame = 0;
        }

       lastTime += FPSLimitTicks;


        if (now >= lastTime)
        {
            lastTime = now;
            return;
        }
        else
        {
            //var fpsCount = Stopwatch.Frequency / (now - lastTime);
            //UnityEngine.Debug.LogError($"SPIN WAIT FPS: {fpsCount}");
            SpinWait.SpinUntil(() => { return (Stopwatch.ElapsedTicks >= lastTime); });
        }


       


    }
    //IEnumerator  WaitForNextFrame()
    //{
    //    while (true)
    //    {
    //        _receivedInput = false;
    //        yield return new WaitForEndOfFrame();
    //        ++FramesInPlay;

    //        if (WaitForReceivedInput && !_receivedInput && _receivedInputOnce)
    //        {
    //            while (!_receivedInput)
    //            {
    //                //break;
    //                //do nothing;
    //            }
    //        }


    //        timeAfterFrameMs += 16;
    //        long t = Stopwatch.ElapsedMilliseconds;
    //        long msSleepTime = timeAfterFrameMs - t ;

    //        if (msSleepTime > 0)
    //        {
    //            UnityEngine.Debug.LogError($"Time to wait l: {msSleepTime} == i: {(int)msSleepTime}");
    //            Thread.Sleep((int)msSleepTime);
    //            while (t < timeAfterFrameMs)
    //            {
    //               t = Stopwatch.ElapsedMilliseconds;
    //            }
    //        }

    //        if (ToggleSleep)
    //        {
    //            Thread.Sleep((int)SleepForMs);
    //            ToggleSleep = false;
    //            //FramesInPlay += sleepForFrames;
    //            t = Stopwatch.ElapsedMilliseconds;
    //            timeAfterFrameMs += SleepForMs/16;
    //        }


    //        //currentFrameTime += 1.0f / FPSLimit;
    //        //var t = Time.realtimeSinceStartup;
    //        //var sleepTime = currentFrameTime - t - 0.01f;



    //        //if (sleepTime > 0)
    //        //        Thread.Sleep((int)(sleepTime * 1000));
    //        //    while (t < currentFrameTime)
    //        //        t = Time.realtimeSinceStartup;


    //        //if (ToggleSleep)
    //        //{
    //        //    Thread.Sleep((int)((1.0f / FPSLimit) * sleepForFrames * 1000));
    //        //    ToggleSleep = false;
    //        //    //FramesInPlay += sleepForFrames;
    //        //    t = Time.realtimeSinceStartup;
    //        //    currentFrameTime += (1.0f / FPSLimit) * sleepForFrames;
    //        //}

    //    }
    //}
}

