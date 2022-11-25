using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
[StructLayout(LayoutKind.Sequential)]

public struct ListenerSocketInfo 
{
    //4 bytes
    public Int32 ipAddress;

    //4 bytes accounted
    public Int32 portNum;
}

//Sync Clock Header informatiopn

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

public struct BufferStruct
{
    //Listener socket to connect to 
    public ListenerSocketInfo ListenerSocketInfo;
    //Information for synchronising clocks of client and initiator
    public SyncClock SyncClock;
    //The Input Data to send
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 10)]
    public InputElement[] InputElements;
}


public class Sender : MonoBehaviour
{

    IPAddress hostIP;
    IPEndPoint IPEndPoint;
    Socket sender;
    SocketAsyncEventArgs sendArgs;
    // private readonly object sendBufferLock = new object();
    public bool SendUpdateEnded = false;

    public bool IsSenderConnected() { return sender != null && sender.Connected; }


    public int TimestampLastSend;
    public int TimestampCurrentSend;

    public int ClockSyncEachFrame = 15;
    private int _syncClockFramePassed = 0;
    private bool _sendSyncClockOnce = false;

    public bool ShouldSentSyncClockPackage()
    {
        if (_sendSyncClockOnce==false)
        {
            return _syncClockFramePassed > ClockSyncEachFrame;
        }
        return false;
    }

   

    public int SendAfterFrames;
    private int _waitedFrames;
    public bool toggleSend = false;
    public bool sendFinished = true;
    byte[] buffBytes;
    int byteCount;
 
    // Start is called before the first frame update
    void Start()
    {
        InitSenderSocket();
        
    }
   public int timesSend = 0;
    int timesSendVoidInput=0;




    private BufferStruct InitDefaultBuffer() 
    {
        BufferStruct BufferStruct;
        //Network Information
        BufferStruct.ListenerSocketInfo.ipAddress = 0;
        BufferStruct.ListenerSocketInfo.portNum = 0;

        //Sync Clock Header informatiopn
        BufferStruct.SyncClock.isActive = false;
        BufferStruct.SyncClock.initiatorSend = 0;
        BufferStruct.SyncClock.clientReceive = 0;
        BufferStruct.SyncClock.clientSend = 0;
        BufferStruct.SyncClock.initiatorReceive = 0;
        //Sync Clock Result information -- ONLY SEND WHEN CLIENT HAS TO WAIT
        BufferStruct.SyncClock.waitForHost = 0;
       // BufferStruct.SyncClock.unPause = false;

        //Default input buffer
        BufferStruct.InputElements = new InputElement[10];

        return BufferStruct;
    }


