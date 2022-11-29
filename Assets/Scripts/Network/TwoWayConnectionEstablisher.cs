using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public struct ListenerSocketInfo
{
    //4 bytes
    public Int32 ipAddress;

    //4 bytes accounted
    public Int32 portNum;
}

public class TwoWayConnectionEstablisher : MonoBehaviour
{
    bool receiveOncePacketInformation = false;
    ListenerSocketInfo listenerSocketInfo;
    byte[] buffer;
    int bufferLength;
    // Start is called before the first frame update
    void Start()
    {
        buffer=SocketComunication.RawSerialize(listenerSocketInfo);
        bufferLength = buffer.Length;
        buffer = new byte[bufferLength];

        SocketAsyncEventArgs accceptArgs = new SocketAsyncEventArgs();
        accceptArgs.Completed += Accept_Completed;
        SocketComunication.ConnectionListener.AcceptAsync(accceptArgs);
    }

     void Accept_Completed(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {

            SocketComunication.Receiver = e.AcceptSocket;

            //Connection established to receiver but not sender
            //Not inittiator
            if (SocketComunication.Receiver.Connected && !SocketComunication.Sender.Connected)
            {
                ClientData.IsClientInitiator = false;


                // NEW CODE 
                SocketComunication.DefaultReceive(buffer, ConnectSenderToReceiver_OnReceive);

               

            }

        }
        else if (e.SocketError == SocketError.ConnectionReset)
        {
            throw new Exception("Socket Error");
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    void ConnectSenderToReceiver_OnReceive(object sender, SocketAsyncEventArgs e)
    {
        listenerSocketInfo = SocketComunication.RawDeserialize<ListenerSocketInfo>(e.Buffer,0);
        if (listenerSocketInfo.portNum > 0)
        {
            SocketComunication.TryConnectToPort(listenerSocketInfo.portNum);
        }
    }

    public void EstablishTwoWayConnection(int portNum) 
    {
        if (!ClientData.TwoWayConnectionEstablished())
        {
            //Try Establishing One Way Connection First between Initiator Sender 
            //and Client Receiver
            bool success=SocketComunication.TryConnectToPort(portNum);


            //If successfull pass the other peer our receiver socket information
            if (success)
            {
                var sender = SocketComunication.Sender;

                //Setup buffer
                IPEndPoint ip = SocketComunication.ConnectionListener.LocalEndPoint as IPEndPoint;

                listenerSocketInfo.ipAddress = (int)ip.Address.Address;
                listenerSocketInfo.portNum = ip.Port;


                buffer = SocketComunication.RawSerialize(listenerSocketInfo);

                SocketComunication.DefaultSend(buffer);
                
            }
        }
    }




    


   

}
