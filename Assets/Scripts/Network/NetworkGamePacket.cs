using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;


public struct GamePacket
{
    //The Input Data to send
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 4)]
    public InputElement[] InputElements;
}
public class NetworkGamePacket : MonoBehaviour
{
    //TODO maybe store multiple
    static public GamePacket LastReceivedGamePacket;

    public bool reocuringReceiveEvent { get; set; }
    public bool sendFinished { get; set; }

    public bool isReceiverStarted { get; set; }

    public byte[] receiveByteBuffer { get; set; }


    private readonly object receiveLock = new object();

    public byte[] sendByteBuffer { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        GamePacket gp;
        gp.InputElements = new InputElement[10];
        receiveByteBuffer = SocketComunication.RawSerialize(gp);
        sendByteBuffer = SocketComunication.RawSerialize(gp);
        sendFinished = true;
        reocuringReceiveEvent = true;
    }

   

    GamePacket PrepareGamePacket() 
    {
        GamePacket gp;
        if (StaticBuffers.Instance.PlayerBuffer.LastFrame!=null)
        {
            gp.InputElements = StaticBuffers.Instance.PlayerBuffer.LastFrame._inputInFrame;

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
        if (ClientData.IsPaused || !ClientData.TwoWayConnectionEstablished())
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
            Debug.LogError("Couldnt send input on this frame");
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
            LastReceivedGamePacket = SocketComunication.RawDeserialize<GamePacket>(arg.Buffer, 0);
            //TODO probably will need a lock

            //Could run before input buffer objects are init
            if (StaticBuffers.Instance != null)
            {
                if (StaticBuffers.Instance.EnemyBuffer != null)
                {
                    StaticBuffers.Instance.EnemyBuffer.AddNewFrame(
                       new InputFrame(LastReceivedGamePacket.InputElements));
                }
            }

        }


        if (reocuringReceiveEvent)
        {
            SocketComunication.DefaultReceive(receiveByteBuffer, ReceiveGamePacket_Reoccur);
        }
    }
}
