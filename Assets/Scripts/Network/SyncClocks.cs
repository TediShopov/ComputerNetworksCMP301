using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

//Class that is called when the game is paused at start of round to caluclate 
// RTT and send unpause command at the correct time
public class SyncClocks : MonoBehaviour
{
    public int TimesToCalculateRTT;
    List<double> _calculatedOneWayLatency;
    byte[] bufferBytes;
    int byteCount;
    SocketAsyncEventArgs sendArgs;
    SocketAsyncEventArgs recvArgs;
    // Start is called before the first frame update
    void Start()
    {
        //Init
        _calculatedOneWayLatency = new List<double>();

        SyncClock syncClockMockPacket = DefaultSyncClockBuffer();
        byteCount = System.Runtime.InteropServices.Marshal.SizeOf(syncClockMockPacket);
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
            bufferBytes = Sender.RawSerialize(syncClock);

            // Try to convert to send async
            if (sendArgs == null)
            {
                sendArgs = new SocketAsyncEventArgs();
                sendArgs.RemoteEndPoint = Sender.Instance.IPEndPoint;
                sendArgs.SocketFlags = SocketFlags.None;
            }
            sendArgs.SetBuffer(bufferBytes, 0, byteCount);
            bool sendAll = !Sender.Instance.sender.SendAsync(sendArgs);

            InitiatorReceive();
        }
        else
        {
            double avgLatenc = Queryable.Average(_calculatedOneWayLatency.AsQueryable());


            //Setup the sync clock packet to send
            syncClock.waitForHost = avgLatenc;
            //Seriazlie it to the byte buffer
            bufferBytes = Sender.RawSerialize(syncClock);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(InitiatorFinalSend_Completed);
            sendArgs.SetBuffer(bufferBytes, 0, byteCount);
            bool sendAll = !Sender.Instance.sender.SendAsync(sendArgs);
            if (sendAll)
            {
                InitiatorFinalSend_Completed(this,sendArgs);
            }


        }



    }

    void InitiatorReceive() 
    
    {
        if (recvArgs == null)
        {
            recvArgs = new SocketAsyncEventArgs();
            recvArgs.SetBuffer(bufferBytes, 0, byteCount);
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(InitiatorReceive_Completed);
            recvArgs.SocketFlags = SocketFlags.None;
        }

        bool receivedAll = !Listener.receiver.ReceiveAsync(recvArgs);
        if (receivedAll)
        {
            InitiatorReceive_Completed(this, recvArgs);
        }

    }

    void ClientReceiveAndSend() 
    {

        if (recvArgs == null)
        {
            recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ClientReceive_Completed);
            recvArgs.SocketFlags = SocketFlags.None;
         

        }
        recvArgs.SetBuffer(bufferBytes, 0, byteCount);

        bool receivedAll = !Listener.receiver.ReceiveAsync(recvArgs);
        if (receivedAll)
        {
            ClientReceive_Completed(this, recvArgs);
        }


    }
    void ClientSend(SyncClock clock) 
    {

        if (FrameLimiter.Instance != null)
        {
            //Stamp the time off receive
            clock.clientSend = FrameLimiter.Instance.GetTimeSinceGameStartup();
        }

        //Seriazlie it to the byte buffer
        bufferBytes = Sender.RawSerialize(clock);
        
        SocketAsyncEventArgs clientSendArgs = new SocketAsyncEventArgs();
        
       // clientSendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ClientSend_Complete);
        clientSendArgs.RemoteEndPoint = Sender.Instance.IPEndPoint;
        clientSendArgs.SocketFlags = SocketFlags.None;
        clientSendArgs.SetBuffer(bufferBytes, 0, byteCount);
        
        bool sendAll = !Sender.Instance.sender.SendAsync(clientSendArgs);
       
    }

    void InitiatorFinalSend_Completed(object sender, SocketAsyncEventArgs e)
    {
        double avgLatenc = Queryable.Average(_calculatedOneWayLatency.AsQueryable());
        //Millisecond to wait for the information to arrive to the other peer
        long avgMsToWait = FrameLimiter.Instance.TickToMilliseconds(avgLatenc);
        FrameLimiter.Instance.WaitForMsAtEndOfFrame += avgMsToWait;
        ClientData.IsPaused = false;
        FrameLimiter.Instance.FPSLimit = 60;

    }
    void ClientReceive_Completed(object sender, SocketAsyncEventArgs e)
    {
        //Deserialzie the received buffer
        SyncClock syncClock = Listener.RawDeserialize<SyncClock>(bufferBytes, 0);

        if (syncClock.waitForHost>0)
        {
            FrameLimiter.Instance.FPSLimit = 60;

            ClientData.IsPaused = false;
        }
        if (FrameLimiter.Instance != null)
        {
            //Stamp the time off receive
            syncClock.clientReceive = FrameLimiter.Instance.GetTimeSinceGameStartup();
        }

        // Try to convert to send async
        ClientSend(syncClock);
      
    }

    void InitiatorReceive_Completed(object sender, SocketAsyncEventArgs e) 
    {
        SyncClock syncClock = Listener.RawDeserialize<SyncClock>(bufferBytes,0);
        syncClock.initiatorReceive = FrameLimiter.Instance.GetTimeSinceGameStartup();
        double RTT = ((syncClock.initiatorReceive - syncClock.initiatorSend)
                       - (syncClock.clientSend - syncClock.clientReceive));
        double oneWayLatencyApprox = RTT / 2.0;
       
        this._calculatedOneWayLatency.Add(oneWayLatencyApprox);
        Debug.LogError($"oneWayLatencyApprox: {oneWayLatencyApprox}");
        double avgLatenc = Queryable.Average(_calculatedOneWayLatency.AsQueryable());

        Debug.LogError($"latencyAvg: {avgLatenc}");

        InitiatorSend();

    }

    
    

    



    // Update is called once per frame
    void Update()
    {
      
    }
}
