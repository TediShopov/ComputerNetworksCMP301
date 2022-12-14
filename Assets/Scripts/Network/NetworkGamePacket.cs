using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;



public struct InputFramePacket 
{
    public Int32 TimeStamp; //4 bytes
    //The Input Data to send
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 6)]
    public byte[] InputElements;
}
public class NetworkGamePacket : MonoBehaviour
{
    //TODO maybe store multiple
    static private InputFramePacket _lastReceivedGamePacket;
    static public InputFramePacket LastReceivedGamePacket { 
        get { 
                lock (receiveLock)
            {
                return _lastReceivedGamePacket;
            }
                 }
        private set { _lastReceivedGamePacket = value; } }

    public bool reocuringReceiveEvent { get; set; }
    public bool sendFinished { get; set; }

    public bool isReceiverStarted { get; set; }

    public byte[] receiveByteBuffer { get; set; }


    private static readonly object receiveLock = new object();

    public byte[] sendByteBuffer { get; set; }
    // Start is called before the first frame update
    void Start()
    {

        InputFramePacket gp;
        gp.TimeStamp = -1;
        gp.InputElements = new byte[4];
        var b = SocketComunication.RawSerialize(gp);


        //GamePacket gp;
        //gp.TimeStamp = -1;
        //gp.InputElements = new InputElement[10];
        receiveByteBuffer = SocketComunication.RawSerialize(gp);
        sendByteBuffer = SocketComunication.RawSerialize(gp);
        sendFinished = true;
        reocuringReceiveEvent = true;
    }



    InputFramePacket PrepareGamePacket() 
    {
       // GamePacket gp;
        InputFramePacket gp;
       
        if (StaticBuffers.Instance.PlayerBuffer.LastFrame!=null)
        {

            gp.TimeStamp = StaticBuffers.Instance.PlayerBuffer.LastFrame.TimeStamp;
            gp.InputElements = StaticBuffers.Instance.PlayerBuffer.LastFrame.Inputs;
        }
        else
        {
            throw new System.Exception("Error send empty player buffer");
        }
        return gp;
    }

    // Update is called once per frame
    void Update()
    {
        if (ClientData.Pause || !ClientData.TwoWayConnectionEstablished())
        {
            return;
        }

        if (!isReceiverStarted)
        {
            isReceiverStarted = true;
            SocketComunication.DefaultReceive(receiveByteBuffer,ReceiveGamePacket_Reoccur);

        }

        if (sendFinished)
        {
            sendFinished = false;
            //Update the buffer to send


            var gamePacket=PrepareGamePacket();
            sendByteBuffer = SocketComunication.RawSerialize(gamePacket);
           // Debug.LogError($"Send Bytes for frame {FrameLimiter.Instance.FramesInPlay}");
            SocketComunication.DefaultSend(sendByteBuffer, SendGamePacket_Completed);
        }
        else
        {
            //Debug.LogError("Couldnt send input on this frame");
        }

    }

    public void SendGamePacket_Completed(object e, SocketAsyncEventArgs arg) 
    {
        sendFinished = true;
    }

    public void ReceiveGamePacket_Reoccur(object e, SocketAsyncEventArgs arg)
    {
        lock (receiveLock)
        {
            LastReceivedGamePacket = SocketComunication.RawDeserialize<InputFramePacket>(arg.Buffer, 0);

            //Could run before input buffer objects are init
            StaticBuffers.Instance.EnemyBuffer?.Enqueue(
                new InputFrame(LastReceivedGamePacket.InputElements,LastReceivedGamePacket.TimeStamp));
        }

        if (reocuringReceiveEvent)
        {
            SocketComunication.DefaultReceive(receiveByteBuffer, ReceiveGamePacket_Reoccur);
        }
    }
}