    private void UpdateBufferToSend()
    {

        BufferStruct BufferStruct=InitDefaultBuffer();
        IPEndPoint localIpEndPoint = sender.LocalEndPoint as IPEndPoint;

        //Setup the listen socket informatiopn
        BufferStruct.ListenerSocketInfo.ipAddress = (int)localIpEndPoint.Address.Address;
        BufferStruct.ListenerSocketInfo.portNum = Listener.Instance.GetListenerPortNum();


        //Inititalie input elements if there is a setup Player Buffer of inputs
        if (StaticBuffers.Instance!=null && StaticBuffers.Instance.PlayerBuffer.GetBuffer() != null)
        {
            BufferStruct.InputElements = StaticBuffers.Instance.PlayerBuffer.GetBuffer().ToArray();
            int a = timesSendVoidInput;
        }
        else
        {
            timesSendVoidInput++;
        }

        //Sync Clock Package information
        if (ClientData.Instance.IsClientInitiator)
        {
            //Initiator is responsible for timestamp first and last timestamp, isActive, waitFor host

            //If Initiator has received the infroamtion about the clock synhronization process
            //and the client is faster so have to wait
            if (Listener.Instance.ReceivedBuffer.SyncClock.waitForHost>0)
            {
                BufferStruct.SyncClock.waitForHost = Listener.Instance.ReceivedBuffer.SyncClock.waitForHost;
            }
            //if (Listener.Instance.ReceivedBuffer.SyncClock.unPause ==true)
            //{
            //    //BufferStruct.SyncClock.unPause = true;
            //    ClientData.Instance.IsPaused = false;
            //}

            //Starting the sync clock sequence
            if (ShouldSentSyncClockPackage())
            {
                _syncClockFramePassed = 0;
                BufferStruct.SyncClock.isActive = true;
                BufferStruct.SyncClock.initiatorSend = FrameLimiter.Instance.GetTimeSinceGameStartup();

                //Debug.LogError($"On Initiator Send IsA={BufferStruct.SyncClock.isActive}," +
                // $"t0 = {BufferStruct.SyncClock.initiatorSend}, t1={BufferStruct.SyncClock.clientReceive}" +
                // $"t2 ={BufferStruct.SyncClock.clientSend}, t3={BufferStruct.SyncClock.initiatorReceive} " +
                // $"wait={BufferStruct.SyncClock.waitForHost}"
                // );

            }
        }
        else
        {
            //Sync the send buffer with the the one got on receive that has isActive and on receivetimestamp
            BufferStruct.SyncClock = Listener.Instance.ReceivedBuffer.SyncClock;

            //Only wrtie to timestamp when you have to 
            if (BufferStruct.SyncClock.isActive)
            {
                //BufferStruct.SyncClock.clientSend = FrameLimiter.FramesInPlay;
                BufferStruct.SyncClock.clientSend = FrameLimiter.Instance.GetTimeSinceGameStartup();

               // Debug.LogError($"On Initiator Send IsA={BufferStruct.SyncClock.isActive}," +
               //$"t0 = {BufferStruct.SyncClock.initiatorSend}, t1={BufferStruct.SyncClock.clientReceive}" +
               //$"t2 ={BufferStruct.SyncClock.clientSend}, t3={BufferStruct.SyncClock.initiatorReceive} " +
               //$"wait={BufferStruct.SyncClock.waitForHost}"
               //);
            }
        }


        //Serialziign and sending the buffer
        buffBytes = RawSerialize(BufferStruct);
        byteCount = System.Runtime.InteropServices.Marshal.SizeOf(BufferStruct);
        // Try to convert to send async
        if (sendArgs == null)
        {
            sendArgs = new SocketAsyncEventArgs();
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SendEventArg_Completed);
            sendArgs.RemoteEndPoint = IPEndPoint;
            sendArgs.SocketFlags = SocketFlags.None;
        }


       
        sendArgs.SetBuffer(buffBytes, 0, byteCount);


       
    }
  

     void Update()
    {
        ++_waitedFrames;
        if (!sender.Connected)
        {
            return;
        }
      
        if (Input.GetKeyDown(KeyCode.M))
        {
            toggleSend = true;
            _waitedFrames = 0;
        }

        if (toggleSend && ClientData.Instance.TwoWayConnectionEstablished())
        {
            ++_syncClockFramePassed;
        }
        //Senbd packet when send is toggle -- happens on game start
        //Or send packages if other peer still has not connected
        //to our local listener
        if (toggleSend /*|| !ClientData.Instance.TwoWayConnectionEstablished()*/)
        {
            
            if (sendFinished)
            {
                sendFinished = false;
                //Update the buffer to send

            
                UpdateBufferToSend();
                timesSend++;
               
                bool sendAll = !sender.SendAsync(sendArgs);
                  if (sendAll)
                  {
                      SendEventArg_Completed(this, sendArgs);
                  }
                 
            }
           
           
        }
    }


    void InitSenderSocket()
    {

        //Init Sender Socket
        sender = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Stream,
                                     ProtocolType.Tcp);
       // sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
        //Bind Sender socket
        IPAddress.TryParse("127.0.0.1", out hostIP);
        Debug.Log(hostIP.ToString());

     

        
    }

 
    public bool TrtConnectToPort(int portNum) 
    {
        try
        {
            IPEndPoint = new IPEndPoint(hostIP, portNum);
            sender.ConnectAsync(IPEndPoint).Wait();
            Debug.Log("Trying to connect to listener");
            ClientData.Instance.IsClientInitiator = !Listener.Instance.IsReceiverConnected();

            //send information for listener socket on this client and an empty buffer
            //do only once
            toggleSend = false;
            UpdateBufferToSend();
            timesSend++;
            bool sendAll = !sender.SendAsync(sendArgs);
            if (sendAll)
            {
                SendEventArg_Completed(this, sendArgs);
            }
            return sender.Connected;
        }
        catch (Exception e)
        {
            Debug.Log($"Trying to connect throwed exception {e}");

            return sender.Connected;
        }
       
    }

    

    public string GetEndPointPortNum() 
    {
        return this.sender.RemoteEndPoint.ToString();
    }

    public string GetLocalPortNum()
    {
        return this.sender.LocalEndPoint.ToString();
    }

  
   
    private static  Sender _instance;

    public static Sender Instance
    {
        get { return _instance; }
        set {
            if (_instance!=null)
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
        DontDestroyOnLoad(this);
        Debug.LogError($"InputBuffer {this.GetInstanceID()} is an active object ");
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
    // Convert an object to a byte array
   



    void SendEventArg_Completed(object sender, SocketAsyncEventArgs e)
    {
        sendFinished = true;
    }


    void OnApplicationQuit()
    {
        if (sender!=null)
        {
            if (sender.Connected)
            {

                try
                {
                    sender.Shutdown(SocketShutdown.Both);

                }
                finally
                {

                    sender.Close();
                }
                
                
            }
           
          
        }
        Debug.Log("Sender socket closed");
        Thread.Sleep(1000);
    }
}
