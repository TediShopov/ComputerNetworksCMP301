using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

public class SocketComunication : MonoBehaviour
{
    const int DEFAULT_LISTEN_PORT = 12000;

    static public Socket Receiver;
    static public Socket Sender;
    static public Socket ConnectionListener;


    private Socket InitSocket()
    {
        Socket socket;
        try
        {
            //Init Listener Socket
            socket = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Stream,
                                         ProtocolType.Tcp);
            Debug.Log($"Socket Initialzied on: {socket.LocalEndPoint}");
            return socket;
        }
        catch (Exception e)
        {
            Debug.Log($"Couldnt set up listener socket. Error {e.Message}");

            throw;
        }
    }


     
    //Port Bind Connection Listener
    static public bool BindConnectionListener(int portNum = DEFAULT_LISTEN_PORT)
    {
        if (ConnectionListener.Connected == false)
        {
            IPAddress hostIP;
            IPEndPoint IPEndPoint;
            //Bind Listener socket
            IPAddress.TryParse("127.0.0.1", out hostIP);
            Debug.Log(hostIP.ToString());
            IPEndPoint = new IPEndPoint(hostIP, portNum);

            //Beign Listen
            ConnectionListener.Bind(IPEndPoint);
            Debug.Log($"Listener Bound Port {IPEndPoint.ToString()}");

            ConnectionListener.Listen(1);
           
            Debug.Log($" Listening on Port {portNum}");
            return true;
        }
        return false;
    }
   

    //Connect Sender To 
    public static bool TryConnectToPort(int portNum)
    {
        try
        {
            IPAddress hostIP;
            //Bind Listener socket
            IPAddress.TryParse("127.0.0.1", out hostIP);
            IPEndPoint ip = new IPEndPoint(hostIP, portNum);

            Sender.ConnectAsync(ip).Wait();
            Debug.Log("Trying to connect to listener");
            ClientData.IsClientInitiator = !Receiver.Connected;
            return true;
        }
        catch (Exception e)
        {
            Debug.Log($"Trying to connect throwed exception");

            return Sender.Connected;
        }

    }

    public static byte[] RawSerialize(object anything)
    {
        int rawSize = Marshal.SizeOf(anything);
        IntPtr buffer = Marshal.AllocHGlobal(rawSize);
        Marshal.StructureToPtr(anything, buffer, false);
        byte[] rawDatas = new byte[rawSize];
        Marshal.Copy(buffer, rawDatas, 0, rawSize);
        Marshal.FreeHGlobal(buffer);
        return rawDatas;
    }

    public static T RawDeserialize<T>(byte[] rawData, int position)
    {
        int rawsize = Marshal.SizeOf(typeof(T));
        if (rawsize > rawData.Length - position)
            throw new ArgumentException("Not enough data to fill struct. Array length from position: " + (rawData.Length - position) + ", Struct length: " + rawsize);
        IntPtr buffer = Marshal.AllocHGlobal(rawsize);
        Marshal.Copy(rawData, position, buffer, rawsize);
        T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
        Marshal.FreeHGlobal(buffer);
        return retobj;
    }

    public static void DefaultSend(byte[] buff, Action<object, SocketAsyncEventArgs> onCompleteAction=null)
    {
        SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
        arg.SetBuffer(buff, 0, buff.Length);
        if (onCompleteAction!=null)
        {
            arg.Completed += new EventHandler<SocketAsyncEventArgs>(onCompleteAction);

        }
        arg.SocketFlags = SocketFlags.None;
        arg.RemoteEndPoint = Sender.RemoteEndPoint;

        bool sendAll = !SocketComunication.Sender.SendAsync(arg);
        if (sendAll)
        {
            if (onCompleteAction!=null)
            {
                onCompleteAction(SocketComunication.Sender, arg);

            }
        }
    }


    public static void DefaultReceive(byte[] buff, Action<object, SocketAsyncEventArgs> onCompleteAction)
    {
        SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
        arg.SetBuffer(buff, 0, buff.Length);
        if (onCompleteAction!=null)
        {
            arg.Completed += new EventHandler<SocketAsyncEventArgs>(onCompleteAction);

        }
        arg.SocketFlags = SocketFlags.None;


        bool receivedAll = !SocketComunication.Receiver.ReceiveAsync(arg);
        if (receivedAll)
        {
            if (onCompleteAction != null)
            {
                onCompleteAction(SocketComunication.Receiver, arg);
            }
        }
    }

    void ShutdownAndCloseSocket(Socket sock, string socketName) 
    {
        if (sock != null)
        {
            if (sock.Connected)
            {
                try
                {
                    sock.Shutdown(SocketShutdown.Both);

                }
                finally
                {
                    sock.Close();
                }
            }
            Debug.Log($"{socketName} socket closed");
        }
    }
    void Awake()
    {
        Receiver = InitSocket();
        Sender = InitSocket();
        ConnectionListener = InitSocket();
        ListenOnDefaultPort();
        //Todo check if we need a lingering option
        Sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true,0));
        Receiver.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 0));


        DontDestroyOnLoad(this.gameObject);
    }

    public void ListenOnDefaultPort()
    {
        
            if (ConnectionListener != null)
            {
                bool succeded = false;
                try
                {
                    succeded = BindConnectionListener(12000);
                }
                catch (System.Exception e)
                {
                    succeded = BindConnectionListener(13000);
                }
        
            }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     void OnApplicationQuit()
    {
        ShutdownAndCloseSocket(Receiver, "Receiver");
        ShutdownAndCloseSocket(Sender, "Sender");
        ShutdownAndCloseSocket(ConnectionListener, "ConnectionListener");
    }
}
