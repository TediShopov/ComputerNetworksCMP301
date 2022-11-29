using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public struct SyncClock
{
    // public bool unPause;
    public bool isActive;               // If Client or initiator is responsible for filling timestamps
    public double waitForHost;           // If set to > 0, Client has to wait for that amount of frames
    public double initiatorSend;         // Time stamp on initiator send
    public double clientReceive;         // Time Stamp on client receive
    public double clientSend;            // Time stmap on client send
    public double initiatorReceive;      // TIme stamp on initiator receive
}

//Class that is called when the game is paused at start of round to caluclate 
// RTT and send unpause command at the correct time
public class SyncClocks : MonoBehaviour
{
    public int TimesToCalculateRTT;
    List<double> _calculatedOneWayLatency;
    byte[] bufferBytes;
    int byteCount;
    // Start is called before the first frame update
    void Start()
    {
        //Init
        _calculatedOneWayLatency = new List<double>();

        SyncClock syncClockMockPacket = DefaultSyncClockBuffer();
        byteCount = System.Runtime.InteropServices.Marshal.SizeOf(syncClockMockPacket);
        bufferBytes = SocketComunication.RawSerialize(syncClockMockPacket);
        if (!ClientData.TwoWayConnectionEstablished())
        {
            throw new System.Exception("Problem establishing connection between sockets");
        }

        


        if (ClientData.IsClientInitiator)
        {
            InitiatorSend();
        }
        else
        {
            ClientReceiveAndSend();
        }



        
        ClientData.IsPaused = true;
        FrameLimiter.Instance.FPSLimit = 9999;

    }


    SyncClock DefaultSyncClockBuffer() 
    {
        SyncClock syncClock;
        syncClock.isActive = false;
        syncClock.waitForHost = 0;
        syncClock.initiatorSend = 0;
        syncClock.clientReceive = 0;
        syncClock.clientSend = 0;
        syncClock.initiatorReceive = 0;
        return syncClock;
    }
    void InitiatorSend() 
    {
        SyncClock syncClock = DefaultSyncClockBuffer();

        //
        if (_calculatedOneWayLatency.Count < TimesToCalculateRTT)
        {
            //Setup the sync clock packet to send
            syncClock.isActive = true;
            syncClock.initiatorSend = FrameLimiter.Instance.GetTimeSinceGameStartup();

            //Seriazlie it to the byte buffer
            bufferBytes = SocketComunication.RawSerialize(syncClock);
            SocketComunication.DefaultSend(bufferBytes);
        
            InitiatorReceive();
        }
        else
        {
            double avgLatenc = Queryable.Average(_calculatedOneWayLatency.AsQueryable());

            //Setup the sync clock packet to send
            syncClock.waitForHost = avgLatenc;
            //Seriazlie it to the byte buffer
            bufferBytes = SocketComunication.RawSerialize(syncClock);
            SocketComunication.DefaultSend(bufferBytes,InitiatorFinalSend_Completed);
        }



    }


   

    void InitiatorReceive() 
    
    {
        SocketComunication.DefaultReceive(bufferBytes, InitiatorReceive_Completed);
    }

    void ClientReceiveAndSend() 
    {
        SocketComunication.DefaultReceive(bufferBytes, ClientReceive_Completed);
    }
    void ClientSend(SyncClock clock) 
    {

        if (FrameLimiter.Instance != null)
        {
            //Stamp the time off receive
            clock.clientSend = FrameLimiter.Instance.GetTimeSinceGameStartup();
        }

        //Seriazlie it to the byte buffer
        bufferBytes = SocketComunication.RawSerialize(clock);
        SocketComunication.DefaultSend(bufferBytes, ClientSend_Completed);
    }

    void ClientSend_Completed(object sender, SocketAsyncEventArgs e)
    {
        ClientReceiveAndSend();
    }

    void InitiatorFinalSend_Completed(object sender, SocketAsyncEventArgs e)
    {
        double avgLatenc = Queryable.Average(_calculatedOneWayLatency.AsQueryable());
        Debug.LogError("Final Sync Clock Packet send");
        //Millisecond to wait for the information to arrive to the other peer
        long avgMsToWait = FrameLimiter.Instance.TickToMilliseconds(avgLatenc);
        FrameLimiter.Instance.WaitForMsAtEndOfFrame += avgMsToWait;
        ClientData.IsPaused = true;
        FrameLimiter.Instance.FPSLimit = 60;

    }
    void ClientReceive_Completed(object sender, SocketAsyncEventArgs e)
    {
        //Deserialzie the received buffer
        SyncClock syncClock = SocketComunication.RawDeserialize<SyncClock>(bufferBytes, 0);

        if (syncClock.waitForHost>0)
        {
            FrameLimiter.Instance.FPSLimit = 60;

            ClientData.IsPaused = true;
        }
        else
        {
            if (FrameLimiter.Instance != null)
            {
                //Stamp the time off receive
                syncClock.clientReceive = FrameLimiter.Instance.GetTimeSinceGameStartup();
            }

            // Try to convert to send async
            ClientSend(syncClock);
        }
       
      
    }

    void InitiatorReceive_Completed(object sender, SocketAsyncEventArgs e) 
    {
        SyncClock syncClock = SocketComunication.RawDeserialize<SyncClock>(bufferBytes,0);
        syncClock.initiatorReceive = FrameLimiter.Instance.GetTimeSinceGameStartup();
        double RTT = ((syncClock.initiatorReceive - syncClock.initiatorSend)
                       - (syncClock.clientSend - syncClock.clientReceive));
        double oneWayLatencyApprox = RTT / 2.0;
       
        this._calculatedOneWayLatency.Add(oneWayLatencyApprox);
        Debug.LogError($"oneWayLatencyApprox: {oneWayLatencyApprox}");
        double avgLatenc = Queryable.Average(_calculatedOneWayLatency.AsQueryable());

        Debug.LogError($"latencyAvg: {avgLatenc} Time:{_calculatedOneWayLatency.Count}");

        InitiatorSend();

    }

    
    

    



    // Update is called once per frame
    void Update()
    {
      
    }
}
