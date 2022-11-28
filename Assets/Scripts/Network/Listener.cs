using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using static Sender;
using System.Linq;

public class Listener : MonoBehaviour
{
    public bool toggleReceiving = false;  
    bool finishedReceiving=false;
    const int DEFAULT_LISTEN_PORT = 12000;

    int bufferLength;
    byte[] byteBuffer;
    
    public Socket listener;
    IPAddress hostIP;
    IPEndPoint IPEndPoint;
    //AsyncCallback onAcceptCallback = new AsyncCallback(LogNewAcceptedConnection);
    SocketAsyncEventArgs recvArgs;
    SocketAsyncEventArgs accceptArgs;
    public BufferStruct ReceivedBuffer;
    public static Socket receiver;

    private List<long> _timestampOneDifferences;
    private List<long> _timestampTwoDifferences;

    public delegate void InputElementsOnReceive();
    public event InputElementsOnReceive OnReceive;

    private static Listener _instance;


    private double _stampOnActualReceive;
    private double _stampOnExecutedReceive;


    public static Listener Instance
    {
        get { return _instance; }
        set
        {
            if (_instance != null)
            {
                Debug.LogError("Instance Already Exit");
            }
            else
            {
                _instance = value;
            }
        }
    }

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        Debug.LogError($"InputBuffer {this.GetInstanceID()} is an active object ");
        _timestampOneDifferences = new List<long>();
        _timestampTwoDifferences = new List<long>();

    }

    // Start is called before the first frame update
    void Start()
    {
        BufferStruct bufferStruct=new BufferStruct();
        bufferLength = Marshal.SizeOf(bufferStruct);
        byteBuffer = new byte[bufferLength];
        IsBound = false;
        InitListenSocket();
        InitReceiverSocket();
        OnReceive += SetSyncClockParameters;
    }

    void InitListenSocket() 
    {
        try
        {
            //Init Listener Socket
            listener = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Stream,
                                         ProtocolType.Tcp);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            Debug.Log($"Receiver Socket Initialzied on: {listener.LocalEndPoint}    ");
        }
        catch (Exception e)
        {
            Debug.Log($"Couldnt set up listener socket. Error {e.Message}");

            throw;
        }

        

    }

    void InitReceiverSocket() 
    {
        //Init Receiver Socket to random free socket address
        try
        {
            receiver = new Socket(AddressFamily.InterNetwork,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);

         
        }
        catch (Exception e )
        {

            Debug.Log($"Couldnt set up receiver socket. Error {e.Message}");
        }
       
    }

    public bool IsBound { get; set; }
    public bool IsListenerConnected() { return this.listener != null && listener.Connected; }

    public bool IsReceiverConnected() { return receiver != null && receiver.Connected; }

    public int GetListenerPortNum()
    {
        IPEndPoint localIpEndPoint = listener.LocalEndPoint as IPEndPoint;
        return localIpEndPoint.Port;
    }

    //TODO FIX GET PROT FUNCTIONS
    public string GetBoundPortString()
    {
        if (IPEndPoint is null)
        {
            return "Port Not Bound";
        }
        return IPEndPoint.ToString();
    }

    public string GetReceiverEndPointNum()
    {
        return receiver.RemoteEndPoint.ToString();
    }

    public string GetReceiverLocalPortNum()
    {
        return receiver.LocalEndPoint.ToString();
    }



    public bool StartListeningForConnections(int portNum=DEFAULT_LISTEN_PORT) 
    {
        if (listener.Connected == false) 
        {
            //Bind Listener socket
            IPAddress.TryParse("127.0.0.1", out hostIP);
            Debug.Log(hostIP.ToString());
            IPEndPoint = new IPEndPoint(hostIP, portNum);

            //Beign Listen
            listener.Bind(IPEndPoint);
            Debug.Log($"Listener Bound Port {IPEndPoint.ToString()}");

            listener.Listen(3);
            accceptArgs = new SocketAsyncEventArgs();
            accceptArgs.Completed += AcceptEventArg_Completed;
           


            listener.AcceptAsync(accceptArgs);
            //listener.BeginAccept(onAcceptCallback, listener);
            Debug.Log($"Listener Listening on Port {listener.LocalEndPoint.ToString()}");

            IsBound = true;
            return true;
        }
        return false;
    }


    void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError==SocketError.Success)
        {
            if (ClientData.IsClientInitiator)
            {
                Sender.Instance.toggleSend = false;
            }
            receiver = e.AcceptSocket;
           
        }
        else if (e.SocketError == SocketError.ConnectionReset)
        {
            throw new Exception("Socket Error");
        }
    }

   

  
    // Update is called once per frame
    public void Update()
    {
        //On key press start receiveing
        if (Input.GetKeyDown(KeyCode.B))
        {
            toggleReceiving = true;
        }

        if (receiver==null || !receiver.Connected)
        {
            return;
        }

        //Receiver is initialied and connected

        //IF event args are not set set them
        if (toggleReceiving || !ClientData.TwoWayConnectionEstablished())
        {
            if (recvArgs == null)
            {
                recvArgs = new SocketAsyncEventArgs();
                recvArgs.SetBuffer(byteBuffer, 0, bufferLength);
                recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveEventArg_Completed);
                recvArgs.SocketFlags = SocketFlags.None;
                finishedReceiving = false;
                bool receivedAll=!receiver.ReceiveAsync(recvArgs);
                if (receivedAll)
                {
                    finishedReceiving = true;
                }

            }
            //if receiving is finsihed
            if (finishedReceiving)
            {

                //Set buffer
                if (FrameLimiter.Instance != null)
                {
                    _stampOnExecutedReceive = FrameLimiter.Instance.GetTimeSinceGameStartup();
                }


                //Debug.LogError($"Actual Time of recv : {_stampOnActualReceive} executed: {_stampOnExecutedReceive} Diff" +
                //    $"{   _stampOnExecutedReceive- _stampOnActualReceive             }");
                ReceivedBuffer = RawDeserialize<BufferStruct>(byteBuffer, 0);
                //If no 2-way coonnection and this client didnt start the first send
                if (!ClientData.TwoWayConnectionEstablished() && !ClientData.IsClientInitiator)
                {
                    Sender.Instance.TrtConnectToPort(ReceivedBuffer.ListenerSocketInfo.portNum);
                }
                else
                {
                    //Debug.LogError($"Received Packets from {receiver.RemoteEndPoint}");

                    //call onReceive (main thread)
                    if (OnReceive != null)
                    {
                        OnReceive();
                    }

                    //recvArgs = new SocketAsyncEventArgs();
                    //recvArgs.SetBuffer(byteBuffer, 0, bufferLength);
                    //recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveEventArg_Completed);
                    //recvArgs.SocketFlags = SocketFlags.None;



                    //set receiving to false 
                    finishedReceiving = false;
                    //call receive async again
                    recvArgs.SetBuffer(byteBuffer, 0, bufferLength);

                    bool receivedAll = !receiver.ReceiveAsync(recvArgs);
                    if (receivedAll)
                    {
                        finishedReceiving = true;
                    }
                }
               
            }
        }

    }


   

    public void SetSyncClockParameters() 
    {
        
        //Initiator is the one responsible for sending the sync pakcages
        if (ClientData.IsClientInitiator)
        {
            //Wait for host should onyl be sned and never received
            if (ReceivedBuffer.SyncClock.waitForHost!=0)
            {
                Debug.LogError("Initiator shouldnt receive waitForHost parameter, only send it");
            }
            if (ReceivedBuffer.SyncClock.isActive)
            {
               
                ReceivedBuffer.SyncClock.initiatorReceive = FrameLimiter.Instance.GetTimeSinceGameStartup();
                double t1, t2;
                t1 = ReceivedBuffer.SyncClock.clientReceive - ReceivedBuffer.SyncClock.initiatorSend;
                t2 = ReceivedBuffer.SyncClock.clientSend - ReceivedBuffer.SyncClock.initiatorReceive;
                double clockOffset =                               
                    (t1 +t2) / 2.0;
                ReceivedBuffer.SyncClock.waitForHost = clockOffset;


                long clockOffsetMs = FrameLimiter.Instance.TickToMilliseconds(clockOffset);
                ////  Debug.LogError($"Clock offset is: {clockOffset}");


                //  //Reset the sync clock variable except the wait for host

                //  //TDOO SET THIS HERE AND TEST
                //  //BufferStruct.SyncClock.isActive = false;
                //  //BufferStruct.SyncClock.initiatorSend = 0;
                //  //BufferStruct.SyncClock.clientReceive = 0;
                //  //BufferStruct.SyncClock.clientSend = 0;
                //  //BufferStruct.SyncClock.initiatorReceive = 0;

                //  //Enemy is behind wait
                if (clockOffsetMs < -6)
                {
                   // Debug.LogError($"Waiting for oppoenent: {clockOffsetMs}");
                    //  FrameLimiter.Instance.WaitForMS(clockOffsetMs);
                    //ReceivedBuffer.SyncClock.unPause = true;

                  //  FrameLimiter.Instance.WaitForMsAtEndOfFrame= -clockOffsetMs;
                }
                else
                {
                    if (clockOffsetMs > 6)
                    {
                        //Debug.LogError($"Opponent should wait unpause: {clockOffsetMs}");
                        // ReceivedBuffer.SyncClock.unPause = true;

                        ReceivedBuffer.SyncClock.waitForHost = clockOffset;

                    }
                    else
                    {
                        //ReceivedBuffer.SyncClock.unPause = true;

                     //   Debug.LogError($"Game should unpause: {clockOffsetMs}");

                    }
                }


                //Debug.LogError($" Difference in  sync clocktimestamps: TD1: " +
                //    $"{FrameLimiter.Instance.TickToMilliseconds(t1)} " +
                //  $"TD2: { FrameLimiter.Instance.TickToMilliseconds(t2)}"
                //  );



                //double RTT = ((ReceivedBuffer.SyncClock.initiatorReceive - ReceivedBuffer.SyncClock.initiatorSend)
                //    - (ReceivedBuffer.SyncClock.clientSend - ReceivedBuffer.SyncClock.clientReceive));
                //Debug.LogError($"RTT: {FrameLimiter.Instance.TickToMilliseconds(RTT)}");

                //Debug.LogError($"MS to wait ={clockOffsetMs}," );



              
            }
        }
        else
        {
            long clockOffsetMs = FrameLimiter.Instance.TickToMilliseconds(ReceivedBuffer.SyncClock.waitForHost);

            if (ReceivedBuffer.SyncClock.waitForHost > 0)
            {

                //Debug.LogError($"Waiting for oppoenent: {clockOffsetMs}");
                //FrameLimiter.Instance.WaitForMsAtEndOfFrame+=clockOffsetMs;

                ReceivedBuffer.SyncClock.waitForHost = 0;
                //SyncClock.Instance.Reset();
            }

            if (ReceivedBuffer.SyncClock.isActive)
            {
                //Debug.LogError($"On Client Receive IsA={ReceivedBuffer.SyncClock.isActive}," +
                //   $"t0 = {ReceivedBuffer.SyncClock.initiatorSend}, t1={ReceivedBuffer.SyncClock.clientReceive}" +
                //   $"t2 ={ReceivedBuffer.SyncClock.clientSend}, t3={ReceivedBuffer.SyncClock.initiatorReceive} " +
                //   $"wait={ReceivedBuffer.SyncClock.waitForHost}"
                //   );
                //Set t1 time of receiveing
                if (ReceivedBuffer.SyncClock.clientReceive==0)
                {
                    ReceivedBuffer.SyncClock.clientReceive = _stampOnActualReceive;

                }
              

            }
        }
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


   
    //Will be invoked automatically wehen socket receive finises asynchonouslky
    void ReceiveEventArg_Completed(object sender, SocketAsyncEventArgs e)
    {
        if (ClientData.IsClientInitiator && ClientData.TwoWayConnectionEstablished())
        {
            Sender.Instance.toggleSend = true;
            int a = Sender.Instance.timesSend;
        }
        if (FrameLimiter.Instance!=null)
        {
            _stampOnActualReceive = FrameLimiter.Instance.GetTimeSinceGameStartup();

        }
        finishedReceiving = true;
    }




    void OnApplicationQuit()
    {
        if (receiver!=null)
        {
            if (receiver.Connected)
            {
                try
                {
                    receiver.Shutdown(SocketShutdown.Both);

                }
                catch (Exception)
                {

                    receiver.Close();

                }
            }
            //TODO check if receiver should be manually closed 
           
            Debug.Log("Receiver socket closed");
        }

        if (listener!=null)
        {
            if (listener.Connected)
            {
                try
                {
                    listener.Shutdown(SocketShutdown.Both);

                }
                finally
                {

                    listener.Close();

                }
            }
         
            Debug.Log("Listener socket closed");
        }

        Thread.Sleep(1000);
       
        
    }
}
