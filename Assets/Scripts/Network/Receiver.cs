//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Net.Sockets;
//using UnityEngine;


//public static class Receiver 
//{ 
//    static Socket _socket;
//    public static Socket Socket 
//    {
//        get {
//            if (_socket==null)
//            {
//                 InitReceiverSocket();
//            }
//            return _socket; 
//        }
//        set
//        {
//            _socket = value;
//        }
//    }


//    static void InitReceiverSocket()
//    {
//        //Init Receiver Socket to random free socket address
//        try
//        {
//            _socket = new Socket(AddressFamily.InterNetwork,
//                                    SocketType.Stream,
//                                    ProtocolType.Tcp);

//        }
//        catch (Exception e)
//        {

//            Debug.Log($"Couldnt set up Socket socket. Error {e.Message}");
//        }

//    }

//    public static void OnApplicationQuit()
//    {
//        if (Socket != null)
//        {
//            if (Socket.Connected)
//            {
//                try
//                {
//                    Socket.Shutdown(SocketShutdown.Both);

//                }
//                catch (Exception)
//                {

//                    Socket.Close();

//                }
//            }
//            //TODO check if Socket should be manually closed 

//            Debug.Log("Receiver socket closed");
//        }



//    }

//}
