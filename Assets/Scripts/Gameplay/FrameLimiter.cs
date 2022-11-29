using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Timers;
using System.Diagnostics;

public class FrameLimiter : MonoBehaviour
{
    public  bool ToggleSleep=false;
    public long SleepForMs = 160;
    private static Int32 _framesInPlay;


    public double FPSLimit;
    public long FPSLimitTicks;
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

}

